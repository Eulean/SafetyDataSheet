using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using SDS.Data;
using SDS.Helper;
using SDS.Models;
using System.Text.RegularExpressions;

namespace SDS.Controllers
{
    [Route("[controller]")]
    public class GeneratePdfController : Controller
    {

        private readonly SdsDbContext _context;
        private readonly IAntiforgery _antiforgery;
        private readonly IWebHostEnvironment _env;

        // Combined constructor that takes both dependencies
        public GeneratePdfController(SdsDbContext context,
            IAntiforgery antiforgery,
            IWebHostEnvironment env)
        {
            _context = context;
            _antiforgery = antiforgery;
            _env = env;
        }

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet("Generate/{productNo}")]
        public async Task<IActionResult> Generate(string productNo)
        {
            try
            {
                if (string.IsNullOrEmpty(productNo))
                {
                    return BadRequest("Product ID is required.");
                }

                var vm = await GetSdsViewModelByProductIdAsync(productNo);
                if (vm == null || string.IsNullOrEmpty(vm.ProductId))
                {
                    return NotFound($"Product {productNo}  not found.");
                }

                // Generate URL to PDF template endpoint
                var pdfHtmlUrl = Url.Action("Pdf", "GeneratePdf", new { productNo }, Request.Scheme);

                if (string.IsNullOrEmpty(pdfHtmlUrl))
                {
                    return BadRequest("Failed to generate PDF template URL");
                }

                // Configure PDF options
                var now = DateTime.Now;
                var imgsrc = ImageLogo.Prooli 
                    ?? Functions.GetBase64Image("prooil.jpg", Path.Combine(_env.WebRootPath, "images"));

                var pdfOptions = new PdfOptions
                {
                    Format = PaperFormat.A4,
                    PrintBackground = true,
                    DisplayHeaderFooter = true,
                    MarginOptions = new MarginOptions
                    {
                        Top = "25mm",
                        Bottom = "40mm",
                    },
                    HeaderTemplate = @$"
                    <div style='width:100%;font-size: 11px; border-bottom: 1px solid #333; 
                        padding-bottom: 6mm; line-height: 1.5; position: relative;'>
                        <div style='display: flex; justify-content: space-between; align-items: center; margin: 0 5mm;'>
                            <div>
                                <strong style='font-size: 12px;'>PRO-OILS AROMATHERAPY</strong><br>
                                MATERIAL SAFETY DATA SHEET
                            </div>
                            <div style='position: absolute; width: 150px; height: 45px; overflow: hidden; left: 50%; top: 40%; transform: translate(-50%, -50%)  ;'>
                              <img src='{imgsrc}'
                                   style='position: absolute; top: 50%; left: 50%; transform: translate(-50%, -50%); width: 100%; height: 100%; object-fit: cover;'/>
                            </div>
                            <div>
                                Page <span class='pageNumber'></span> of <span class='totalPages'></span>
                            </div>
                        </div>
                    </div>",
                    FooterTemplate = @$"
                    <div style='width:100%; font-size:9px; color:#222; border-top:1px solid #333; padding-top:3mm; line-height:1.6; font-family:Arial,sans-serif;'>
                        <div style='display:grid; grid-template-columns:1fr 1fr; gap:8mm; margin:0 5mm 2mm;'>
                            <div style='line-height:1.4;'>
                                <strong style='display:block; margin-bottom:1mm;'>PRO-OILS AROMATHERAPY</strong>
                                 <div style='margin-top:1mm;'>Tel: {Functions.RemoveHtmlTags(vm.EmergencyPhone)}</div>
                                <div >info@prooils.com.au</div>
                                <div>Unit 6, 163 Newbridge Road,Chipping Norton NSW 2170 </div>                               
                            </div>
                            <div style='text-align:right; line-height:1.4;'>
                                <strong style='display:block; margin-bottom:3mm;'>REVISION DETAILS</strong>
                                <div>
                                <p style='display:block;'> Date: {vm.RevisionDate ?? DateTime.Now:yyyy-MM-dd}</p>
                                 <p style='display:block; white-space: nowrap;'> Rev No: {Functions.RemoveHtmlTags(vm.RevNo)}</p>
                                </div>
                            </div>
                        </div>
                        <div style='font-size:10px; font-weight:600; text-align:center; margin-top:2mm; padding-bottom:1mm;'>
                            PAGE <span class='pageNumber'></span> OF <span class='totalPages'></span>
                        </div>
                    </div>"
                };

                // Generate PDF using helper
                var pdfBytes = await HtmlToPdfGenerateHelper.GenerateAsync(pdfHtmlUrl, pdfOptions);

                // Return PDF file
                return File(pdfBytes, "application/pdf", $"{productNo}_{(vm.RevisionDate ?? DateTime.Now):yyyy-mm-dd}.pdf");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                // Log error here
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("Pdf/{productNo}")]
        public async Task<IActionResult> Pdf(string productNo)
        {
            try
            {
                var vm = await GetSdsViewModelByProductIdAsync(productNo);
                if (vm == null) return NotFound();
                return View("~/Views/GeneratePdf/GeneratePdf.cshtml", vm);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
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

        //private void MapFromHeaderHImageToViewModel(List<HeaderHImage> headerHImages, SdsViewModel viewModel)
        //{
        //    if (headerHImages == null || !headerHImages.Any())
        //    {
        //        return;
        //    }

        //    // Group images by ContentID
        //    var imagesByContentId = headerHImages.GroupBy(img => img.ContentID);

        //    foreach (var group in imagesByContentId)
        //    {
        //        string contentId = group.Key;

        //        // Skip if contentId is null or empty
        //        if (string.IsNullOrEmpty(contentId))
        //        {
        //            continue;
        //        }

        //        // Create a list for this contentId if it doesn't exist
        //        if (!viewModel.ImagesByContentID.ContainsKey(contentId))
        //        {
        //            viewModel.ImagesByContentID[contentId] = new List<HeaderHImage>();
        //        }

        //        // Add each image to the appropriate list, ordered by the Order property
        //        foreach (var image in group.OrderBy(img => img.Order))
        //        {
        //            viewModel.ImagesByContentID[contentId].Add(new HeaderHImage
        //            {
        //                Id = image.Id,
        //                ContentID = image.ContentID,
        //                ProductId = image.ProductId,
        //                ImageName = image.ImageName,
        //                ContentType = image.ContentType,
        //                ImageData = image.ImageData,
        //                Base64Image = image.Base64Image != null ? Convert.ToBase64String(image.ImageData) : null,
        //                Order = image.Order
        //            });
        //        }
        //    }
        //}
        private void MapFromHeaderHImageToViewModel(List<HeaderHImage> headerHImages, SdsViewModel viewModel)
        {
            if (headerHImages == null || !headerHImages.Any()) return;

            var contentGroups = headerHImages
                .Where(img => !string.IsNullOrEmpty(img.ContentID))
                .GroupBy(img => img.ContentID)
                .ToDictionary(g => g.Key,
                    g => g.OrderBy(img => img.Order)
                                    .Select(img => new HeaderHImage
                                    {
                                        Id = img.Id,
                                        ContentID = img.ContentID,
                                        ProductId = img.ProductId,
                                        ImageName = img.ImageName,
                                        ContentType = img.ContentType,
                                        ImageData = img.ImageData,
                                        Base64Image = img.Base64Image != null
                                            ? Convert.ToBase64String(img.ImageData)
                                            : null,
                                        Order = img.Order
                                    })
                                    .ToList());

            viewModel.ImagesByContentID = contentGroups;
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