using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SDS.Data;
using SDS.Models;
using SDS.Models.Dto;

namespace SDS.Services
{
    public class SdsService
    {
        private readonly SdsDbContext _context;

        public SdsService(SdsDbContext context)
        {
            _context = context;
        }

        // A better approach to render formatted text with proper underlines
        // private void RenderSimpleFormattedText(TextDescriptor text, string htmlContent)
        // {
        //     if (string.IsNullOrWhiteSpace(htmlContent))
        //     {
        //         text.Span("No additional data available.").Italic();
        //         return;
        //     }

        //     // Clean the HTML to remove complex formatting
        //     var cleanedHtml = htmlContent
        //         .Replace("\r\n", " ")
        //         .Replace("\n", " ");

        //     // Process the HTML content
        //     var processedContent = ProcessHtmlContent(cleanedHtml);

        //     // Render the processed content
        //     foreach (var segment in processedContent)
        //     {
        //         var textSpan = text.Span(segment.Text);

        //         if (segment.IsBold)
        //             textSpan.Bold();

        //         if (segment.IsItalic)
        //             textSpan.Italic();

        //         if (segment.IsUnderlined)
        //             textSpan.StyledText(style => style.Underline());
        //     }
        // }

        // // Helper class to represent a formatted text segment
        // private class TextSegment
        // {
        //     public string Text { get; set; }
        //     public bool IsBold { get; set; }
        //     public bool IsItalic { get; set; }
        //     public bool IsUnderlined { get; set; }
        // }

        // // Process HTML content and extract formatted segments
        // private List<TextSegment> ProcessHtmlContent(string html)
        // {
        //     var result = new List<TextSegment>();

        //     try
        //     {
        //         // Extract bold text
        //         ProcessFormattedText(html, @"<(b|strong)>(.*?)</\1>", result, isBold: true);

        //         // Extract italic text
        //         ProcessFormattedText(html, @"<(i|em)>(.*?)</\1>", result, isItalic: true);

        //         // Extract underlined text
        //         ProcessFormattedText(html, @"<u>(.*?)</u>", result, isUnderlined: true);

        //         // Extract remaining plain text
        //         var plainText = Regex.Replace(html, @"<(b|strong|i|em|u)>.*?</\1>", "");
        //         plainText = Regex.Replace(plainText, "<.*?>", " ").Trim();

        //         if (!string.IsNullOrWhiteSpace(plainText))
        //         {
        //             result.Add(new TextSegment
        //             {
        //                 Text = plainText,
        //                 IsBold = false,
        //                 IsItalic = false,
        //                 IsUnderlined = false
        //             });
        //         }
        //     }
        //     catch
        //     {
        //         // Fallback if processing fails
        //         result.Add(new TextSegment
        //         {
        //             Text = Regex.Replace(html, "<.*?>", " ").Trim(),
        //             IsBold = false,
        //             IsItalic = false,
        //             IsUnderlined = false
        //         });
        //     }

        //     return result;
        // }

        // // Helper method to process formatted text
        // private void ProcessFormattedText(string html, string pattern, List<TextSegment> segments,
        //                                  bool isBold = false, bool isItalic = false, bool isUnderlined = false)
        // {
        //     var matches = Regex.Matches(html, pattern, RegexOptions.IgnoreCase);
        //     foreach (Match match in matches)
        //     {
        //         var content = Regex.Replace(match.Groups[2].Value, "<.*?>", "").Trim();
        //         if (!string.IsNullOrWhiteSpace(content))
        //         {
        //             segments.Add(new TextSegment
        //             {
        //                 Text = content,
        //                 IsBold = isBold,
        //                 IsItalic = isItalic,
        //                 IsUnderlined = isUnderlined
        //             });
        //         }
        //     }
        // }


        public List<SDSContent> ApplyTimestamps(List<SDSContent> sDSContents, bool isUpdate)
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

        public bool IsImageFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return false;
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".svg", ".webp" };
            var fileExtension = Path.GetExtension(fileName)?.ToLowerInvariant();
            return allowedExtensions.Contains(fileExtension);
        }

        public string NormalizeText(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            // Remove HTML tags using Regex
            return Regex.Replace(input, "<.*?>", string.Empty).Trim();
        }

        public List<HeaderHImage> MapFromViewModelToHeaderHImage(SdsViewModel viewModel, string productId)
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

        public List<SDSContent> MapFromViewModelToSDSContent(SdsViewModel viewModel, string productId)
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
            AddIfNotEmpty(SdsContentName.ProductCode, viewModel.ProductCode);
            AddIfNotEmpty(SdsContentName.ProductName, viewModel.ProductName);
            AddIfNotEmpty(SdsContentName.ProductImage, viewModel.ProductImage);
            AddIfNotEmpty(SdsContentName.BiologicalDefinition, viewModel.BiologicalDefinition);
            AddIfNotEmpty(SdsContentName.InciName, viewModel.InciName);
            AddIfNotEmpty(SdsContentName.CasNumber, viewModel.CasNumber);
            AddIfNotEmpty(SdsContentName.FemaNumber, viewModel.FemaNumber);
            AddIfNotEmpty(SdsContentName.EinecsNumber, viewModel.EinecsNumber);
            AddIfNotEmpty(SdsContentName.IdentifiedUses, viewModel.IdentifiedUses);
            AddIfNotEmpty(SdsContentName.SupplierDetails, viewModel.SupplierDetails);
            AddIfNotEmpty(SdsContentName.EmergencyPhone, viewModel.EmergencyPhone);

            // Section 2: Hazard Identification
            AddIfNotEmpty(SdsContentName.ClassificationContent, viewModel.ClassificationContent);
            AddIfNotEmpty(SdsContentName.LabelContent, viewModel.LabelContent);
            AddIfNotEmpty(SdsContentName.SignalWord, viewModel.SignalWord);
            AddIfNotEmpty(SdsContentName.ContainsInfo, viewModel.ContainsInfo);
            AddIfNotEmpty(SdsContentName.HazardStatements, viewModel.HazardStatements);
            AddIfNotEmpty(SdsContentName.Precautionary, viewModel.Precautionary); // new
            AddIfNotEmpty(SdsContentName.Supplementary, viewModel.Supplementary); // new
            AddIfNotEmpty(SdsContentName.OtherHazards, viewModel.OtherHazards);

            // Section 3: Composition
            AddIfNotEmpty(SdsContentName.SubstancesContent, viewModel.SubstancesContent);

            // Section 4: First Aid
            AddIfNotEmpty(SdsContentName.InhalationFirstAid, viewModel.InhalationFirstAid);
            AddIfNotEmpty(SdsContentName.IngestionFirstAid, viewModel.IngestionFirstAid);
            AddIfNotEmpty(SdsContentName.SkinContactFirstAid, viewModel.SkinContactFirstAid);
            AddIfNotEmpty(SdsContentName.EyeContactFirstAid, viewModel.EyeContactFirstAid);
            AddIfNotEmpty(SdsContentName.SymptomsEffects, viewModel.SymptomsEffects);
            AddIfNotEmpty(SdsContentName.MedicalAttention, viewModel.MedicalAttention);

            // Section 5: Firefighting
            AddIfNotEmpty(SdsContentName.ExtinguishingMediaContent, viewModel.ExtinguishingMediaContent);
            AddIfNotEmpty(SdsContentName.SpecialHazardsContent, viewModel.SpecialHazardsContent);
            AddIfNotEmpty(SdsContentName.FirefighterAdviceContent, viewModel.FirefighterAdviceContent);

            // Section 6: Accidental Release
            AddIfNotEmpty(SdsContentName.PersonalPrecautions, viewModel.PersonalPrecautions);
            AddIfNotEmpty(SdsContentName.EnvironmentalPrecautions, viewModel.EnvironmentalPrecautions);
            AddIfNotEmpty(SdsContentName.ContainmentMethods, viewModel.ContainmentMethods);
            AddIfNotEmpty(SdsContentName.SectionReferences, viewModel.SectionReferences);

            // Section 7: Handling and Storage
            AddIfNotEmpty(SdsContentName.SafeHandlingPrecautions, viewModel.SafeHandlingPrecautions);
            AddIfNotEmpty(SdsContentName.SafeStorageConditions, viewModel.SafeStorageConditions);
            AddIfNotEmpty(SdsContentName.SpecificEndUses, viewModel.SpecificEndUses);

            // Section 8: Exposure Controls/Personal Protection
            AddIfNotEmpty(SdsContentName.ControlParameters, viewModel.ControlParameters); // new
            AddIfNotEmpty(SdsContentName.ProtectiveEquipmentImage, viewModel.ProtectiveEquipmentImage);
            AddIfNotEmpty(SdsContentName.ProcessConditions, viewModel.ProcessConditions);
            AddIfNotEmpty(SdsContentName.EngineeringMeasures, viewModel.EngineeringMeasures);
            AddIfNotEmpty(SdsContentName.RespiratoryEquipment, viewModel.RespiratoryEquipment);
            AddIfNotEmpty(SdsContentName.HandProtection, viewModel.HandProtection);
            AddIfNotEmpty(SdsContentName.EyeProtection, viewModel.EyeProtection);
            AddIfNotEmpty(SdsContentName.OtherProtection, viewModel.OtherProtection);
            AddIfNotEmpty(SdsContentName.HygieneMeasures, viewModel.HygieneMeasures);
            AddIfNotEmpty(SdsContentName.PersonalProtection, viewModel.PersonalProtection);
            AddIfNotEmpty(SdsContentName.SkinProtection, viewModel.SkinProtection);
            AddIfNotEmpty(SdsContentName.EnvironmentalExposure, viewModel.EnvironmentalExposure);

            // Section 9: Physical and Chemical Properties
            AddIfNotEmpty(SdsContentName.Appearance, viewModel.Appearance);
            AddIfNotEmpty(SdsContentName.Colour, viewModel.Colour);
            AddIfNotEmpty(SdsContentName.Odour, viewModel.Odour);
            AddIfNotEmpty(SdsContentName.RelativeDensity, viewModel.RelativeDensity);
            AddIfNotEmpty(SdsContentName.FlashPoint, viewModel.FlashPoint);
            AddIfNotEmpty(SdsContentName.MeltingPoint, viewModel.MeltingPoint);
            AddIfNotEmpty(SdsContentName.RefractiveIndex, viewModel.RefractiveIndex);
            AddIfNotEmpty(SdsContentName.BoilingPoint, viewModel.BoilingPoint);
            AddIfNotEmpty(SdsContentName.VapourPressure, viewModel.VapourPressure);
            AddIfNotEmpty(SdsContentName.SolubilityInWater, viewModel.SolubilityInWater);
            AddIfNotEmpty(SdsContentName.AutoIgnitionTemp, viewModel.AutoIgnitionTemp);
            AddIfNotEmpty(SdsContentName.OtherChemicalInfo, viewModel.OtherChemicalInfo);

            // Section 10: Stability and Reactivity
            AddIfNotEmpty(SdsContentName.ReactivityInfo, viewModel.ReactivityInfo);
            AddIfNotEmpty(SdsContentName.ChemicalStability, viewModel.ChemicalStability);
            AddIfNotEmpty(SdsContentName.HazardousReactions, viewModel.HazardousReactions);
            AddIfNotEmpty(SdsContentName.ConditionsToAvoid, viewModel.ConditionsToAvoid);
            AddIfNotEmpty(SdsContentName.IncompatibleMaterials, viewModel.IncompatibleMaterials);
            AddIfNotEmpty(SdsContentName.HazardousDecomposition, viewModel.HazardousDecomposition);

            // Section 11: Toxicological Information
            AddIfNotEmpty(SdsContentName.ToxicologicalEffects, viewModel.ToxicologicalEffects);
            AddIfNotEmpty(SdsContentName.AcuteToxicity, viewModel.AcuteToxicity); // new
            AddIfNotEmpty(SdsContentName.SkinCorrosion, viewModel.SkinCorrosion); // new
            AddIfNotEmpty(SdsContentName.EyeDamage, viewModel.EyeDamage); // new
            AddIfNotEmpty(SdsContentName.SkinSensitization, viewModel.SkinSensitization); // new
            AddIfNotEmpty(SdsContentName.GermCell, viewModel.GermCell); // new
            AddIfNotEmpty(SdsContentName.Carcinogenicity, viewModel.Carcinogenicity); // new
            AddIfNotEmpty(SdsContentName.ReproductiveToxicity, viewModel.ReproductiveToxicity); // new
            AddIfNotEmpty(SdsContentName.SingleExposure, viewModel.SingleExposure); // new
            AddIfNotEmpty(SdsContentName.RepeatedExposure, viewModel.RepeatedExposure); // new
            AddIfNotEmpty(SdsContentName.AspirationHazard, viewModel.AspirationHazard); // new
            AddIfNotEmpty(SdsContentName.PhotoToxicity, viewModel.PhotoToxicity); // new
            AddIfNotEmpty(SdsContentName.OtherInfo, viewModel.OtherInfo); // new

            // Section 12: Ecological Information
            AddIfNotEmpty(SdsContentName.EcoToxicity, viewModel.EcoToxicity);
            AddIfNotEmpty(SdsContentName.PersistenceDegradability, viewModel.PersistenceDegradability);
            AddIfNotEmpty(SdsContentName.BioaccumulationPotential, viewModel.BioaccumulationPotential);
            AddIfNotEmpty(SdsContentName.SoilMobility, viewModel.SoilMobility);
            AddIfNotEmpty(SdsContentName.PbtAssessment, viewModel.PbtAssessment);
            AddIfNotEmpty(SdsContentName.OtherAdverseEffects, viewModel.OtherAdverseEffects);

            // Section 13: Disposal Considerations
            AddIfNotEmpty(SdsContentName.WasteTreatmentMethod, viewModel.WasteTreatmentMethod);

            // Section 14: Transport Information
            AddIfNotEmpty(SdsContentName.UnRoad, viewModel.UnRoad);
            AddIfNotEmpty(SdsContentName.UnSea, viewModel.UnSea);
            AddIfNotEmpty(SdsContentName.UnAir, viewModel.UnAir);
            AddIfNotEmpty(SdsContentName.ShippingName, viewModel.ShippingName);
            AddIfNotEmpty(SdsContentName.HazardClass, viewModel.HazardClass);
            AddIfNotEmpty(SdsContentName.PackingGroup, viewModel.PackingGroup);
            AddIfNotEmpty(SdsContentName.EnvironmentalHazards, viewModel.EnvironmentalHazards);
            AddIfNotEmpty(SdsContentName.SpecialPrecautions, viewModel.SpecialPrecautions); // new
            AddIfNotEmpty(SdsContentName.BulkTranprt, viewModel.BulkTranprt); // new
            AddIfNotEmpty(SdsContentName.IbcCode, viewModel.IbcCode); // new

            // Section 15: Regulatory Information
            AddIfNotEmpty(SdsContentName.SafetyRegulations, viewModel.SafetyRegulations);
            AddIfNotEmpty(SdsContentName.ChemicalSafetyAssessment, viewModel.ChemicalSafetyAssessment);

            // Section 16: Other Information
            AddIfNotEmpty(SdsContentName.OtherInformation, viewModel.OtherInformation);
            AddIfNotEmpty(SdsContentName.PrecautionaryStatements, viewModel.PrecautionaryStatements); // new
            AddIfNotEmpty(SdsContentName.RevisionDate, viewModel.RevisionDate?.ToString("yyyy - MM - dd")); // new
            AddIfNotEmpty(SdsContentName.RevisionReason, viewModel.RevisionReason); // new
            AddIfNotEmpty(SdsContentName.RevNo, viewModel.RevNo);  // new

            return sdsContents;
        }

        public async Task<string> GenerateProductIdAsync()
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
            SetPropertyIfExists(SdsContentName.ProductCode, value => viewModel.ProductCode = value);
            SetPropertyIfExists(SdsContentName.ProductName, value => viewModel.ProductName = value);
            SetPropertyIfExists(SdsContentName.ProductImage, value => viewModel.ProductImage = value);
            SetPropertyIfExists(SdsContentName.BiologicalDefinition, value => viewModel.BiologicalDefinition = value);
            SetPropertyIfExists(SdsContentName.InciName, value => viewModel.InciName = value);
            SetPropertyIfExists(SdsContentName.CasNumber, value => viewModel.CasNumber = value);
            SetPropertyIfExists(SdsContentName.FemaNumber, value => viewModel.FemaNumber = value);
            SetPropertyIfExists(SdsContentName.EinecsNumber, value => viewModel.EinecsNumber = value);
            SetPropertyIfExists(SdsContentName.IdentifiedUses, value => viewModel.IdentifiedUses = value);
            SetPropertyIfExists(SdsContentName.SupplierDetails, value => viewModel.SupplierDetails = value);
            SetPropertyIfExists(SdsContentName.EmergencyPhone, value => viewModel.EmergencyPhone = value);

            // Section 2: Hazard Identification
            SetPropertyIfExists(SdsContentName.ClassificationContent, value => viewModel.ClassificationContent = value);
            SetPropertyIfExists(SdsContentName.LabelContent, value => viewModel.LabelContent = value);
            SetPropertyIfExists(SdsContentName.SignalWord, value => viewModel.SignalWord = value);
            SetPropertyIfExists(SdsContentName.ContainsInfo, value => viewModel.ContainsInfo = value);
            SetPropertyIfExists(SdsContentName.HazardStatements, value => viewModel.HazardStatements = value);
            SetPropertyIfExists(SdsContentName.Precautionary, value => viewModel.Precautionary = value); // new
            SetPropertyIfExists(SdsContentName.Supplementary, value => viewModel.Supplementary = value); // new
            SetPropertyIfExists(SdsContentName.OtherHazards, value => viewModel.OtherHazards = value);

            // Section 3: Composition
            SetPropertyIfExists(SdsContentName.SubstancesContent, value => viewModel.SubstancesContent = value);

            // Section 4: First Aid
            SetPropertyIfExists(SdsContentName.InhalationFirstAid, value => viewModel.InhalationFirstAid = value);
            SetPropertyIfExists(SdsContentName.IngestionFirstAid, value => viewModel.IngestionFirstAid = value);
            SetPropertyIfExists(SdsContentName.SkinContactFirstAid, value => viewModel.SkinContactFirstAid = value);
            SetPropertyIfExists(SdsContentName.EyeContactFirstAid, value => viewModel.EyeContactFirstAid = value);
            SetPropertyIfExists(SdsContentName.SymptomsEffects, value => viewModel.SymptomsEffects = value);
            SetPropertyIfExists(SdsContentName.MedicalAttention, value => viewModel.MedicalAttention = value);

            // Section 5: Firefighting
            SetPropertyIfExists(SdsContentName.ExtinguishingMediaContent, value => viewModel.ExtinguishingMediaContent = value);
            SetPropertyIfExists(SdsContentName.SpecialHazardsContent, value => viewModel.SpecialHazardsContent = value);
            SetPropertyIfExists(SdsContentName.FirefighterAdviceContent, value => viewModel.FirefighterAdviceContent = value);

            // Section 6: Accidental Release
            SetPropertyIfExists(SdsContentName.PersonalPrecautions, value => viewModel.PersonalPrecautions = value);
            SetPropertyIfExists(SdsContentName.EnvironmentalPrecautions, value => viewModel.EnvironmentalPrecautions = value);
            SetPropertyIfExists(SdsContentName.ContainmentMethods, value => viewModel.ContainmentMethods = value);
            SetPropertyIfExists(SdsContentName.SectionReferences, value => viewModel.SectionReferences = value);

            // Section 7: Handling and Storage
            SetPropertyIfExists(SdsContentName.SafeHandlingPrecautions, value => viewModel.SafeHandlingPrecautions = value);
            SetPropertyIfExists(SdsContentName.SafeStorageConditions, value => viewModel.SafeStorageConditions = value);
            SetPropertyIfExists(SdsContentName.SpecificEndUses, value => viewModel.SpecificEndUses = value);

            // Section 8: Exposure Controls/Personal Protection
            SetPropertyIfExists(SdsContentName.ControlParameters, value => viewModel.ControlParameters = value); // new
            SetPropertyIfExists(SdsContentName.ProtectiveEquipmentImage, value => viewModel.ProtectiveEquipmentImage = value);
            SetPropertyIfExists(SdsContentName.ProcessConditions, value => viewModel.ProcessConditions = value);
            SetPropertyIfExists(SdsContentName.EngineeringMeasures, value => viewModel.EngineeringMeasures = value);
            SetPropertyIfExists(SdsContentName.RespiratoryEquipment, value => viewModel.RespiratoryEquipment = value);
            SetPropertyIfExists(SdsContentName.HandProtection, value => viewModel.HandProtection = value);
            SetPropertyIfExists(SdsContentName.EyeProtection, value => viewModel.EyeProtection = value);
            SetPropertyIfExists(SdsContentName.OtherProtection, value => viewModel.OtherProtection = value);
            SetPropertyIfExists(SdsContentName.HygieneMeasures, value => viewModel.HygieneMeasures = value);
            SetPropertyIfExists(SdsContentName.PersonalProtection, value => viewModel.PersonalProtection = value);
            SetPropertyIfExists(SdsContentName.SkinProtection, value => viewModel.SkinProtection = value);
            SetPropertyIfExists(SdsContentName.EnvironmentalExposure, value => viewModel.EnvironmentalExposure = value);

            // Section 9: Physical and Chemical Properties
            SetPropertyIfExists(SdsContentName.Appearance, value => viewModel.Appearance = value);
            SetPropertyIfExists(SdsContentName.Colour, value => viewModel.Colour = value);
            SetPropertyIfExists(SdsContentName.Odour, value => viewModel.Odour = value);
            SetPropertyIfExists(SdsContentName.RelativeDensity, value => viewModel.RelativeDensity = value);
            SetPropertyIfExists(SdsContentName.FlashPoint, value => viewModel.FlashPoint = value);
            SetPropertyIfExists(SdsContentName.MeltingPoint, value => viewModel.MeltingPoint = value);
            SetPropertyIfExists(SdsContentName.RefractiveIndex, value => viewModel.RefractiveIndex = value);
            SetPropertyIfExists(SdsContentName.BoilingPoint, value => viewModel.BoilingPoint = value);
            SetPropertyIfExists(SdsContentName.VapourPressure, value => viewModel.VapourPressure = value);
            SetPropertyIfExists(SdsContentName.SolubilityInWater, value => viewModel.SolubilityInWater = value);
            SetPropertyIfExists(SdsContentName.AutoIgnitionTemp, value => viewModel.AutoIgnitionTemp = value);
            SetPropertyIfExists(SdsContentName.OtherChemicalInfo, value => viewModel.OtherChemicalInfo = value);

            // Section 10: Stability and Reactivity
            SetPropertyIfExists(SdsContentName.ReactivityInfo, value => viewModel.ReactivityInfo = value);
            SetPropertyIfExists(SdsContentName.ChemicalStability, value => viewModel.ChemicalStability = value);
            SetPropertyIfExists(SdsContentName.HazardousReactions, value => viewModel.HazardousReactions = value);
            SetPropertyIfExists(SdsContentName.ConditionsToAvoid, value => viewModel.ConditionsToAvoid = value);
            SetPropertyIfExists(SdsContentName.IncompatibleMaterials, value => viewModel.IncompatibleMaterials = value);
            SetPropertyIfExists(SdsContentName.HazardousDecomposition, value => viewModel.HazardousDecomposition = value);

            // Section 11: Toxicological Information
            SetPropertyIfExists(SdsContentName.ToxicologicalEffects, value => viewModel.ToxicologicalEffects = value);
            SetPropertyIfExists(SdsContentName.AcuteToxicity, value => viewModel.AcuteToxicity = value); //new
            SetPropertyIfExists(SdsContentName.SkinCorrosion, value => viewModel.SkinCorrosion = value); //new
            SetPropertyIfExists(SdsContentName.EyeDamage, value => viewModel.EyeDamage = value); //new
            SetPropertyIfExists(SdsContentName.SkinSensitization, value => viewModel.SkinSensitization = value); //new
            SetPropertyIfExists(SdsContentName.GermCell, value => viewModel.GermCell = value); //new
            SetPropertyIfExists(SdsContentName.Carcinogenicity, value => viewModel.Carcinogenicity = value); //new
            SetPropertyIfExists(SdsContentName.ReproductiveToxicity, value => viewModel.ReproductiveToxicity = value); //new
            SetPropertyIfExists(SdsContentName.SingleExposure, value => viewModel.SingleExposure = value); //new
            SetPropertyIfExists(SdsContentName.RepeatedExposure, value => viewModel.RepeatedExposure = value); //new
            SetPropertyIfExists(SdsContentName.AspirationHazard, value => viewModel.AspirationHazard = value); //new
            SetPropertyIfExists(SdsContentName.PhotoToxicity, value => viewModel.PhotoToxicity = value); //new
            SetPropertyIfExists(SdsContentName.OtherInfo, value => viewModel.OtherInfo = value); //new


            // Section 12: Ecological Information
            SetPropertyIfExists(SdsContentName.EcoToxicity, value => viewModel.EcoToxicity = value);
            SetPropertyIfExists(SdsContentName.PersistenceDegradability, value => viewModel.PersistenceDegradability = value);
            SetPropertyIfExists(SdsContentName.BioaccumulationPotential, value => viewModel.BioaccumulationPotential = value);
            SetPropertyIfExists(SdsContentName.SoilMobility, value => viewModel.SoilMobility = value);
            SetPropertyIfExists(SdsContentName.PbtAssessment, value => viewModel.PbtAssessment = value);
            SetPropertyIfExists(SdsContentName.OtherAdverseEffects, value => viewModel.OtherAdverseEffects = value);

            // Section 13: Disposal Considerations
            SetPropertyIfExists(SdsContentName.WasteTreatmentMethod, value => viewModel.WasteTreatmentMethod = value);

            // Section 14: Transport Information
            SetPropertyIfExists(SdsContentName.UnRoad, value => viewModel.UnRoad = value);
            SetPropertyIfExists(SdsContentName.UnSea, value => viewModel.UnSea = value);
            SetPropertyIfExists(SdsContentName.UnAir, value => viewModel.UnAir = value);
            SetPropertyIfExists(SdsContentName.ShippingName, value => viewModel.ShippingName = value);
            SetPropertyIfExists(SdsContentName.HazardClass, value => viewModel.HazardClass = value);
            SetPropertyIfExists(SdsContentName.PackingGroup, value => viewModel.PackingGroup = value);
            SetPropertyIfExists(SdsContentName.EnvironmentalHazards, value => viewModel.EnvironmentalHazards = value);
            SetPropertyIfExists(SdsContentName.SpecialPrecautions, value => viewModel.SpecialPrecautions = value); // new
            SetPropertyIfExists(SdsContentName.BulkTranprt, value => viewModel.BulkTranprt = value); // new
            SetPropertyIfExists(SdsContentName.IbcCode, value => viewModel.IbcCode = value); // new


            // Section 15: Regulatory Information
            SetPropertyIfExists(SdsContentName.SafetyRegulations, value => viewModel.SafetyRegulations = value);
            SetPropertyIfExists(SdsContentName.ChemicalSafetyAssessment, value => viewModel.ChemicalSafetyAssessment = value);

            // Section 16: Other Information
            SetPropertyIfExists(SdsContentName.OtherInformation, value => viewModel.OtherInformation = value);
            SetPropertyIfExists(SdsContentName.PrecautionaryStatements, value => viewModel.PrecautionaryStatements = value); // new
                                                                                                                             // SetPropertyIfExists(SdsContentName.RevisionDate, value =>
                                                                                                                             // {
                                                                                                                             //     if (DateTime.TryParseExact(value, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var parsedDate)) // new
                                                                                                                             //     {
                                                                                                                             //         viewModel.RevisionDate = parsedDate;
                                                                                                                             //     }
                                                                                                                             //     else
                                                                                                                             //     {
                                                                                                                             //         viewModel.RevisionDate = null;
                                                                                                                             //     }
                                                                                                                             // });
                                                                                                                             // SetPropertyIfExists(SdsContentName.RevisionDate, value =>
                                                                                                                             // {
                                                                                                                             //     Console.WriteLine($"Date string: '{value}'");
                                                                                                                             //     if (DateTime.TryParseExact(value, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var parsedDate))
                                                                                                                             //     {
                                                                                                                             //         viewModel.RevisionDate = parsedDate;
                                                                                                                             //     }
                                                                                                                             //     else
                                                                                                                             //     {
                                                                                                                             //         viewModel.RevisionDate = null;
                                                                                                                             //     }
                                                                                                                             // });

            SetPropertyIfExists(SdsContentName.RevisionDate, value =>
            {
                Console.WriteLine($"Date string: '{value}'");
                if (DateTime.TryParse(value, out var parsedDate))
                {
                    viewModel.RevisionDate = parsedDate;
                }
                else
                {
                    viewModel.RevisionDate = null;
                }
            });


            SetPropertyIfExists(SdsContentName.RevisionReason, value => viewModel.RevisionReason = value); // new
            SetPropertyIfExists(SdsContentName.RevNo, value => viewModel.RevNo = value); // new

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
                        Base64Image = image.Base64Image != null ? Convert.ToBase64String(image.ImageData) : null,
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



    }
}