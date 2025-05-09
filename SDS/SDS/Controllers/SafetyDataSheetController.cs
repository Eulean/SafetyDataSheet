using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SDS.Data;
using SDS.Models;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using SDS.Helpers; // For HTML parsing 


namespace SDS.Controllers
{
    [Route("[controller]")]
    public class SafetyDataSheetController : Controller
    {
        private readonly SdsDbContext _context;
        private readonly IAntiforgery _antiforgery;

        private readonly ILogger<SafetyDataSheetController> _logger;


        // Combined constructor that takes both dependencies
        public SafetyDataSheetController(SdsDbContext context, IAntiforgery antiforgery, ILogger<SafetyDataSheetController> logger)
        {
            _context = context;
            _antiforgery = antiforgery;
            _logger = logger;
        }

        [HttpGet("Index")]
        public async Task<IActionResult> Index(int page = 1)
        {
            int pageSize = 20;
            int skip = (page - 1) * pageSize;

            var totalItems = await _context.Products.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var products = await _context.Products
                .Skip(skip)
                .Take(pageSize)
                .Select(p => new ProductViewModel
                {
                    ProductNo = p.ProductNo,
                    ProductCode = p.ProductCode,
                    ProductName = p.ProductName,
                })
                .ToListAsync();

            var model = new ProductListViewModel
            {
                Products = products,
                CurrentPage = page,
                TotalPages = totalPages,
                PageSize = pageSize,
                TotalItems = totalItems
            };

            return View(model);
        }


        [HttpGet("Create")]
        public async Task<IActionResult> Create(string id = null)
        {
            // Get the CSRF tokens for the current request
            var tokens = _antiforgery.GetAndStoreTokens(HttpContext);
            ViewData["CSRFToken"] = tokens.RequestToken;

            var model = new SdsViewModel();

            if (!string.IsNullOrEmpty(id))
            {
                // Edit mode - load existing data
                model = await GetSdsViewModelByProductIdAsync(id);
                ViewBag.IsEdit = true;
            }
            else
            {
                // Create mode
                ViewBag.IsEdit = false;
            }

            return View("Make", model); // ← make sure model is passed to the view
        }

        // // GET: SafetyDataSheet/Delete/
        // public async Task<IActionResult> Delete(string? id)
        // {
        //     if (id == null)
        //         return BadRequest();

        //     var product = await _context.Products
        //         .FirstOrDefaultAsync(p => p.ProductNo == id);

        //     if (product == null)
        //         return NotFound();

        //     return View(product); // Show confirmation page
        // }

        // // POST: SafetyDataSheet/Delete/
        // [HttpPost, ActionName("Delete")]
        // [ValidateAntiForgeryToken]
        // public async Task<IActionResult> DeleteConfirmed(string id)
        // {
        //     var product = await _context.Products
        //         .FirstOrDefaultAsync(p => p.ProductNo == id);

        //     if (product == null)
        //         return NotFound();

        //     _context.Products.Remove(product);
        //     await _context.SaveChangesAsync();

        //     return RedirectToAction(nameof(Index));
        // }

        [HttpPost("save")]
        public async Task<IActionResult> Save([FromBody] SdsViewModel viewModel)
        {
            try
            {
                if (string.IsNullOrEmpty(viewModel.ProductName))
                {
                    return BadRequest(new { success = false, message = "Product Name is required." });
                }

                var normalizedProductCode = NormalizeText(viewModel.ProductCode);
                var normalizeProductName = NormalizeText(viewModel.ProductName);

                // Determine if this is an update or create
                bool isUpdate = !string.IsNullOrEmpty(viewModel.ProductId);
                var productId = isUpdate ? viewModel.ProductId : await GenerateProductIdAsync();

                var products = await _context.Products.ToListAsync();
                var existingProduct = products.FirstOrDefault(p => p.ProductNo == viewModel.ProductId);

                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    if (isUpdate)
                    {

                        if (existingProduct == null)
                        {
                            return NotFound(new { success = false, message = "Product not found" });
                        }

                        // Update existing product
                        existingProduct.ProductName = normalizeProductName;
                        existingProduct.ProductCode = normalizedProductCode;
                        existingProduct.UpdatedAt = DateTime.Now;
                        _context.Products.Update(existingProduct);
                    }
                    else
                    {
                        if (existingProduct != null)
                        {
                            return BadRequest(new { success = false, message = "Product already exists!" });
                        }

                        var product = new Product
                        {
                            ProductCode = normalizedProductCode,
                            ProductNo = productId,
                            ProductName = normalizeProductName,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = null,
                            DeletedAt = null,
                            IsDeleted = false
                        };
                        _context.Products.Add(product);
                    }

                    // Remove existing content if updating
                    if (isUpdate)
                    {
                        var existingContents = await _context.SDSContents
                            .Where(c => c.ProductId == viewModel.ProductId)
                            .ToListAsync();

                        _context.SDSContents.RemoveRange(existingContents);

                        // var existingImages = await _context.HeaderHImages
                        //     .Where(i => i.ProductId == viewModel.ProductId)
                        //     .ToListAsync();

                        // _context.HeaderHImages.RemoveRange(existingImages);
                        var product = new Product
                        {
                            ProductCode = normalizedProductCode,
                            ProductNo = productId,
                            // ProductName = normalizedProductName,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now,
                            DeletedAt = DateTime.Now,
                            IsDeleted = false
                        };

                        // Get all existing images for this product
                        var existingImages = await _context.HeaderHImages
                            .Where(i => i.ProductId == viewModel.ProductId)
                            .ToListAsync();

                        // List of ContentIds that might have images
                        var imageContentIds = new List<string>
                        {
                            "productImage",
                            "labelImage",
                            "protectiveEquipmentImage",
                            "hazardClassImage",
                            "environmentalHazardsImage"
                        };

                        // List to track images that need to be removed
                        var imagesToRemove = new List<HeaderHImage>();

                        if (viewModel.ImagesByContentID != null)
                        {
                            foreach (var contentId in imageContentIds)
                            {
                                // Check if this contentId has images in the viewModel
                                if (viewModel.ImagesByContentID.ContainsKey(contentId) &&
                                    viewModel.ImagesByContentID[contentId] != null &&
                                    viewModel.ImagesByContentID[contentId].Any())
                                {
                                    // Find existing images with this contentId and add them to removal list
                                    var imagesForThisContentId = existingImages
                                        .Where(img => img.ContentID == contentId)
                                        .ToList();

                                    imagesToRemove.AddRange(imagesForThisContentId);
                                }
                            }
                        }

                        // Remove the identified images
                        if (imagesToRemove.Any())
                        {
                            _context.HeaderHImages.RemoveRange(imagesToRemove);
                        }
                    }

                    // Add new content
                    var sdsContents = MapFromViewModelToSDSContent(viewModel, productId);
                    var headerHImages = MapFromViewModelToHeaderHImage(viewModel, productId);

                    sdsContents = ApplyTimestamps(sdsContents, isUpdate);

                    if (sdsContents.Any())
                    {
                        _context.SDSContents.AddRange(sdsContents);
                    }

                    if (headerHImages.Any())
                    {
                        _context.HeaderHImages.AddRange(headerHImages);
                    }

                    // reopen if u want the database to horizontal 
                    // var sdsModel = SdsMapper.MapToSdsModel(viewModel);
                    // sdsModel.ProductId = productId;
                    // // sdsModel.ProductName = normalizeProductName;
                    // // sdsModel.ProductCode = normalizedProductCode;
                    // _context.SdsModels.Add(sdsModel);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    _logger.LogInformation($"SDS data {(isUpdate ? "updated" : "saved")} successfully for ProductId: {(isUpdate ? viewModel.ProductId : productId)}");

                    return Ok(new
                    {
                        success = true,
                        message = $"Data {(isUpdate ? "updated" : "saved")} successfully.",
                        productId = viewModel.ProductId
                    });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, $"Error {(isUpdate ? "updating" : "saving")} SDS data: {ex.Message}");
                    return StatusCode(500, new
                    {
                        success = false,
                        message = $"An error occurred while {(isUpdate ? "updating" : "saving")} the data."
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing SDS data: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while processing the data. Please try again or contact support."
                });
            }
        }


        [HttpGet("Error")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }


        #region Private Methods

        private List<SDSContent> ApplyTimestamps(List<SDSContent> sDSContents, bool isUpdate)
        {
            var now = DateTime.UtcNow;
            foreach (var content in sDSContents)
            {
                if (isUpdate)
                {
                    content.UpdatedAt = now;

                    if (content.IsDeleted)
                    {
                        content.DeletedAt = null;
                        content.IsDeleted = false;
                    }
                }
                else
                {
                    content.CreatedAt = now;
                    content.IsDeleted = false;
                }
            }

            return sDSContents;
        }
        private bool IsImageFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return false;
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".svg", ".webp" };
            var fileExtension = Path.GetExtension(fileName)?.ToLowerInvariant();
            return allowedExtensions.Contains(fileExtension);
        }

        private string NormalizeText(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            // Remove HTML tags using Regex
            return Regex.Replace(input, "<.*?>", string.Empty).Trim();
        }
        private List<HeaderHImage> MapFromViewModelToHeaderHImage(SdsViewModel viewModel, string productId)
        {
            var headerHImages = new List<HeaderHImage>();
            const int MaxImageSize = 5 * 1024 * 1024; // 5MB

            if (viewModel.ImagesByContentID == null || !viewModel.ImagesByContentID.Any())
            {
                return headerHImages; // Return empty list if no images
            }

            foreach (var contentId in viewModel.ImagesByContentID.Keys)
            {
                if (string.IsNullOrEmpty(contentId))
                {
                    continue; // Skip entries with null or empty contentId
                }

                var images = viewModel.ImagesByContentID[contentId];
                if (images == null)
                {
                    continue; // Skip if the image list is null
                }

                foreach (var image in images)
                {
                    if (image == null ||
                        image.ImageData == null ||
                        image.ImageData.Length == 0 ||
                        !IsImageFile(image.ImageName) ||
                        image.ImageData.Length > MaxImageSize)
                    {
                        continue;
                    }

                    string base64Image = Convert.ToBase64String(image.ImageData);

                    headerHImages.Add(new HeaderHImage
                    {
                        ContentID = contentId,
                        ProductId = productId,
                        ImageName = image.ImageName ?? "unnamed",
                        ContentType = image.ContentType ?? "application/octet-stream",
                        ImageData = image.ImageData,
                        Base64Image = base64Image,

                        Order = Math.Max(1, Math.Min(5, image.Order))
                    });
                }
            }

            return headerHImages;
        }


        private List<SDSContent> MapFromViewModelToSDSContent(SdsViewModel viewModel, string productId)
        {
            var sdsContents = new List<SDSContent>();

            // Helper method to reduce repetition
            void AddIfNotEmpty(string contentId, string content)
            {
                if (!string.IsNullOrEmpty(content))
                {
                    sdsContents.Add(new SDSContent
                    {
                        ContentID = contentId,
                        Content = content,
                        ProductId = productId
                    });
                }
            }

            // Section 1: Identification
            AddIfNotEmpty("productCode", viewModel.ProductCode);
            AddIfNotEmpty("productName", viewModel.ProductName);
            AddIfNotEmpty("productImage", viewModel.ProductImage);
            AddIfNotEmpty("biologicalDefinition", viewModel.BiologicalDefinition);
            AddIfNotEmpty("inciName", viewModel.InciName);
            AddIfNotEmpty("casNumber", viewModel.CasNumber);
            AddIfNotEmpty("femaNumber", viewModel.FemaNumber);
            AddIfNotEmpty("einecsNumber", viewModel.EinecsNumber);
            AddIfNotEmpty("identifiedUses", viewModel.IdentifiedUses);
            AddIfNotEmpty("supplierDetails", viewModel.SupplierDetails);
            AddIfNotEmpty("emergencyPhone", viewModel.EmergencyPhone);

            // Section 2: Hazard Identification
            AddIfNotEmpty("classificationContent", viewModel.ClassificationContent);
            AddIfNotEmpty("labelContent", viewModel.LabelContent);
            AddIfNotEmpty("signalWord", viewModel.SignalWord);
            AddIfNotEmpty("containsInfo", viewModel.ContainsInfo);
            AddIfNotEmpty("hazardStatements", viewModel.HazardStatements);
            AddIfNotEmpty("precautionary", viewModel.Precautionary); // new
            AddIfNotEmpty("supplementary", viewModel.Supplementary); // new
            AddIfNotEmpty("otherHazards", viewModel.OtherHazards);

            // Section 3: Composition
            AddIfNotEmpty("substancesContent", viewModel.SubstancesContent);

            // Section 4: First Aid
            AddIfNotEmpty("inhalationFirstAid", viewModel.InhalationFirstAid);
            AddIfNotEmpty("ingestionFirstAid", viewModel.IngestionFirstAid);
            AddIfNotEmpty("skinContactFirstAid", viewModel.SkinContactFirstAid);
            AddIfNotEmpty("eyeContactFirstAid", viewModel.EyeContactFirstAid);
            AddIfNotEmpty("symptomsEffects", viewModel.SymptomsEffects);
            AddIfNotEmpty("medicalAttention", viewModel.MedicalAttention);

            // Section 5: Firefighting
            AddIfNotEmpty("extinguishingMediaContent", viewModel.ExtinguishingMediaContent);
            AddIfNotEmpty("specialHazardsContent", viewModel.SpecialHazardsContent);
            AddIfNotEmpty("firefighterAdviceContent", viewModel.FirefighterAdviceContent);

            // Section 6: Accidental Release
            AddIfNotEmpty("personalPrecautions", viewModel.PersonalPrecautions);
            AddIfNotEmpty("environmentalPrecautions", viewModel.EnvironmentalPrecautions);
            AddIfNotEmpty("containmentMethods", viewModel.ContainmentMethods);
            AddIfNotEmpty("sectionReferences", viewModel.SectionReferences);

            // Section 7: Handling and Storage
            AddIfNotEmpty("safeHandlingPrecautions", viewModel.SafeHandlingPrecautions);
            AddIfNotEmpty("safeStorageConditions", viewModel.SafeStorageConditions);
            AddIfNotEmpty("specificEndUses", viewModel.SpecificEndUses);

            // Section 8: Exposure Controls/Personal Protection
            AddIfNotEmpty("controlParameters", viewModel.ControlParameters); // new
            AddIfNotEmpty("protectiveEquipmentImage", viewModel.ProtectiveEquipmentImage);
            AddIfNotEmpty("processConditions", viewModel.ProcessConditions);
            AddIfNotEmpty("engineeringMeasures", viewModel.EngineeringMeasures);
            AddIfNotEmpty("respiratoryEquipment", viewModel.RespiratoryEquipment);
            AddIfNotEmpty("handProtection", viewModel.HandProtection);
            AddIfNotEmpty("eyeProtection", viewModel.EyeProtection);
            AddIfNotEmpty("otherProtection", viewModel.OtherProtection);
            AddIfNotEmpty("hygieneMeasures", viewModel.HygieneMeasures);
            AddIfNotEmpty("personalProtection", viewModel.PersonalProtection);
            AddIfNotEmpty("skinProtection", viewModel.SkinProtection);
            AddIfNotEmpty("environmentalExposure", viewModel.EnvironmentalExposure);

            // Section 9: Physical and Chemical Properties
            AddIfNotEmpty("appearance", viewModel.Appearance);
            AddIfNotEmpty("colour", viewModel.Colour);
            AddIfNotEmpty("odour", viewModel.Odour);
            AddIfNotEmpty("relativeDensity", viewModel.RelativeDensity);
            AddIfNotEmpty("flashPoint", viewModel.FlashPoint);
            AddIfNotEmpty("meltingPoint", viewModel.MeltingPoint);
            AddIfNotEmpty("refractiveIndex", viewModel.RefractiveIndex);
            AddIfNotEmpty("boilingPoint", viewModel.BoilingPoint);
            AddIfNotEmpty("vapourPressure", viewModel.VapourPressure);
            AddIfNotEmpty("solubilityInWater", viewModel.SolubilityInWater);
            AddIfNotEmpty("autoIgnitionTemp", viewModel.AutoIgnitionTemp);
            AddIfNotEmpty("otherChemicalInfo", viewModel.OtherChemicalInfo);

            // Section 10: Stability and Reactivity
            AddIfNotEmpty("reactivityInfo", viewModel.ReactivityInfo);
            AddIfNotEmpty("chemicalStability", viewModel.ChemicalStability);
            AddIfNotEmpty("hazardousReactions", viewModel.HazardousReactions);
            AddIfNotEmpty("conditionsToAvoid", viewModel.ConditionsToAvoid);
            AddIfNotEmpty("incompatibleMaterials", viewModel.IncompatibleMaterials);
            AddIfNotEmpty("hazardousDecomposition", viewModel.HazardousDecomposition);

            // Section 11: Toxicological Information
            AddIfNotEmpty("toxicologicalEffects", viewModel.ToxicologicalEffects);
            AddIfNotEmpty("acuteToxicity", viewModel.AcuteToxicity); // new
            AddIfNotEmpty("skinCorrosion", viewModel.SkinCorrosion); // new
            AddIfNotEmpty("eyeDamage", viewModel.EyeDamage); // new
            AddIfNotEmpty("skinSensitization", viewModel.SkinSensitization); // new
            AddIfNotEmpty("germCell", viewModel.GermCell); // new
            AddIfNotEmpty("carcinogenicity", viewModel.Carcinogenicity); // new
            AddIfNotEmpty("reproductiveToxicity", viewModel.ReproductiveToxicity); // new
            AddIfNotEmpty("singleExposure", viewModel.SingleExposure); // new
            AddIfNotEmpty("repeatedExposure", viewModel.RepeatedExposure); // new
            AddIfNotEmpty("aspirationHazard", viewModel.AspirationHazard); // new
            AddIfNotEmpty("photoToxicity", viewModel.PhotoToxicity); // new
            AddIfNotEmpty("otherInfo", viewModel.OtherInfo); // new

            // Section 12: Ecological Information
            AddIfNotEmpty("ecoToxicity", viewModel.EcoToxicity);
            AddIfNotEmpty("persistenceDegradability", viewModel.PersistenceDegradability);
            AddIfNotEmpty("bioaccumulationPotential", viewModel.BioaccumulationPotential);
            AddIfNotEmpty("soilMobility", viewModel.SoilMobility);
            AddIfNotEmpty("pbtAssessment", viewModel.PbtAssessment);
            AddIfNotEmpty("otherAdverseEffects", viewModel.OtherAdverseEffects);

            // Section 13: Disposal Considerations
            AddIfNotEmpty("wasteTreatmentMethod", viewModel.WasteTreatmentMethod);

            // Section 14: Transport Information
            AddIfNotEmpty("unRoad", viewModel.UnRoad);
            AddIfNotEmpty("unSea", viewModel.UnSea);
            AddIfNotEmpty("unAir", viewModel.UnAir);
            AddIfNotEmpty("shippingName", viewModel.ShippingName);
            AddIfNotEmpty("hazardClass", viewModel.HazardClass);
            AddIfNotEmpty("packingGroup", viewModel.PackingGroup);
            AddIfNotEmpty("environmentalHazards", viewModel.EnvironmentalHazards);
            AddIfNotEmpty("specialPrecautions", viewModel.SpecialPrecautions); // new
            AddIfNotEmpty("bulkTranprt", viewModel.BulkTranprt); // new
            AddIfNotEmpty("ibcCode", viewModel.IbcCode); // new

            // Section 15: Regulatory Information
            AddIfNotEmpty("safetyRegulations", viewModel.SafetyRegulations);
            AddIfNotEmpty("chemicalSafetyAssessment", viewModel.ChemicalSafetyAssessment);

            // Section 16: Other Information
            AddIfNotEmpty("otherInformation", viewModel.OtherInformation);
            AddIfNotEmpty("precautionaryStatements", viewModel.PrecautionaryStatements); // new
            AddIfNotEmpty("revisionDate", viewModel.RevisionDate?.ToString("yyyy-MM-dd")); // new
            AddIfNotEmpty("revisionReason", viewModel.RevisionReason); // new
            AddIfNotEmpty("revNo", viewModel.RevNo);  // new

            return sdsContents;
        }


        private async Task<string> GenerateProductIdAsync()
        {
            // Generate a GUID-based unique identifier
            var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper(); // Take the first 8 characters of the GUID

            // Check if there are existing entries to maintain a numeric sequence
            var lastId = await _context.SDSContents
                .OrderByDescending(s => s.Id)
                .Select(s => s.ProductId)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(lastId))
            {
                // Start with P00001 and append the GUID-based unique identifier
                return $"P00001-{uniqueId}";
            }
            else
            {
                // Extract the numeric part of the last ProductId (assuming format P00001-XXXXXXX)
                var numericPart = lastId.Split('-')[0].Substring(1); // Get the numeric part after "P"
                var lastNumericId = int.Parse(numericPart);

                // Increment the numeric part and append the GUID-based unique identifier
                return $"P{(lastNumericId + 1).ToString("D5")}-{uniqueId}";
            }
        }

        private SdsViewModel MapFromSDSContentToViewModel(List<SDSContent> sdsContents)
        {
            var viewModel = new SdsViewModel();

            if (sdsContents == null || !sdsContents.Any())
            {
                return viewModel;
            }

            // Extract ProductId from the first content item (should be the same for all)
            viewModel.ProductId = sdsContents.FirstOrDefault()?.ProductId;

            // Helper method to reduce repetition
            void SetPropertyIfExists(string contentId, Action<string> setter)
            {
                var content = sdsContents.FirstOrDefault(c => c.ContentID == contentId)?.Content;
                if (!string.IsNullOrEmpty(content))
                {
                    setter(content);
                }
            }

            // Section 1: Identification
            SetPropertyIfExists("productCode", value => viewModel.ProductCode = value);
            SetPropertyIfExists("productName", value => viewModel.ProductName = value);
            SetPropertyIfExists("productImage", value => viewModel.ProductImage = value);
            SetPropertyIfExists("biologicalDefinition", value => viewModel.BiologicalDefinition = value);
            SetPropertyIfExists("inciName", value => viewModel.InciName = value);
            SetPropertyIfExists("casNumber", value => viewModel.CasNumber = value);
            SetPropertyIfExists("femaNumber", value => viewModel.FemaNumber = value);
            SetPropertyIfExists("einecsNumber", value => viewModel.EinecsNumber = value);
            SetPropertyIfExists("identifiedUses", value => viewModel.IdentifiedUses = value);
            SetPropertyIfExists("supplierDetails", value => viewModel.SupplierDetails = value);
            SetPropertyIfExists("emergencyPhone", value => viewModel.EmergencyPhone = value);

            // Section 2: Hazard Identification
            SetPropertyIfExists("classificationContent", value => viewModel.ClassificationContent = value);
            SetPropertyIfExists("labelContent", value => viewModel.LabelContent = value);
            SetPropertyIfExists("signalWord", value => viewModel.SignalWord = value);
            SetPropertyIfExists("containsInfo", value => viewModel.ContainsInfo = value);
            SetPropertyIfExists("hazardStatements", value => viewModel.HazardStatements = value);
            SetPropertyIfExists("precautionary", value => viewModel.Precautionary = value); // new
            SetPropertyIfExists("supplementary", value => viewModel.Supplementary = value); // new
            SetPropertyIfExists("otherHazards", value => viewModel.OtherHazards = value);

            // Section 3: Composition
            SetPropertyIfExists("substancesContent", value => viewModel.SubstancesContent = value);

            // Section 4: First Aid
            SetPropertyIfExists("inhalationFirstAid", value => viewModel.InhalationFirstAid = value);
            SetPropertyIfExists("ingestionFirstAid", value => viewModel.IngestionFirstAid = value);
            SetPropertyIfExists("skinContactFirstAid", value => viewModel.SkinContactFirstAid = value);
            SetPropertyIfExists("eyeContactFirstAid", value => viewModel.EyeContactFirstAid = value);
            SetPropertyIfExists("symptomsEffects", value => viewModel.SymptomsEffects = value);
            SetPropertyIfExists("medicalAttention", value => viewModel.MedicalAttention = value);

            // Section 5: Firefighting
            SetPropertyIfExists("extinguishingMediaContent", value => viewModel.ExtinguishingMediaContent = value);
            SetPropertyIfExists("specialHazardsContent", value => viewModel.SpecialHazardsContent = value);
            SetPropertyIfExists("firefighterAdviceContent", value => viewModel.FirefighterAdviceContent = value);

            // Section 6: Accidental Release
            SetPropertyIfExists("personalPrecautions", value => viewModel.PersonalPrecautions = value);
            SetPropertyIfExists("environmentalPrecautions", value => viewModel.EnvironmentalPrecautions = value);
            SetPropertyIfExists("containmentMethods", value => viewModel.ContainmentMethods = value);
            SetPropertyIfExists("sectionReferences", value => viewModel.SectionReferences = value);

            // Section 7: Handling and Storage
            SetPropertyIfExists("safeHandlingPrecautions", value => viewModel.SafeHandlingPrecautions = value);
            SetPropertyIfExists("safeStorageConditions", value => viewModel.SafeStorageConditions = value);
            SetPropertyIfExists("specificEndUses", value => viewModel.SpecificEndUses = value);

            // Section 8: Exposure Controls/Personal Protection
            SetPropertyIfExists("controlParameters", value => viewModel.ControlParameters = value); // new
            SetPropertyIfExists("protectiveEquipmentImage", value => viewModel.ProtectiveEquipmentImage = value);
            SetPropertyIfExists("processConditions", value => viewModel.ProcessConditions = value);
            SetPropertyIfExists("engineeringMeasures", value => viewModel.EngineeringMeasures = value);
            SetPropertyIfExists("respiratoryEquipment", value => viewModel.RespiratoryEquipment = value);
            SetPropertyIfExists("handProtection", value => viewModel.HandProtection = value);
            SetPropertyIfExists("eyeProtection", value => viewModel.EyeProtection = value);
            SetPropertyIfExists("otherProtection", value => viewModel.OtherProtection = value);
            SetPropertyIfExists("hygieneMeasures", value => viewModel.HygieneMeasures = value);
            SetPropertyIfExists("personalProtection", value => viewModel.PersonalProtection = value);
            SetPropertyIfExists("skinProtection", value => viewModel.SkinProtection = value);
            SetPropertyIfExists("environmentalExposure", value => viewModel.EnvironmentalExposure = value);

            // Section 9: Physical and Chemical Properties
            SetPropertyIfExists("appearance", value => viewModel.Appearance = value);
            SetPropertyIfExists("colour", value => viewModel.Colour = value);
            SetPropertyIfExists("odour", value => viewModel.Odour = value);
            SetPropertyIfExists("relativeDensity", value => viewModel.RelativeDensity = value);
            SetPropertyIfExists("flashPoint", value => viewModel.FlashPoint = value);
            SetPropertyIfExists("meltingPoint", value => viewModel.MeltingPoint = value);
            SetPropertyIfExists("refractiveIndex", value => viewModel.RefractiveIndex = value);
            SetPropertyIfExists("boilingPoint", value => viewModel.BoilingPoint = value);
            SetPropertyIfExists("vapourPressure", value => viewModel.VapourPressure = value);
            SetPropertyIfExists("solubilityInWater", value => viewModel.SolubilityInWater = value);
            SetPropertyIfExists("autoIgnitionTemp", value => viewModel.AutoIgnitionTemp = value);
            SetPropertyIfExists("otherChemicalInfo", value => viewModel.OtherChemicalInfo = value);

            // Section 10: Stability and Reactivity
            SetPropertyIfExists("reactivityInfo", value => viewModel.ReactivityInfo = value);
            SetPropertyIfExists("chemicalStability", value => viewModel.ChemicalStability = value);
            SetPropertyIfExists("hazardousReactions", value => viewModel.HazardousReactions = value);
            SetPropertyIfExists("conditionsToAvoid", value => viewModel.ConditionsToAvoid = value);
            SetPropertyIfExists("incompatibleMaterials", value => viewModel.IncompatibleMaterials = value);
            SetPropertyIfExists("hazardousDecomposition", value => viewModel.HazardousDecomposition = value);

            // Section 11: Toxicological Information
            SetPropertyIfExists("toxicologicalEffects", value => viewModel.ToxicologicalEffects = value);
            SetPropertyIfExists("acuteToxicity", value => viewModel.AcuteToxicity = value); //new
            SetPropertyIfExists("skinCorrosion", value => viewModel.SkinCorrosion = value); //new
            SetPropertyIfExists("eyeDamage", value => viewModel.EyeDamage = value); //new
            SetPropertyIfExists("skinSensitization", value => viewModel.SkinSensitization = value); //new
            SetPropertyIfExists("germCell", value => viewModel.GermCell = value); //new
            SetPropertyIfExists("carcinogenicity", value => viewModel.Carcinogenicity = value); //new
            SetPropertyIfExists("reproductiveToxicity", value => viewModel.ReproductiveToxicity = value); //new
            SetPropertyIfExists("singleExposure", value => viewModel.SingleExposure = value); //new
            SetPropertyIfExists("repeatedExposure", value => viewModel.RepeatedExposure = value); //new
            SetPropertyIfExists("aspirationHazard", value => viewModel.AspirationHazard = value); //new
            SetPropertyIfExists("photoToxicity", value => viewModel.PhotoToxicity = value); //new
            SetPropertyIfExists("otherInfo", value => viewModel.OtherInfo = value); //new


            // Section 12: Ecological Information
            SetPropertyIfExists("ecoToxicity", value => viewModel.EcoToxicity = value);
            SetPropertyIfExists("persistenceDegradability", value => viewModel.PersistenceDegradability = value);
            SetPropertyIfExists("bioaccumulationPotential", value => viewModel.BioaccumulationPotential = value);
            SetPropertyIfExists("soilMobility", value => viewModel.SoilMobility = value);
            SetPropertyIfExists("pbtAssessment", value => viewModel.PbtAssessment = value);
            SetPropertyIfExists("otherAdverseEffects", value => viewModel.OtherAdverseEffects = value);

            // Section 13: Disposal Considerations
            SetPropertyIfExists("wasteTreatmentMethod", value => viewModel.WasteTreatmentMethod = value);

            // Section 14: Transport Information
            SetPropertyIfExists("unRoad", value => viewModel.UnRoad = value);
            SetPropertyIfExists("unSea", value => viewModel.UnSea = value);
            SetPropertyIfExists("unAir", value => viewModel.UnAir = value);
            SetPropertyIfExists("shippingName", value => viewModel.ShippingName = value);
            SetPropertyIfExists("hazardClass", value => viewModel.HazardClass = value);
            SetPropertyIfExists("packingGroup", value => viewModel.PackingGroup = value);
            SetPropertyIfExists("environmentalHazards", value => viewModel.EnvironmentalHazards = value);
            SetPropertyIfExists("specialPrecautions", value => viewModel.SpecialPrecautions = value); // new
            SetPropertyIfExists("bulkTranprt", value => viewModel.BulkTranprt = value); // new
            SetPropertyIfExists("ibcCode", value => viewModel.IbcCode = value); // new


            // Section 15: Regulatory Information
            SetPropertyIfExists("safetyRegulations", value => viewModel.SafetyRegulations = value);
            SetPropertyIfExists("chemicalSafetyAssessment", value => viewModel.ChemicalSafetyAssessment = value);

            // Section 16: Other Information
            SetPropertyIfExists("otherInformation", value => viewModel.OtherInformation = value);
            SetPropertyIfExists("precautionaryStatements", value => viewModel.PrecautionaryStatements = value); // new
            SetPropertyIfExists("revisionDate", value =>
            {
                if (DateTime.TryParseExact(value, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var parsedDate)) // new
                {
                    viewModel.RevisionDate = parsedDate;
                }
                else
                {
                    viewModel.RevisionDate = null;
                }
            });
            SetPropertyIfExists("revisionReason", value => viewModel.RevisionReason = value); // new
            SetPropertyIfExists("revNo", value => viewModel.RevNo = value); // new

            return viewModel;
        }

        private void MapFromHeaderHImageToViewModel(List<HeaderHImage> headerHImages, SdsViewModel viewModel)
        {
            if (headerHImages == null || !headerHImages.Any())
            {
                return;
            }

            // Group images by ContentID
            var imagesByContentId = headerHImages.GroupBy(img => img.ContentID);

            foreach (var group in imagesByContentId)
            {
                string contentId = group.Key;

                // Skip if contentId is null or empty
                if (string.IsNullOrEmpty(contentId))
                {
                    continue;
                }

                // Create a list for this contentId if it doesn't exist
                if (!viewModel.ImagesByContentID.ContainsKey(contentId))
                {
                    viewModel.ImagesByContentID[contentId] = new List<HeaderHImage>();
                }

                // Add each image to the appropriate list, ordered by the Order property
                foreach (var image in group.OrderBy(img => img.Order))
                {
                    viewModel.ImagesByContentID[contentId].Add(new HeaderHImage
                    {
                        Id = image.Id,
                        ContentID = image.ContentID,
                        ProductId = image.ProductId,
                        ImageName = image.ImageName,
                        ContentType = image.ContentType,
                        ImageData = image.ImageData,
                        Base64Image = image.Base64Image != null ? Convert.ToBase64String(image.ImageData) : null,
                        Order = image.Order
                    });
                }
            }
        }

        private async Task<SdsViewModel> GetSdsViewModelByProductIdAsync(string productId)
        {
            // Retrieve all SDSContent items for this ProductId
            var sdsContents = await _context.SDSContents
                .Where(c => c.ProductId == productId)
                .ToListAsync();

            // Map SDSContent items to the ViewModel
            var viewModel = MapFromSDSContentToViewModel(sdsContents);

            // Retrieve all HeaderHImage items for this ProductId
            var headerHImages = await _context.HeaderHImages
                .Where(img => img.ProductId == productId)
                .ToListAsync();

            // Map HeaderHImage items to the ViewModel
            MapFromHeaderHImageToViewModel(headerHImages, viewModel);

            // Retrieve product information if needed
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductNo == productId);

            if (product != null)
            {
                // Update any additional product-specific properties if needed
                viewModel.ProductCode = product.ProductCode;
                viewModel.ProductName = product.ProductName;
            }

            return viewModel;
        }
        #endregion

    }


}