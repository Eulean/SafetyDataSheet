using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PuppeteerSharp.Media;
using PuppeteerSharp;
using SDS.Data;
using SDS.Helper;
using SDS.Models;
using Microsoft.CodeAnalysis;

namespace SDS.Controllers
{
    [Route("[controller]")]
    public class GeneratePdfController : Controller
    {

        private readonly SdsDbContext _context;
        private readonly IAntiforgery _antiforgery;

        // Combined constructor that takes both dependencies
        public GeneratePdfController(SdsDbContext context, IAntiforgery antiforgery)
        {
            _context = context;
            _antiforgery = antiforgery;
        }

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet("Pdf/{productId}")]
        public async Task<IActionResult> Pdf(string productNo)
        {
            var vm = await GetSdsViewModelByProductIdAsync(productNo);
            if (vm == null) return NotFound();

            return View("~/Views/GeneratePdf/GeneratePdf.cshtml", vm);
        }


        // // TODO : Delete after viewModel is successfully work
        // [HttpGet("TestReview")]
        // public ActionResult TestReview()
        // {
        //     return View("~/Views/GeneratePdf/GeneratePdf.cshtml");
        // }
        // // TODO : Delete after viewModel is successfully work
        // [HttpGet("Test")]
        // public async Task<ActionResult> Test()
        // {
        //     try
        //     {
        //         // Generate URL to PDF template endpoint
        //         var pdfHtmlUrl = Url.Action("TestReview", "GeneratePdf", null, Request.Scheme);

        //         // Configure PDF options
        //         var pdfOptions = new PdfOptions
        //         {
        //             Format = PaperFormat.A4,
        //             PrintBackground = true,
        //             DisplayHeaderFooter = true,
        //             MarginOptions = new MarginOptions
        //             {
        //                 Top = "25mm",
        //                 Bottom = "35mm",
        //             },
        //             HeaderTemplate = @"
        //             <div style='width:100%;font-size: 11px; border-bottom: 1px solid #333; 
        //                         padding-bottom: 3mm; line-height: 1.5;'>
        //                 <div style='display: flex; justify-content: space-between; 
        //                             align-items: center; margin: 0 5mm;'>
        //                     <div>
        //                         <strong style='font-size: 12px;'>THE INGREDIENT WAREHOUSE</strong><br>
        //                         MATERIAL SAFETY DATA SHEET
        //                     </div>
        //                     <div>
        //                         Page <span class='pageNumber'></span> of <span class='totalPages'></span>
        //                     </div>
        //                 </div>
        //             </div>",
        //             FooterTemplate = @"
        //             <div style='width:100%;font-size: 9px; color: #222; border-top: 1px solid #333;
        //                         padding-top: 3mm; line-height: 1.6;'>
        //                 <div style='display: grid; grid-template-columns: 1fr 1fr; gap: 8mm; margin: 0 5mm;'>
        //                     <div style='line-height: 1.6;'>
        //                         <strong>THE INGREDIENT WAREHOUSE</strong><br>
        //                         73 National Avenue, Pakenham VIC 3810<br>
        //                         AUSTRALIA<br>
        //                         Tel: 03 5940 8920 (Int +61 3 5940 8920)
        //                     </div>
        //                     <div style='text-align: right; line-height: 1.6;'>
        //                         <strong>REVISION DETAILS</strong><br>
        //                         Date: 07/08/2023<br>
        //                         Rev No: SDS Generated 02
        //                     </div>
        //                 </div>
        //                 <div style='font-size: 10px; font-weight: 600; 
        //                             text-align: center; margin-top: 3mm;'>
        //                     PAGE <span class='pageNumber'></span> OF <span class='totalPages'></span>
        //                 </div>
        //             </div>"
        //         };
        //         // Generate PDF using helper
        //         var pdfBytes = await HtmlToPdfGenerateHelper.GenerateAsync(pdfHtmlUrl, pdfOptions);

        //         // Return PDF file
        //         return File(pdfBytes, "application/pdf", $"SDS.pdf");
        //     }
        //     catch (ArgumentException ex)
        //     {
        //         return BadRequest(ex.Message);
        //     }
        //     catch (KeyNotFoundException ex)
        //     {
        //         return NotFound(ex.Message);
        //     }
        //     catch (Exception ex)
        //     {
        //         // Log error here
        //         return StatusCode(500, "Internal server error");
        //     }
        // }

        [HttpGet("GeneratePdf/{productId}")]
        public async Task<IActionResult> GeneratePdf(string productId)
        {
            try
            {
                if (string.IsNullOrEmpty(productId))
                {
                    return BadRequest("Product ID is required.");
                }

                var vm = await GetSdsViewModelByProductIdAsync(productId);
                if (vm == null || string.IsNullOrEmpty(vm.ProductId))
                {
                    return NotFound($"Product {productId}  not found.");
                }

                // Generate URL to PDF template endpoint
                var pdfHtmlUrl = Url.Action("PdfHtml", "GeneratePdf", new { productId }, Request.Scheme);

                // Configure PDF options
                var pdfOptions = new PdfOptions
                {
                    Format = PaperFormat.A4,
                    PrintBackground = true,
                    DisplayHeaderFooter = true,
                    MarginOptions = new MarginOptions
                    {
                        Top = "25mm",
                        Bottom = "35mm",
                    },
                    HeaderTemplate = @"
                    <div style='width:100%;font-size: 11px; border-bottom: 1px solid #333; 
                                padding-bottom: 3mm; line-height: 1.5;'>
                        <div style='display: flex; justify-content: space-between; 
                                    align-items: center; margin: 0 5mm;'>
                            <div>
                                <strong style='font-size: 12px;'>THE INGREDIENT WAREHOUSE</strong><br>
                                MATERIAL SAFETY DATA SHEET
                            </div>
                            <div>
                                Page <span class='pageNumber'></span> of <span class='totalPages'></span>
                            </div>
                        </div>
                    </div>",
                    FooterTemplate = @"
                    <div style='width:100%;font-size: 9px; color: #222; border-top: 1px solid #333;
                                padding-top: 3mm; line-height: 1.6;'>
                        <div style='display: grid; grid-template-columns: 1fr 1fr; gap: 8mm; margin: 0 5mm;'>
                            <div style='line-height: 1.6;'>
                                <strong>THE INGREDIENT WAREHOUSE</strong><br>
                                73 National Avenue, Pakenham VIC 3810<br>
                                AUSTRALIA<br>
                                Tel: 03 5940 8920 (Int +61 3 5940 8920)
                            </div>
                            <div style='text-align: right; line-height: 1.6;'>
                                <strong>REVISION DETAILS</strong><br>
                                Date: 07/08/2023<br>
                                Rev No: SDS Generated 02
                            </div>
                        </div>
                        <div style='font-size: 10px; font-weight: 600; 
                                    text-align: center; margin-top: 3mm;'>
                            PAGE <span class='pageNumber'></span> OF <span class='totalPages'></span>
                        </div>
                    </div>"
                };

                // Generate PDF using helper
                var pdfBytes = await HtmlToPdfGenerateHelper.GenerateAsync(pdfHtmlUrl, pdfOptions);

                // Return PDF file
                return File(pdfBytes, "application/pdf", $"{productId}-SDS.pdf");
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

        [HttpGet("PdfHtml/{productId}")]
        public async Task<IActionResult> PdfHtml(string productId)
        {
            try
            {
                var viewModel = await GetSdsViewModelByProductIdAsync(productId);
                return View("~/Views/GeneratePdf/GeneratePdf.cshtml", viewModel);
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
        #endregion

        [HttpGet("aidMeasurement")]
        public ActionResult AidMeasurementView()
        {
            return View("AidMeasurementDesign");
        }
    }
}
