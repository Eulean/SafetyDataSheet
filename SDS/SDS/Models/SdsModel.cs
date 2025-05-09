using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SDS.Models
{
    public class SdsModel
    {
        public int Id { get; set; }
        public string? ProductId { get; set; }
        // Section 1: Identification
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string? ProductImage { get; set; }
        public string? BiologicalDefinition { get; set; }
        public string? InciName { get; set; }
        public string? CasNumber { get; set; }
        public string? FemaNumber { get; set; }
        public string? EinecsNumber { get; set; }
        public string? IdentifiedUses { get; set; }
        public string? SupplierDetails { get; set; }
        public string? EmergencyPhone { get; set; }
        // Section 2: Hazard Identification
        public string? ClassificationContent { get; set; }
        public string? LabelContent { get; set; }
        public string? SignalWord { get; set; }
        public string? ContainsInfo { get; set; }
        public string? HazardStatements { get; set; }
        public string? Precautionary { get; set; } // new
        public string? Supplementary { get; set; } // new
        public string? OtherHazards { get; set; }
        // Section 3: Composition
        public string? SubstancesContent { get; set; }
        // Section 4: First Aid
        public string? InhalationFirstAid { get; set; }
        public string? IngestionFirstAid { get; set; }
        public string? SkinContactFirstAid { get; set; }
        public string? EyeContactFirstAid { get; set; }
        public string? SymptomsEffects { get; set; }
        public string? MedicalAttention { get; set; }
        // Section 5: Firefighting
        public string? ExtinguishingMediaContent { get; set; }
        public string? SpecialHazardsContent { get; set; }
        public string? FirefighterAdviceContent { get; set; }
        // Section 6: Accidental Release
        public string? PersonalPrecautions { get; set; }
        public string? EnvironmentalPrecautions { get; set; }
        public string? ContainmentMethods { get; set; }
        public string? SectionReferences { get; set; }
        // Section 7: Handling and Storage
        public string? SafeHandlingPrecautions { get; set; }
        public string? SafeStorageConditions { get; set; }
        public string? SpecificEndUses { get; set; }
        // Section 8: Exposure Controls/Personal Protection
        public string? ControlParameters { get; set; } // new
        public string? ProtectiveEquipmentImage { get; set; }
        public string? ProcessConditions { get; set; }
        public string? EngineeringMeasures { get; set; }
        public string? RespiratoryEquipment { get; set; }
        public string? HandProtection { get; set; }
        public string? EyeProtection { get; set; }
        public string? OtherProtection { get; set; }
        public string? HygieneMeasures { get; set; }
        public string? PersonalProtection { get; set; }
        public string? SkinProtection { get; set; }
        public string? EnvironmentalExposure { get; set; }
        // Section 9: Physical and Chemical Properties
        public string? Appearance { get; set; }
        public string? Colour { get; set; }
        public string? Odour { get; set; }
        public string? RelativeDensity { get; set; }
        public string? FlashPoint { get; set; }
        public string? MeltingPoint { get; set; }
        public string? RefractiveIndex { get; set; }
        public string? BoilingPoint { get; set; }
        public string? VapourPressure { get; set; }
        public string? SolubilityInWater { get; set; }
        public string? AutoIgnitionTemp { get; set; }
        public string? OtherChemicalInfo { get; set; }
        // Section 10: Stability and Reactivity
        public string? ReactivityInfo { get; set; }
        public string? ChemicalStability { get; set; }
        public string? HazardousReactions { get; set; }
        public string? ConditionsToAvoid { get; set; }
        public string? IncompatibleMaterials { get; set; }
        public string? HazardousDecomposition { get; set; }
        // Section 11: Toxicological Information
        public string? ToxicologicalEffects { get; set; }
        public string? AcuteToxicity { get; set; } // new
        public string? SkinCorrosion { get; set; } // new
        public string? EyeDamage { get; set; } // new
        public string? SkinSensitization { get; set; } // new
        public string? GermCell { get; set; } // new
        public string? Carcinogenicity { get; set; } // new
        public string? ReproductiveToxicity { get; set; } // new
        public string? SingleExposure { get; set; } // new
        public string? RepeatedExposure { get; set; } // new
        public string? AspirationHazard { get; set; } // new
        public string? PhotoToxicity { get; set; } // new
        public string? OtherInfo { get; set; } // new
        // Section 12: Ecological Information
        public string? EcoToxicity { get; set; }
        public string? PersistenceDegradability { get; set; }
        public string? BioaccumulationPotential { get; set; }
        public string? SoilMobility { get; set; }
        public string? PbtAssessment { get; set; }
        public string? OtherAdverseEffects { get; set; }
        // Section 13: Disposal Considerations
        public string? WasteTreatmentMethod { get; set; }
        // Section 14: Transport Information
        public string? UnRoad { get; set; }
        public string? UnSea { get; set; }
        public string? UnAir { get; set; }
        public string? ShippingName { get; set; }
        public string? HazardClass { get; set; }
        public string? PackingGroup { get; set; }
        public string? EnvironmentalHazards { get; set; }
        public string? SpecialPrecautions { get; set; } // new
        public string? BulkTranprt { get; set; } // new
        public string? IbcCode { get; set; } // new
        // Section 15: Regulatory Information
        public string? SafetyRegulations { get; set; }
        public string? ChemicalSafetyAssessment { get; set; }
        // Section 16: Other Information
        public string? OtherInformation { get; set; } // new
        public string? PrecautionaryStatements { get; set; } // new
        public DateTime? RevisionDate { get; set; } // new
        public string? RevisionReason { get; set; } // new
        public string? RevNo { get; set; } // new

        // New property to hold images for each ContentID
        // public Dictionary<string, List<HeaderHImage>> ImagesByContentID { get; set; } = new Dictionary<string, List<HeaderHImage>>();
        // public virtual ICollection<HeaderHImage> Images { get; set; }
    }

}