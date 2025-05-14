using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
                    // row.ConstantItem(220).Height(70).AlignCenter().AlignMiddle()
                    //     .Image(_logPath).FitArea();
                    // In your ComposeHeader method
                    // centered logo
                    if (_logPath != null && File.Exists(_logPath))
                    {
                        row.ConstantItem(200).Height(70).AlignCenter().AlignMiddle()
                            .Image(_logPath).FitArea();
                    }
                    else
                    {
                        // Fallback when image is not available
                        row.ConstantItem(220).Height(70).AlignCenter().AlignMiddle()
                            .Text("PRO-OILS").FontSize(16).Bold();
                    }


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

                // Section 1
                column.Item().Element(c => c.Column(column =>
                {
                    column.Spacing(5);
                    column.Item().Text("01. IDENTIFICATION OF THE SUBSTANCE/PREPARATION & THE COMPANY/UNDERTAKING")
                        .FontSize(10)
                        .FontColor(Colors.Black)
                        .Bold()
                        .Underline();
                    column.Item().Height(2);
                }));
                column.Item().Element(ComposeSection1);
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

        void ComposeSection1(IContainer container)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(30);   // Section number column
                    columns.RelativeColumn(2);    // Label column / first content column
                    columns.RelativeColumn(3);    // Main content column
                    columns.RelativeColumn(2);    // FEMA label column
                    columns.RelativeColumn(3);    // FEMA value column
                    columns.RelativeColumn(2);    // EINECS label column
                    columns.RelativeColumn(3);    // EINECS value column
                });

                // Section 1.1 header
                table.Cell().Background("#002060").Padding(5).Text("1.1")
                    .FontColor(Colors.White).Bold();
                table.Cell().ColumnSpan(6).Background("#002060").Padding(5).Text("Product Identifier")
                    .FontColor(Colors.White).Bold();

                // 1.1 Product Name
                table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignLeft().Text("");
                table.Cell().ColumnSpan(2).Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignLeft().Text("Product Name").Bold();
                table.Cell().ColumnSpan(4).Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignLeft().Text("Example Product Name");

                // 1.1 Biological Definition
                table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignLeft().Text("");
                table.Cell().ColumnSpan(2).Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignLeft().Text("Biological Definition").Bold();
                table.Cell().ColumnSpan(4).Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignLeft().Text("Example biological definition of product.");

                // 1.1 INCI Name
                table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignLeft().Text("");
                table.Cell().ColumnSpan(2).Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignLeft().Text("INCI Name").Bold();
                table.Cell().ColumnSpan(4).Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignLeft().Text("Example INCI Name");

                // 1.1 CAS/FEMA/EINECS
                table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignLeft().Text("");
                table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignLeft().Text("CAS No:");
                table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignLeft().Text("111-22-3");
                table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignLeft().Text("FEMA No:");
                table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignLeft().Text("1234");
                table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignLeft().Text("EINECS No:");
                table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignLeft().Text("234-567-8");

                // Section 1.2 header
                table.Cell().Background("#002060").Padding(5).Text("1.2")
                    .FontColor(Colors.White).Bold();
                table.Cell().ColumnSpan(6).Background("#002060").Padding(5).Text("Identified Uses")
                    .FontColor(Colors.White).Bold();

                // 1.2 Identified Uses content
                table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignLeft().Text("");
                table.Cell().ColumnSpan(6).Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignLeft()
                    .Text("Example identified uses or restrictions.");

                // Section 1.3 header
                table.Cell().Background("#002060").Padding(5).Text("1.3")
                    .FontColor(Colors.White).Bold();
                table.Cell().ColumnSpan(6).Background("#002060").Padding(5).Text("Supplier Details")
                    .FontColor(Colors.White).Bold();

                // 1.3 Supplier Details content
                table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignLeft().Text("");
                table.Cell().ColumnSpan(6).Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignLeft()
                    .Text("Example Supplier Name\n1234 Example Street\nCity, Country");

                // Section 1.4 header
                table.Cell().Background("#002060").Padding(5).Text("1.4")
                    .FontColor(Colors.White).Bold();
                table.Cell().ColumnSpan(6).Background("#002060").Padding(5).Text("Emergency Telephone Number")
                    .FontColor(Colors.White).Bold();

                // 1.4 Emergency Tel content
                table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignLeft().Text("");
                table.Cell().ColumnSpan(6).Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignLeft()
                    .Text("Example: +1-800-123-4567");
            });
        }
    }
}