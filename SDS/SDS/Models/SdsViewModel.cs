using System;

namespace SDS.Models
{
    public class SdsViewModel
    {
        public int Id { get; set; }
        public string? ProductId { get; set; }
        public string? ProductCode { get; set; }
        public string? ProductName { get; set; }
        public string? ProductImage { get; set; }
        public string? BiologicalDefinition { get; set; }
        public string? InciName { get; set; }
        public string? CasNumber { get; set; }
        public string? FemaNumber { get; set; }
        public string? EinecsNumber { get; set; }
        public string? IdentifiedUses { get; set; }
        public string? SupplierDetails { get; set; }
        public string? EmergencyPhone { get; set; }
        public string? ClassificationContent { get; set; }
        public string? LabelContent { get; set; }
        public string? SignalWord { get; set; }
        public string? ContainsInfo { get; set; }
        public string? HazardStatements { get; set; }
        public string? OtherHazards { get; set; }
        public string? SubstancesContent { get; set; }
        public string? InhalationFirstAid { get; set; }
        public string? IngestionFirstAid { get; set; }
        public string? SkinContactFirstAid { get; set; }
        public string? EyeContactFirstAid { get; set; }
        public string? SymptomsEffects { get; set; }
        public string? MedicalAttention { get; set; }
        public string? ExtinguishingMediaContent { get; set; }
        public string? SpecialHazardsContent { get; set; }
        public string? FirefighterAdviceContent { get; set; }
        public string? PersonalPrecautions { get; set; }
        public string? EnvironmentalPrecautions { get; set; }
        public string? ContainmentMethods { get; set; }
        public string? SectionReferences { get; set; }
        public string? SafeHandlingPrecautions { get; set; }
        public string? SafeStorageConditions { get; set; }
        public string? SpecificEndUses { get; set; }
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
        public string? ReactivityInfo { get; set; }
        public string? ChemicalStability { get; set; }
        public string? HazardousReactions { get; set; }
        public string? ConditionsToAvoid { get; set; }
        public string? IncompatibleMaterials { get; set; }
        public string? HazardousDecomposition { get; set; }
        public string? ToxicologicalEffects { get; set; }
        public string? EcoToxicity { get; set; }
        public string? PersistenceDegradability { get; set; }
        public string? BioaccumulationPotential { get; set; }
        public string? SoilMobility { get; set; }
        public string? PbtAssessment { get; set; }
        public string? OtherAdverseEffects { get; set; }
        public string? WasteTreatmentMethod { get; set; }
        public string? UnRoad { get; set; }
        public string? UnSea { get; set; }
        public string? UnAir { get; set; }
        public string? ShippingName { get; set; }
        public string? HazardClass { get; set; }
        public string? PackingGroup { get; set; }
        public string? EnvironmentalHazards { get; set; }
        public string? SafetyRegulations { get; set; }
        public string? ChemicalSafetyAssessment { get; set; }
        public string? OtherInformation { get; set; }

        // New property to hold images for each ContentID
        public Dictionary<string, List<HeaderHImage>> ImagesByContentID { get; set; } = new Dictionary<string, List<HeaderHImage>>();
    }
}
