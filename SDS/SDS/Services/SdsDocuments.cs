using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SDS.Data;
using SDS.Helper;
using SDS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SDS.Services
{
    public class SdsDocuments : IDocument
    {
        private readonly SdsViewModel _model;
        private readonly IWebHostEnvironment _env;
        private readonly string _logPath;
        private readonly SdsDbContext _context;

        // Style 



        public SdsDocuments(SdsViewModel model, IWebHostEnvironment env)
        {
            _model = model;
            _env = env;

            try
            {
                var imagePath = Path.Combine(_env.WebRootPath, "images", "prooil.jpg");
                _logPath = File.Exists(imagePath) ? imagePath : null;
            }
            catch
            {
                // If there's any issue loading the image, set path to null
                _logPath = null;
            }
        }



        // private void AppendHtmlNode(ITextDescriptor text, HtmlAgilityPack.HtmlNode node)
        // {
        //     if (node.NodeType == HtmlAgilityPack.HtmlNodeType.Text)
        //     {
        //         var clean = HtmlEntity.DeEntitize(node.InnerText);
        //         if (!string.IsNullOrWhiteSpace(clean))
        //             text.Span(clean);
        //         return;
        //     }

        //     switch (node.Name.ToLower())
        //     {
        //         case "br":
        //             text.Span("\n");
        //             return;

        //         case "p":
        //             foreach (var child in node.ChildNodes)
        //                 AppendHtmlNode(text, child);
        //             text.Span("\n\n"); // Paragraph spacing
        //             return;

        //         case "ul":
        //             foreach (var li in node.SelectNodes("./li") ?? Enumerable.Empty<HtmlAgilityPack.HtmlNode>())
        //             {
        //                 text.Span("• ");
        //                 AppendHtmlNode(text, li);
        //                 text.Span("\n");
        //             }
        //             return;

        //         case "ol":
        //             int count = 1;
        //             foreach (var li in node.SelectNodes("./li") ?? Enumerable.Empty<HtmlAgilityPack.HtmlNode>())
        //             {
        //                 text.Span($"{count++}. ");
        //                 AppendHtmlNode(text, li);
        //                 text.Span("\n");
        //             }
        //             return;
        //     }

        //     // Inline formatting (bold, italic, underline)
        //     text.Span(span =>
        //     {
        //         if (node.Name is "b" or "strong")
        //             span = span.Bold();
        //         if (node.Name is "i" or "em")
        //             span = span.Italic();
        //         if (node.Name is "u")
        //             span = span.Underline();

        //         foreach (var child in node.ChildNodes)
        //         {
        //             AppendHtmlNodeToSpan(span, child);
        //         }
        //     });
        // }

        // private void AppendHtmlNodeToSpan(ITextSpanDescriptor span, HtmlAgilityPack.HtmlNode node)
        // {
        //     if (node.NodeType == HtmlAgilityPack.HtmlNodeType.Text)
        //     {
        //         var clean = HtmlEntity.DeEntitize(node.InnerText);
        //         if (!string.IsNullOrWhiteSpace(clean))
        //             span.Text(clean);
        //         return;
        //     }

        //     // Nested inline formatting
        //     span.Span(inner =>
        //     {
        //         if (node.Name is "b" or "strong")
        //             inner = inner.Bold();
        //         if (node.Name is "i" or "em")
        //             inner = inner.Italic();
        //         if (node.Name is "u")
        //             inner = inner.Underline();

        //         foreach (var child in node.ChildNodes)
        //         {
        //             AppendHtmlNodeToSpan(inner, child);
        //         }
        //     });
        // }



        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;



        public void Compose(IDocumentContainer container)
        {
            container
                .Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(25);
                    page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(10));

                    // Set explicit heights for header and footer
                    page.Header().Height(80).Element(ComposeHeader);
                    page.Content().Element(ComposeContent);
                    page.Footer().Height(80).Element(ComposeFooter);
                });
        }




        private void ComposeHeader(IContainer container)
        {
            // Increase the header height to accommodate the logo
            container.Height(80).PaddingBottom(5).BorderBottom(1).BorderColor(Colors.Grey.Medium)
                .Row(row =>
                {
                    // left text
                    row.RelativeItem().Column(column =>
                    {
                        column.Item().Text("PRO-OILS AROMATHERAPY").Bold().FontSize(10);
                        column.Item().Text("MATERIAL SAFETY DATA SHEET");
                    });

                    // // centered logo - make sure height matches container height
                    row.ConstantItem(220).Height(70).AlignCenter().AlignMiddle()
                        .Image(_logPath).FitArea();
                    // In your ComposeHeader method
                    // centered logo
                    // if (_logPath != null && File.Exists(_logPath))
                    // {
                    //     row.ConstantItem(200).Height(70).AlignCenter().AlignMiddle()
                    //         .Image(_logPath).FitArea();
                    // }
                    // else
                    // {
                    //     // Fallback when image is not available
                    //     row.ConstantItem(220).Height(70).AlignCenter().AlignMiddle()
                    //         .Text("PRO-OILS").FontSize(16).Bold();
                    // }


                    // right text
                    row.RelativeItem().AlignRight().Text(text =>
                    {
                        text.Span("Page ");
                        text.CurrentPageNumber();
                        text.Span(" of ");
                        text.TotalPages();
                    });
                });
        }



        private void ComposeContent(IContainer container)
        {
            container.PaddingVertical(10).Column(column =>
            {
                // Title
                column.Item().AlignCenter().PaddingBottom(20).Background(Colors.Yellow.Medium).Border(1).BorderColor(Colors.Black)
                .Padding(5).Text("Safety Data Sheet").FontFamily("Georgia").FontSize(20).Bold();

                // section 1; Identification

                // section 2: hazards identification

                // section 3: composition/information on ingredients
                column.Item().Element(ComposeSection3);

                // section 4: first aid measures
                column.Item().Element(ComposeSection4);

                // section 5:  fire fighting measures
                column.Item().Element(ComposeSection5);

                // column.Item().Element(ComposeSection6);
                column.Item().Element(ComposeSection6);

                // column.Item().Element(ComposeSection6);
                column.Item().Element(ComposeSection7);

                // column.Item().Element(ComposeSection6);
                column.Item().Element(ComposeSection8);



            });
        }


        private void ComposeSection3(IContainer container)
        {
            container.Column(c =>
            {
                // Add spacing
                c.Spacing(5);

                // Section Title
                c.Item().Text("03. COMPOSITION/INFORMATION ON INGREDIENTS")
                    .Underline().FontSize(12);

                // Add spacing
                c.Spacing(5);

                //Create the table
                c.Item().Table(table =>
                {
                    // Define the columns = one for the section number, one for content
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(30); // Section number
                        columns.RelativeColumn(); // Content
                    });



                    // header row
                    table.Cell().Border(1).BorderColor(Colors.Grey.Medium)
                        .Background(Colors.Blue.Darken4)
                        .Padding(5)
                        .Text("3.1").FontColor(Colors.White).Bold();

                    table.Cell().Border(1).BorderColor(Colors.Grey.Medium)
                        .Background(Colors.Blue.Darken4)
                        .Padding(5)
                        .Text("Substance").FontColor(Colors.White).Bold();

                    // Content row
                    table.Cell().ColumnSpan(2).Border(1).BorderColor(Colors.Grey.Medium)
                    .Padding(10)
                    .Element(container =>
                    {
                        // check if contetn exists
                        var content = Functions.RemoveHtmlTags(_model.SubstancesContent);

                        if (string.IsNullOrEmpty(content))
                        {
                            container.Text("No additional data available").Italic();
                        }
                        else
                        {
                            container.Text(content);
                        }
                    });

                    // Content row
                    // table.Cell().(1).BorderColor(Colors.Grey.Medium)
                    //     .BorderRight(0).BorderColor(Colors.White)
                    //     .Padding(5)
                    //     .Text("");

                    // table.Cell().ColumnSpan(2).Border(1).BorderColor(Colors.Grey.Medium)
                    //     .Padding(5)
                    //     .Element(container => RenderFormattedText(container, _model.SubstancesContent));
                });
            });
        }

        private void ComposeSection4(IContainer container)
        {
            container.Column(c =>
            {

                // Section Title
                c.Item().PaddingTop(5).PaddingBottom(5).Text("04. FIRST AID MEASURES")

                    .Underline().FontSize(12);

                // Add spacing
                // c.Spacing(5);

                // First table for the header row
                c.Item().PaddingBottom(0).Table(table =>
                {
                    // Define the columns = one for the section number, one for content
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(30); // Section number
                        columns.RelativeColumn(); // Content
                    });

                    // header row
                    table.Cell().Border(1).BorderColor(Colors.Grey.Medium)
                        .BorderBottom(0) // Remove bottom border to connect with next table
                        .Background(Colors.Blue.Darken4)
                        .Padding(5)
                        .Text("4.1").FontColor(Colors.White).Bold();

                    table.Cell().Border(1).BorderColor(Colors.Grey.Medium)
                        .BorderBottom(0) // Remove bottom border to connect with next table
                        .Background(Colors.Blue.Darken4)
                        .Padding(5)
                        .Text("Description of first aid measures").FontColor(Colors.White).Bold();
                });

                // Second table for the sub-sections - no top margin
                c.Item().PaddingTop(0).Table(table =>
                {
                    // Define different columns for the sub-sections
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(100); // Wider column for "Inhalation"
                        columns.RelativeColumn(); // Content
                    });

                    // sub header row
                    table.Cell().Border(1).BorderColor(Colors.Grey.Medium)
                        .BorderTop(0) // Remove top border to connect with previous table
                        .Background(Colors.Blue.Darken4)
                        .Padding(10)
                        .Text("Inhalation").FontColor(Colors.White).Bold();

                    table.Cell().Border(1).BorderColor(Colors.Grey.Medium)
                        .BorderTop(0) // Remove top border to connect with previous table
                        .Padding(10)
                        .Element(container =>
                        {
                            var content = Functions.RemoveHtmlTags(_model.InhalationFirstAid);
                            if (string.IsNullOrEmpty(content))
                            {
                                container.Text("No additional data available").Italic();
                            }
                            else
                            {
                                container.Text(content);
                            }
                        });

                    // sub header row
                    table.Cell().Border(1).BorderColor(Colors.Grey.Medium)
                        .BorderTop(0) // Remove top border to connect with previous table
                        .Background(Colors.Blue.Darken4)
                        .Padding(10)
                        .Text("Ingestion").FontColor(Colors.White).Bold();

                    table.Cell().Border(1).BorderColor(Colors.Grey.Medium)
                        .BorderTop(0) // Remove top border to connect with previous table
                        .Padding(10)
                        .Element(container =>
                        {
                            var content = Functions.RemoveHtmlTags(_model.IngestionFirstAid);
                            if (string.IsNullOrEmpty(content))
                            {
                                container.Text("No additional data available").Italic();
                            }
                            else
                            {
                                container.Text(content);
                            }
                        });

                    // sub header row
                    table.Cell().Border(1).BorderColor(Colors.Grey.Medium)
                        .BorderTop(0) // Remove top border to connect with previous table
                        .Background(Colors.Blue.Darken4)
                        .Padding(10)
                        .Text("Skin Contact").FontColor(Colors.White).Bold();

                    table.Cell().Border(1).BorderColor(Colors.Grey.Medium)
                        .BorderTop(0) // Remove top border to connect with previous table
                        .Padding(10)
                        .Element(container =>
                        {
                            var content = Functions.RemoveHtmlTags(_model.SkinContactFirstAid);
                            if (string.IsNullOrEmpty(content))
                            {
                                container.Text("No additional data available").Italic();
                            }
                            else
                            {
                                container.Text(content);
                            }
                        });

                    // sub header row
                    table.Cell().Border(1).BorderColor(Colors.Grey.Medium)
                        .BorderTop(0) // Remove top border to connect with previous table
                        .Background(Colors.Blue.Darken4)
                        .Padding(10)
                        .Text("Eye Contact").FontColor(Colors.White).Bold();

                    table.Cell().Border(1).BorderColor(Colors.Grey.Medium)
                        .BorderTop(0) // Remove top border to connect with previous table
                        .Padding(10)
                        .Element(container =>
                        {
                            var content = Functions.RemoveHtmlTags(_model.EyeContactFirstAid);
                            if (string.IsNullOrEmpty(content))
                            {
                                container.Text("No additional data available").Italic();
                            }
                            else
                            {
                                container.Text(content);
                            }
                        });


                });

                c.Item().PaddingTop(0).Table(table =>
                {
                    // Define the columns = one for the section number, one for content
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(30); // Section number
                        columns.RelativeColumn(); // Content
                    });

                    // header row
                    table.Cell().Border(1).BorderColor(Colors.Grey.Medium)
                        .Background(Colors.Blue.Darken4)
                        .Padding(5)
                        .Text("4.2").FontColor(Colors.White).Bold();

                    table.Cell().Border(1).BorderColor(Colors.Grey.Medium)
                        .Background(Colors.Blue.Darken4)
                        .Padding(5)
                        .Text("Most important symptoms and effects, both acute and delayed").FontColor(Colors.White).Bold();

                    // Content row
                    table.Cell().ColumnSpan(2).Border(1).BorderColor(Colors.Grey.Medium)
                    .Padding(10)
                    .Element(container =>
                    {
                        var content = Functions.RemoveHtmlTags(_model.SymptomsEffects);
                        if (string.IsNullOrEmpty(content))
                        {
                            container.Text("No additional data available").Italic();
                        }
                        else
                        {
                            container.Text(content);
                        }
                    });

                    // header row
                    table.Cell().Border(1).BorderColor(Colors.Grey.Medium)
                        .Background(Colors.Blue.Darken4)
                        .Padding(5)
                        .Text("4.3").FontColor(Colors.White).Bold();

                    table.Cell().Border(1).BorderColor(Colors.Grey.Medium)
                        .Background(Colors.Blue.Darken4)
                        .Padding(5)
                        .Text("Indication of any immediate medical attention and special treatment needed").FontColor(Colors.White).Bold();

                    // Content row
                    table.Cell().ColumnSpan(2).Border(1).BorderColor(Colors.Grey.Medium)
                    .Padding(10)
                    .Element(container =>
                    {
                        var content = Functions.RemoveHtmlTags(_model.MedicalAttention);
                        if (string.IsNullOrEmpty(content))
                        {
                            container.Text("No additional data available").Italic();
                        }
                        else
                        {
                            container.Text(content);
                        }
                    });
                });
            });
        }

        private void ComposeSection5(IContainer container)
        {
            container.Column(c =>
            {
                // Section Title
                c.Item().PaddingTop(5).PaddingBottom(5).Text("05. FIRE FIGHTING MEASURES")
                    .Underline().FontSize(12);

                // Add spacing
                c.Spacing(5);

                // Create the table
                c.Item().Table(table =>
                {
                    // Define the columns = one for the section number, one for content
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(30); // Section number
                        columns.RelativeColumn(); // Content
                    });

                    // header row
                    table.Cell().Border(1).BorderColor(Colors.Grey.Medium)
                        .Background(Colors.Blue.Darken4)
                        .Padding(5)
                        .Text("5.1").FontColor(Colors.White).Bold();

                    table.Cell().Border(1).BorderColor(Colors.Grey.Medium)
                        .Background(Colors.Blue.Darken4)
                        .Padding(5)
                        .Text("Extinguishing Media").FontColor(Colors.White).Bold();

                    // Content row
                    table.Cell().ColumnSpan(2).Border(1).BorderColor(Colors.Grey.Medium)
                    .Padding(10)
                    .Element(container =>
                    {
                        var content = Functions.RemoveHtmlTags(_model.ExtinguishingMediaContent);
                        if (string.IsNullOrEmpty(content))
                        {
                            container.Text("No additional data available").Italic();
                        }
                        else
                        {
                            container.Text(content);
                        }
                    });

                    // header row
                    table.Cell().Border(1).BorderColor(Colors.Grey.Medium)
                        .Background(Colors.Blue.Darken4)
                        .Padding(5)
                        .Text("5.2").FontColor(Colors.White).Bold();

                    table.Cell().Border(1).BorderColor(Colors.Grey.Medium)
                    .Background(Colors.Blue.Darken4)
                        .Padding(5)
                        .Text("Specific hazards arising from the product").FontColor(Colors.White).Bold();
                    // Content row
                    table.Cell().ColumnSpan(2).Border(1).BorderColor(Colors.Grey.Medium)
                    .Padding(10)
                    .Element(container =>
                    {
                        var content = Functions.RemoveHtmlTags(_model.SpecialHazardsContent);

                        if (string.IsNullOrEmpty(content))
                        {
                            container.Text("No additional data available").Italic();
                        }
                        else
                        {
                            container.Text(content);
                        }
                    });


                    //header row
                    table.Cell().Border(1).BorderColor(Colors.Grey.Medium)
                        .Background(Colors.Blue.Darken4)
                        .Padding(5)
                        .Text("5.3").FontColor(Colors.White).Bold();
                    table.Cell().Border(1).BorderColor(Colors.Grey.Medium)
                   .Background(Colors.Blue.Darken4)
                       .Padding(5)
                       .Text("Advice for fire fighters").FontColor(Colors.White).Bold();

                    // Content row
                    table.Cell().ColumnSpan(2).Border(1).BorderColor(Colors.Grey.Medium)
                    .Padding(10)
                    .Element(container =>
                    {
                        var content = Functions.RemoveHtmlTags(_model.FirefighterAdviceContent);

                        if (string.IsNullOrEmpty(content))
                        {
                            container.Text("No additional data available").Italic();
                        }
                        else
                        {
                            container.Text(content);
                        }
                    });

                });
            });
        }


        private void ComposeSection6(IContainer container)
        {
            container.Column(c =>
            {
                // Section Title
                c.Item().PaddingTop(5).PaddingBottom(5).Text("06. ACCIDENTAL RELEASE MEASURES ")
                    .Underline().FontSize(12);

                // Add spacing
                c.Spacing(5);

                // Create the table
                c.Item().Table(table =>
                {
                    // Define the columns = one for the section number, one for content
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(30); // Section number
                        columns.RelativeColumn(); // Content
                    });

                    // header row
                    table.Cell().Border(1).BorderColor(Colors.Grey.Medium)
                        .Background(Colors.Blue.Darken4)
                        .Padding(5)
                        .Text("6.1").FontColor(Colors.White).Bold();

                    table.Cell().Border(1).BorderColor(Colors.Grey.Medium)
                        .Background(Colors.Blue.Darken4)
                        .Padding(5)
                        .Text("Personal precautions, protective equipment and emergency procedures").FontColor(Colors.White).Bold();

                    // Content row
                    table.Cell().ColumnSpan(2).Border(1).BorderColor(Colors.Grey.Medium)
                    .Padding(10)
                    .Element(container =>
                    {
                        var content = Functions.RemoveHtmlTags(_model.PersonalPrecautions);
                        if (string.IsNullOrEmpty(content))
                        {
                            container.Text("No additional data available").Italic();
                        }
                        else
                        {
                            container.Text(content);
                        }
                    });

                    // header row
                    table.Cell().Border(1).BorderColor(Colors.Grey.Medium)
                        .Background(Colors.Blue.Darken4)
                        .Padding(5)
                        .Text("6.2").FontColor(Colors.White).Bold();

                    table.Cell().Border(1).BorderColor(Colors.Grey.Medium)
                    .Background(Colors.Blue.Darken4)
                        .Padding(5)
                        .Text("Environmental Precautions").FontColor(Colors.White).Bold();
                    // Content row
                    table.Cell().ColumnSpan(2).Border(1).BorderColor(Colors.Grey.Medium)
                    .Padding(10)
                    .Element(container =>
                    {
                        var content = Functions.RemoveHtmlTags(_model.EnvironmentalPrecautions);

                        if (string.IsNullOrEmpty(content))
                        {
                            container.Text("No additional data available").Italic();
                        }
                        else
                        {
                            container.Text(content);
                        }
                    });


                    //header row
                    table.Cell().Border(1).BorderColor(Colors.Grey.Medium)
                        .Background(Colors.Blue.Darken4)
                        .Padding(5)
                        .Text("6.3").FontColor(Colors.White).Bold();
                    table.Cell().Border(1).BorderColor(Colors.Grey.Medium)
                   .Background(Colors.Blue.Darken4)
                       .Padding(5)
                       .Text("Methods and material for containment and cleaning up. ").FontColor(Colors.White).Bold();

                    // Content row
                    table.Cell().ColumnSpan(2).Border(1).BorderColor(Colors.Grey.Medium)
                    .Padding(10)
                    .Element(container =>
                    {
                        var content = Functions.RemoveHtmlTags(_model.ContainmentMethods);

                        if (string.IsNullOrEmpty(content))
                        {
                            container.Text("No additional data available").Italic();
                        }
                        else
                        {
                            container.Text(content);
                        }
                    });

                    //header row
                    table.Cell().Border(1).BorderColor(Colors.Grey.Medium)
                        .Background(Colors.Blue.Darken4)
                        .Padding(5)
                        .Text("6.4").FontColor(Colors.White).Bold();
                    table.Cell().Border(1).BorderColor(Colors.Grey.Medium)
                   .Background(Colors.Blue.Darken4)
                       .Padding(5)
                       .Text("Reference to other sections ").FontColor(Colors.White).Bold();

                    // Content row
                    table.Cell().ColumnSpan(2).Border(1).BorderColor(Colors.Grey.Medium)
                    .Padding(10)
                    .Element(container =>
                    {
                        var content = Functions.RemoveHtmlTags(_model.SectionReferences);

                        if (string.IsNullOrEmpty(content))
                        {
                            container.Text("No additional data available").Italic();
                        }
                        else
                        {
                            container.Text(content);
                        }
                    });

                });
            });
        }

        private void ComposeSection7(IContainer container)
        {
            container.Column(c =>
            {
                // Section Title
                c.Item().PaddingTop(5).PaddingBottom(5).Text("07. HANDLING AND STORAGE")
                    .Underline().FontSize(12);

                // Add spacing
                c.Spacing(5);

                // Create the table
                c.Item().Table(table =>
                {
                    // Define the columns = one for the section number, one for content
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(30); // Section number
                        columns.RelativeColumn(); // Content
                    });

                    // header row
                    table.Cell().Border(1).BorderColor(Colors.Grey.Medium)
                        .Background(Colors.Blue.Darken4)
                        .Padding(5)
                        .Text("7.1").FontColor(Colors.White).Bold();

                    table.Cell().Border(1).BorderColor(Colors.Grey.Medium)
                        .Background(Colors.Blue.Darken4)
                        .Padding(5)
                        .Text("Precautions for safe handling").FontColor(Colors.White).Bold();

                    // Content row
                    table.Cell().ColumnSpan(2).Border(1).BorderColor(Colors.Grey.Medium)
                    .Padding(10)
                    .Element(container =>
                    {
                        var content = Functions.RemoveHtmlTags(_model.SafeHandlingPrecautions);
                        if (string.IsNullOrEmpty(content))
                        {
                            container.Text("No additional data available").Italic();
                        }
                        else
                        {
                            container.Text(content);
                        }
                    });

                    // header row
                    table.Cell().Border(1).BorderColor(Colors.Grey.Medium)
                        .Background(Colors.Blue.Darken4)
                        .Padding(5)
                        .Text("7.2").FontColor(Colors.White).Bold();

                    table.Cell().Border(1).BorderColor(Colors.Grey.Medium)
                    .Background(Colors.Blue.Darken4)
                        .Padding(5)
                        .Text("Conditions for safe storage, including any incompatibilities ").FontColor(Colors.White).Bold();
                    // Content row
                    table.Cell().ColumnSpan(2).Border(1).BorderColor(Colors.Grey.Medium)
                    .Padding(10)
                    .Element(container =>
                    {
                        var content = Functions.RemoveHtmlTags(_model.SafeStorageConditions);

                        if (string.IsNullOrEmpty(content))
                        {
                            container.Text("No additional data available").Italic();
                        }
                        else
                        {
                            container.Text(content);
                        }
                    });


                    //header row
                    table.Cell().Border(1).BorderColor(Colors.Grey.Medium)
                        .Background(Colors.Blue.Darken4)
                        .Padding(5)
                        .Text("7.3").FontColor(Colors.White).Bold();
                    table.Cell().Border(1).BorderColor(Colors.Grey.Medium)
                   .Background(Colors.Blue.Darken4)
                       .Padding(5)
                       .Text("Specific end use(s)").FontColor(Colors.White).Bold();

                    // Content row
                    table.Cell().ColumnSpan(2).Border(1).BorderColor(Colors.Grey.Medium)
                    .Padding(10)
                    .Element(container =>
                    {
                        var content = Functions.RemoveHtmlTags(_model.SpecificEndUses);

                        if (string.IsNullOrEmpty(content))
                        {
                            container.Text("No additional data available").Italic();
                        }
                        else
                        {
                            container.Text(content);
                        }
                    });

                });
            });
        }

        private void ComposeSection8(IContainer container)
        {
            container.Column(c =>
            {
                // Section Title
                c.Item().PaddingTop(5).PaddingBottom(5).Text("08. EXPOSURE CONTROLS/PERSONAL PROTECTION")
                    .Underline().FontSize(12);

                // Add spacing
                c.Spacing(5);

                // Create the table
                c.Item().PaddingBottom(0).Table(table =>
                {
                    // Define the columns = one for the section number, one for content
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(30); // Section number
                        columns.RelativeColumn(); // Content
                    });

                    // header row
                    table.Cell().Border(1).BorderColor(Colors.Grey.Medium)
                        .BorderBottom(0) // Remove bottom border to connect with next table
                        .Background(Colors.Blue.Darken4)
                        .Padding(5)
                        .Text("8.1").FontColor(Colors.White).Bold();

                    table.Cell().Border(1).BorderColor(Colors.Grey.Medium)
                        .BorderBottom(0) // Remove bottom border to connect with next table
                        .Background(Colors.Blue.Darken4)
                        .Padding(5)
                        .Text("Control parameters").FontColor(Colors.White).Bold();

                    // Content row
                    table.Cell().ColumnSpan(2).Border(1).BorderColor(Colors.Grey.Medium)
                    .Padding(10)
                    .Element(container =>
                    {
                        var content = Functions.RemoveHtmlTags(_model.ControlParameters);
                        if (string.IsNullOrEmpty(content))
                        {
                            container.Text("No additional data available").Italic();
                        }
                        else
                        {
                            container.Text(content);
                        }
                    });

                    // header row
                    table.Cell().Border(1).BorderColor(Colors.Grey.Medium)
                        .BorderBottom(0) // Remove bottom border to connect with next table
                        .Background(Colors.Blue.Darken4)
                        .Padding(5)
                        .Text("8.2").FontColor(Colors.White).Bold();
                    
                    table.Cell().Border(1).BorderColor(Colors.Grey.Medium)
                        .BorderBottom(0) // Remove bottom border to connect with next table
                        .Background(Colors.Blue.Darken4)
                        .Padding(5)
                        .Text("Exposure controls").FontColor(Colors.White).Bold();

                    table.Cell().ColumnSpan(2).Border(1).BorderColor(Colors.Grey.Medium)
                        .BorderBottom(0)
                       .Padding(5)
                       .Text("Protective Equipment ");

                    table.Cell().ColumnSpan(2).Border(1).BorderColor(Colors.Grey.Medium)
                        .BorderBottom(0) // Remove bottom border to connect with next table
           .Padding(2)
           .AlignMiddle()
           .AlignCenter()
           .Element(container =>
           {
               if (_model.ImagesByContentID.TryGetValue(ImageIds.protectiveEquipmentImage, out var images))
               {
                   container.Row(row =>
                   {
                       foreach (var image in images)
                       {
                           row.AutoItem().Column(column =>
                           {
                               column.Item().AlignCenter().Width(50).Height(50).Image(image.ImageData);
                               //column.Item().AlignCenter().Text(image.ImageName)
                                   //.FontSize(7).FontColor(Colors.Grey.Medium);
                           });
                           row.AutoItem().Width(16); // Gap between images
                       }
                   });
               }
               else
               {
                   container.Text("No additional data available.");
               }
           });



                    // Second table for the sub-sections - no top margin
                    c.Item().PaddingTop(0).Table(table =>
                    {
                        // Define different columns for the sub-sections
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(100); // Wider column for "Inhalation"
                            columns.RelativeColumn(); // Content
                        });

                        // sub header row
                        table.Cell().Border(1).BorderColor(Colors.Grey.Medium)
                            .BorderTop(0) // Remove top border to connect with previous table
                            .Background(Colors.Blue.Darken4)
                            .Padding(10)
                            .Text("Process Conditions ").FontColor(Colors.White).Bold();

                        table.Cell().Border(1).BorderColor(Colors.Grey.Medium)
                            .BorderTop(0) // Remove top border to connect with previous table
                            .Padding(10)
                            .Element(container =>
                            {
                                var content = Functions.RemoveHtmlTags(_model.ProcessConditions);
                                if (string.IsNullOrEmpty(content))
                                {
                                    container.Text("No additional data available").Italic();
                                }
                                else
                                {
                                    container.Text(content);
                                }
                            });

                        // sub header row
                        table.Cell().Border(1).BorderColor(Colors.Grey.Medium)
                            .BorderTop(0) // Remove top border to connect with previous table
                            .Background(Colors.Blue.Darken4)
                            .Padding(10)
                            .Text("Engineering Measures").FontColor(Colors.White).Bold();

                        table.Cell().Border(1).BorderColor(Colors.Grey.Medium)
                            .BorderTop(0) // Remove top border to connect with previous table
                            .Padding(10)
                            .Element(container =>
                            {
                                var content = Functions.RemoveHtmlTags(_model.EngineeringMeasures);
                                if (string.IsNullOrEmpty(content))
                                {
                                    container.Text("No additional data available").Italic();
                                }
                                else
                                {
                                    container.Text(content);
                                }
                            });

                        // sub header row
                        table.Cell().Border(1).BorderColor(Colors.Grey.Medium)
                            .BorderTop(0) // Remove top border to connect with previous table
                            .Background(Colors.Blue.Darken4)
                            .Padding(10)
                            .Text("Respiratory Equipment").FontColor(Colors.White).Bold();

                        table.Cell().Border(1).BorderColor(Colors.Grey.Medium)
                            .BorderTop(0) // Remove top border to connect with previous table
                            .Padding(10)
                            .Element(container =>
                            {
                                var content = Functions.RemoveHtmlTags(_model.RespiratoryEquipment);
                                if (string.IsNullOrEmpty(content))
                                {
                                    container.Text("No additional data available").Italic();
                                }
                                else
                                {
                                    container.Text(content);
                                }
                            });
                        // sub header row
                        table.Cell().Border(1).BorderColor(Colors.Grey.Medium)
                            .BorderTop(0) // Remove top border to connect with previous table
                            .Background(Colors.Blue.Darken4)
                            .Padding(10)
                            .Text("Respiratory Equipment ").FontColor(Colors.White).Bold();

                        table.Cell().Border(1).BorderColor(Colors.Grey.Medium)
                            .BorderTop(0) // Remove top border to connect with previous table
                            .Padding(10)
                            .Element(container =>
                            {
                                var content = Functions.RemoveHtmlTags(_model.HandProtection);
                                if (string.IsNullOrEmpty(content))
                                {
                                    container.Text("No additional data available").Italic();
                                }
                                else
                                {
                                    container.Text(content);
                                }
                            });

                        // sub header row
                        table.Cell().Border(1).BorderColor(Colors.Grey.Medium)
                            .BorderTop(0) // Remove top border to connect with previous table
                            .Background(Colors.Blue.Darken4)
                            .Padding(10)
                            .Text("Eye Protection ").FontColor(Colors.White).Bold();

                        table.Cell().Border(1).BorderColor(Colors.Grey.Medium)
                            .BorderTop(0) // Remove top border to connect with previous table
                            .Padding(10)
                            .Element(container =>
                            {
                                var content = Functions.RemoveHtmlTags(_model.EyeProtection);
                                if (string.IsNullOrEmpty(content))
                                {
                                    container.Text("No additional data available").Italic();
                                }
                                else
                                {
                                    container.Text(content);
                                }
                            });

                        // sub header row
                        table.Cell().Border(1).BorderColor(Colors.Grey.Medium)
                            .BorderTop(0) // Remove top border to connect with previous table
                            .Background(Colors.Blue.Darken4)
                            .Padding(10)
                            .Text("Other Protection").FontColor(Colors.White).Bold();

                        table.Cell().Border(1).BorderColor(Colors.Grey.Medium)
                            .BorderTop(0) // Remove top border to connect with previous table
                            .Padding(10)
                            .Element(container =>
                            {
                                var content = Functions.RemoveHtmlTags(_model.OtherProtection);
                                if (string.IsNullOrEmpty(content))
                                {
                                    container.Text("No additional data available").Italic();
                                }
                                else
                                {
                                    container.Text(content);
                                }
                            });

                        // sub header row
                        table.Cell().Border(1).BorderColor(Colors.Grey.Medium)
                            .BorderTop(0) // Remove top border to connect with previous table
                            .Background(Colors.Blue.Darken4)
                            .Padding(10)
                            .Text("Hygiene Measures").FontColor(Colors.White).Bold();

                        table.Cell().Border(1).BorderColor(Colors.Grey.Medium)
                            .BorderTop(0) // Remove top border to connect with previous table
                            .Padding(10)
                            .Element(container =>
                            {
                                var content = Functions.RemoveHtmlTags(_model.HygieneMeasures);
                                if (string.IsNullOrEmpty(content))
                                {
                                    container.Text("No additional data available").Italic();
                                }
                                else
                                {
                                    container.Text(content);
                                }
                            });
                        // sub header row
                        table.Cell().Border(1).BorderColor(Colors.Grey.Medium)
                            .BorderTop(0) // Remove top border to connect with previous table
                            .Background(Colors.Blue.Darken4)
                            .Padding(10)
                            .Text("Personal Protection ").FontColor(Colors.White).Bold();

                        table.Cell().Border(1).BorderColor(Colors.Grey.Medium)
                            .BorderTop(0) // Remove top border to connect with previous table
                            .Padding(10)
                            .Element(container =>
                            {
                                var content = Functions.RemoveHtmlTags(_model.PersonalProtection);
                                if (string.IsNullOrEmpty(content))
                                {
                                    container.Text("No additional data available").Italic();
                                }
                                else
                                {
                                    container.Text(content);
                                }
                            });

                        // sub header row
                        table.Cell().Border(1).BorderColor(Colors.Grey.Medium)
                            .BorderTop(0) // Remove top border to connect with previous table
                            .Background(Colors.Blue.Darken4)
                            .Padding(10)
                            .Text("Skin Protection").FontColor(Colors.White).Bold();

                        table.Cell().Border(1).BorderColor(Colors.Grey.Medium)
                            .BorderTop(0) // Remove top border to connect with previous table
                            .Padding(10)
                            .Element(container =>
                            {
                                var content = Functions.RemoveHtmlTags(_model.SkinProtection);
                                if (string.IsNullOrEmpty(content))
                                {
                                    container.Text("No additional data available").Italic();
                                }
                                else
                                {
                                    container.Text(content);
                                }
                            });

                        // sub header row
                        table.Cell().Border(1).BorderColor(Colors.Grey.Medium)
                            .BorderTop(0) // Remove top border to connect with previous table
                            .Background(Colors.Blue.Darken4)
                            .Padding(10)
                            .Text("Environmental Exposure Control ").FontColor(Colors.White).Bold();

                        table.Cell().Border(1).BorderColor(Colors.Grey.Medium)
                            .BorderTop(0) // Remove top border to connect with previous table
                            .Padding(10)
                            .Element(container =>
                            {
                                var content = Functions.RemoveHtmlTags(_model.EnvironmentalExposure);
                                if (string.IsNullOrEmpty(content))
                                {
                                    container.Text("No additional data available").Italic();
                                }
                                else
                                {
                                    container.Text(content);
                                }
                            });
                    });


                });
            });
        }


        private void ComposeFooter(IContainer container)
        {
            container.BorderTop(1).BorderColor(Colors.Grey.Medium).PaddingTop(5)
                .Column(column =>
                {
                    column.Item().Grid(grid =>
                    {
                        grid.Columns(2);

                        // Left side
                        grid.Item().Column(col =>
                        {
                            col.Item().Text("PRO-OILS AROMATHERAPY").Bold();
                            col.Item().Text($"Tel: {Functions.RemoveHtmlTags(_model.EmergencyPhone)}");
                            col.Item().Text("info@prooils.com.au");
                            col.Item().Text("Unit 6, 163 Newbridge Road, Chipping Norton NSW 2170");
                        });

                        // Right side
                        grid.Item().AlignRight().Column(col =>
                        {
                            col.Item().Text("REVISION DETAILS").Bold();
                            col.Item().Text($"Date: {(_model.RevisionDate ?? DateTime.Now):yyyy-MM-dd}");
                            col.Item().Text($"Rev No: {_model.RevNo}");
                        });
                    });

                    column.Item().AlignCenter().Text(text =>
                    {
                        text.Span("PAGE ").Bold().FontSize(10);
                        text.CurrentPageNumber().Bold().FontSize(10);
                        text.Span(" OF ").Bold().FontSize(10);
                        text.TotalPages().Bold().FontSize(10);
                    });
                });
        }



    }
}