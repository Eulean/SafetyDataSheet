using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SDS.Data;
using SDS.Models;

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
        
        [HttpGet("productTable")]
        public ActionResult ProductTableView()
        {
            return View("ProductTableDesign");
        }

        [HttpGet("")]
        // GET: GeneratePdfController
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet("View/{productId}")]
        public async Task<IActionResult> View(string productId) /// make suer to change some so that u can call form the page
        {
            try
            {
                if (string.IsNullOrEmpty(productId))
                {
                    return BadRequest("Product ID is required");
                }

                // Get the complete ViewModel for this ProductId
                var viewModel = await GetSdsViewModelByProductIdAsync(productId);

                // Check if any data was found
                if (viewModel == null || string.IsNullOrEmpty(viewModel.ProductId))
                {
                    return NotFound($"No SDS data found for Product ID: {productId}");
                }

                // Return the view with the populated ViewModel
                return View(viewModel);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error");
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
