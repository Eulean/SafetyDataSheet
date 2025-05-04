using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using SDS.Data;
using Microsoft.EntityFrameworkCore;
using SDS.Models; // For HTML parsing 

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

        [HttpGet("")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("Create")]
        public IActionResult Create()
        {
            // Get the CSRF tokens for the current request
            var tokens = _antiforgery.GetAndStoreTokens(HttpContext);

            // Pass the CSRF token to the view using ViewData
            ViewData["CSRFToken"] = tokens.RequestToken;

            return View("Make");
            // return View("Date");
        }


        // [HttpPost("save")]
        // public async Task<IActionResult> Save([FromBody] List<SDSContentItem> items)
        // {
        //     try
        //     {
        //         foreach (var item in items)
        //         {
        //             var sdsContent = new SDSContent
        //             {
        //                 // HeadersHDId = item.ContentID,
        //                 // Content = item.Content,

        //                 // // Auto-generate ProductId (example => P00001)
        //                 // ProductId = await GenerateProductIdAsync()
        //                 Content = item.Content,
        //                 ContentID = item.ContentID,    // <-- don’t forget this or you’ll get NULL
        //                 HeadersHId = item.HeadersHId.ToString(),
        //                 HeadersHDId = item.HeadersHDId.ToString(),
        //                 ProductId = await GenerateProductIdAsync()
        //             };
        //             _context.Add(sdsContent);
        //         }

        //         await _context.SaveChangesAsync();
        //         return Json(new { success = true });
        //     }
        //     catch (Exception ex)
        //     {
        //         return Json(new { success = false, message = ex.Message });
        //     }
        // }

        //copilot
        // [HttpPost("save")]
        // public async Task<IActionResult> Save([FromBody] SdsViewModel viewModel)
        // {
        //     try
        //     {
        //         // Step 1: Generate a new ProductId
        //         var productId = await GenerateProductIdAsync();

        //         // Step 2: Map and save SDSContent
        //         var sdsContents = MapFromViewModelToSDSContent(viewModel, productId);
        //         if (sdsContents.Any())
        //         {
        //             _context.SDSContents.AddRange(sdsContents);
        //         }

        //         // Step 3: Map and save HeaderHImage
        //         var headerHImages = MapFromViewModelToHeaderHImage(viewModel, productId);
        //         if (headerHImages.Any())
        //         {
        //             _context.HeaderHImages.AddRange(headerHImages);
        //         }

        //         // Step 4: Save changes to the database
        //         await _context.SaveChangesAsync();

        //         // Return success response with the generated ProductId
        //         return Json(new { success = true, message = "Data saved successfully.", productId });
        //     }
        //     catch (Exception ex)
        //     {
        //         // Return error response in case of an exception
        //         return Json(new { success = false, message = ex.Message });
        //     }
        // }

        //cody
        [HttpPost("save")]
        public async Task<IActionResult> Save([FromBody] SdsViewModel viewModel)
        {
            // // Validate the model
            // if (!ModelState.IsValid)
            // {
            //     return BadRequest(new { success = false, message = "Invalid data", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
            // }

            try
            {
                var products = await _context.Products.ToListAsync();

                var normalizedProductCode = NormalizeText(viewModel.ProductCode);
                var existingProduct = products.FirstOrDefault(p => NormalizeText(p.ProductCode) == normalizedProductCode);
                if (existingProduct != null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Product already exists!"
                    });
                }
                var productId = await GenerateProductIdAsync();

                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    var sdsContents = MapFromViewModelToSDSContent(viewModel, productId);

                    if (sdsContents.Any())
                    {
                        _context.SDSContents.AddRange(sdsContents);
                    }


                    var headerHImages = MapFromViewModelToHeaderHImage(viewModel, productId);
                    if (headerHImages.Any())
                    {
                        _context.HeaderHImages.AddRange(headerHImages);
                    }

                    var product = new Product
                    {
                        ProductCode = viewModel.ProductCode,
                        ProductNo = productId,
                        ProductName = viewModel.ProductName,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        DeletedAt = DateTime.Now,
                        IsDeleted = false
                    };

                    _context.Products.Add(product);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    _logger.LogInformation($"SDS data saved successfully for ProductId: {productId}");

                    return Json(new { success = true, message = "Data saved successfully.", productId });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, $"Error saving SDS data: {ex.Message}");


                return Json(new
                {
                    success = false,
                    message = "An error occurred while saving the data. Please try again or contact support."
                });
            }
        }


        public class SDSContentItem
        {
            public string ContentID { get; set; }
            public string Content { get; set; }
            public int HeadersHId { get; set; }
            public int HeadersHDId { get; set; }
        }

        [HttpGet("Error")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }


        #region Private Methods
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
                    if (image == null || image.ImageData == null || image.ImageData.Length == 0 || !IsImageFile(image.ImageName))
                    {
                        continue;
                    }

                    headerHImages.Add(new HeaderHImage
                    {
                        ContentID = contentId,
                        ProductId = productId,
                        ImageName = image.ImageName ?? "unnamed",
                        ContentType = image.ContentType ?? "application/octet-stream",
                        ImageData = image.ImageData,
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

            // Section 15: Regulatory Information
            AddIfNotEmpty("safetyRegulations", viewModel.SafetyRegulations);
            AddIfNotEmpty("chemicalSafetyAssessment", viewModel.ChemicalSafetyAssessment);

            // Section 16: Other Information
            AddIfNotEmpty("otherInformation", viewModel.OtherInformation);

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

        #endregion


        #region public mehods
        public SdsViewModel MapFromSDSContentToViewModel(List<SDSContent> sdsContents)
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

            // Section 15: Regulatory Information
            SetPropertyIfExists("safetyRegulations", value => viewModel.SafetyRegulations = value);
            SetPropertyIfExists("chemicalSafetyAssessment", value => viewModel.ChemicalSafetyAssessment = value);

            // Section 16: Other Information
            SetPropertyIfExists("otherInformation", value => viewModel.OtherInformation = value);

            return viewModel;
        }

        public void MapFromHeaderHImageToViewModel(List<HeaderHImage> headerHImages, SdsViewModel viewModel)
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
                        Order = image.Order
                    });
                }
            }
        }

        public async Task<SdsViewModel> GetSdsViewModelByProductIdAsync(string productId)
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
        #endregion

    }


}