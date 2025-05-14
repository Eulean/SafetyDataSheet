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