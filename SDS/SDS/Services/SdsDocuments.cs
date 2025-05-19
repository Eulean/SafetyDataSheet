using HtmlAgilityPack;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SDS.Helper;
using SDS.Models;

namespace SDS.Services
{
    public class SdsDocuments : IDocument
    {
        private readonly SdsViewModel _model;
        private readonly IWebHostEnvironment _env;
        private readonly string _logPath;

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
                column.Item().Element(GenerateHtmlContainer(@"User will given like these or 
<p>The Full Text for all Hazard Statements are Displayed in Section 16.</p><p><u>ENVIRONMENT</u></p><p>The product contains a substance which is harmful to aquatic organisms, and which may cause long term adverse effects in the aquatic environment.</p><p><u>HUMAN HEALTH</u></p><p>May cause serious eye damage and skin irritation.</p><p><u>Physical and Chemical Hazards</u>: Not classified.</p><p><u>Human health Asp</u>: Skin Irrit. 2 - H315; Eye Dam. 1 - H318; Skin Sens. 1 - H317</p><p><u>Environment</u>: Aquatic Acute 2 - H401; Aquatic Chronic 2 - H411</p>
"));
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


        private Action<IContainer> GenerateHtmlContainer(string htmlContent)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlContent);

            return container => container.Padding(10).Column(column =>
            {
                var paragraphs = htmlDoc.DocumentNode.SelectNodes("//p");

                if (paragraphs != null)
                {
                    foreach (var p in paragraphs)
                    {
                        var text = p.InnerText?.Trim();
                        bool isEmpty = string.IsNullOrWhiteSpace(text)
                                    || (p.ChildNodes.Count == 1 && p.ChildNodes[0].Name == "br");

                        if (isEmpty)
                        {
                            column.Item().Text("No additional data available.");
                        }
                        else
                        {
                            column.Item().Padding(1).Element(inner =>
                            {
                                inner.Text(text =>
                                {
                                    text.DefaultTextStyle(x => x.LineHeight(1.2f));
                                    foreach (var node in p.ChildNodes)
                                    {
                                        if (node.Name == "#text")
                                            text.Span(node.InnerText);
                                        else if (node.Name == "b")
                                            text.Span(node.InnerText).Bold();
                                        else if (node.Name == "u")
                                            text.Span(node.InnerText).Underline();
                                        else if (node.Name == "br")
                                            text.EmptyLine();
                                        else
                                            text.Span(node.InnerText); // fallback
                                    }
                                });
                            });
                        }
                    }
                }
                else
                {
                    column.Item().Text("No additional data available.");
                }
            });
        }
    }
}