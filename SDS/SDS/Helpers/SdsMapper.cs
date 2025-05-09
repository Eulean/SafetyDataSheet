using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SDS.Models;

namespace SDS.Helpers
{
    public static class SdsMapper
    {
        public static SdsModel MapToSdsModel(SdsViewModel model)
        {
            return new SdsModel
            {
                Id = model.Id,
                ProductId = model.ProductId,
                ProductCode = model.ProductCode!,
                ProductName = model.ProductName!,
                ProductImage = model.ProductImage,
                BiologicalDefinition = model.BiologicalDefinition,
                InciName = model.InciName,
                CasNumber = model.CasNumber,
                FemaNumber = model.FemaNumber,
                EinecsNumber = model.EinecsNumber,
                IdentifiedUses = model.IdentifiedUses,
                SupplierDetails = model.SupplierDetails,
                EmergencyPhone = model.EmergencyPhone,

                ClassificationContent = model.ClassificationContent,
                LabelContent = model.LabelContent,
                SignalWord = model.SignalWord,
                ContainsInfo = model.ContainsInfo,
                HazardStatements = model.HazardStatements,
                Precautionary = model.Precautionary,
                Supplementary = model.Supplementary,
                OtherHazards = model.OtherHazards,

                SubstancesContent = model.SubstancesContent,

                InhalationFirstAid = model.InhalationFirstAid,
                IngestionFirstAid = model.IngestionFirstAid,
                SkinContactFirstAid = model.SkinContactFirstAid,
                EyeContactFirstAid = model.EyeContactFirstAid,
                SymptomsEffects = model.SymptomsEffects,
                MedicalAttention = model.MedicalAttention,

                ExtinguishingMediaContent = model.ExtinguishingMediaContent,
                SpecialHazardsContent = model.SpecialHazardsContent,
                FirefighterAdviceContent = model.FirefighterAdviceContent,

                PersonalPrecautions = model.PersonalPrecautions,
                EnvironmentalPrecautions = model.EnvironmentalPrecautions,
                ContainmentMethods = model.ContainmentMethods,
                SectionReferences = model.SectionReferences,

                SafeHandlingPrecautions = model.SafeHandlingPrecautions,
                SafeStorageConditions = model.SafeStorageConditions,
                SpecificEndUses = model.SpecificEndUses,

                ControlParameters = model.ControlParameters,
                ProtectiveEquipmentImage = model.ProtectiveEquipmentImage,
                ProcessConditions = model.ProcessConditions,
                EngineeringMeasures = model.EngineeringMeasures,
                RespiratoryEquipment = model.RespiratoryEquipment,
                HandProtection = model.HandProtection,
                EyeProtection = model.EyeProtection,
                OtherProtection = model.OtherProtection,
                HygieneMeasures = model.HygieneMeasures,
                PersonalProtection = model.PersonalProtection,
                SkinProtection = model.SkinProtection,
                EnvironmentalExposure = model.EnvironmentalExposure,

                Appearance = model.Appearance,
                Colour = model.Colour,
                Odour = model.Odour,
                RelativeDensity = model.RelativeDensity,
                FlashPoint = model.FlashPoint,
                MeltingPoint = model.MeltingPoint,
                RefractiveIndex = model.RefractiveIndex,
                BoilingPoint = model.BoilingPoint,
                VapourPressure = model.VapourPressure,
                SolubilityInWater = model.SolubilityInWater,
                AutoIgnitionTemp = model.AutoIgnitionTemp,
                OtherChemicalInfo = model.OtherChemicalInfo,

                ReactivityInfo = model.ReactivityInfo,
                ChemicalStability = model.ChemicalStability,
                HazardousReactions = model.HazardousReactions,
                ConditionsToAvoid = model.ConditionsToAvoid,
                IncompatibleMaterials = model.IncompatibleMaterials,
                HazardousDecomposition = model.HazardousDecomposition,

                ToxicologicalEffects = model.ToxicologicalEffects,
                AcuteToxicity = model.AcuteToxicity,
                SkinCorrosion = model.SkinCorrosion,
                EyeDamage = model.EyeDamage,
                SkinSensitization = model.SkinSensitization,
                GermCell = model.GermCell,
                Carcinogenicity = model.Carcinogenicity,
                ReproductiveToxicity = model.ReproductiveToxicity,
                SingleExposure = model.SingleExposure,
                RepeatedExposure = model.RepeatedExposure,
                AspirationHazard = model.AspirationHazard,
                PhotoToxicity = model.PhotoToxicity,
                OtherInfo = model.OtherInfo,

                EcoToxicity = model.EcoToxicity,
                PersistenceDegradability = model.PersistenceDegradability,
                BioaccumulationPotential = model.BioaccumulationPotential,
                SoilMobility = model.SoilMobility,
                PbtAssessment = model.PbtAssessment,
                OtherAdverseEffects = model.OtherAdverseEffects,

                WasteTreatmentMethod = model.WasteTreatmentMethod,

                UnRoad = model.UnRoad,
                UnSea = model.UnSea,
                UnAir = model.UnAir,
                ShippingName = model.ShippingName,
                HazardClass = model.HazardClass,
                PackingGroup = model.PackingGroup,
                EnvironmentalHazards = model.EnvironmentalHazards,
                SpecialPrecautions = model.SpecialPrecautions,
                BulkTranprt = model.BulkTranprt,
                IbcCode = model.IbcCode,

                SafetyRegulations = model.SafetyRegulations,
                ChemicalSafetyAssessment = model.ChemicalSafetyAssessment,

                OtherInformation = model.OtherInformation,
                PrecautionaryStatements = model.PrecautionaryStatements,
                RevisionDate = model.RevisionDate,
                RevisionReason = model.RevisionReason,
                RevNo = model.RevNo
            };
        }
    }
}