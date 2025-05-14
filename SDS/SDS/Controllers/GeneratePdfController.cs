using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using QuestPDF.Fluent;
using SDS.Data;
using SDS.Helper;
using SDS.Models;
using SDS.Services;
using System.Text.RegularExpressions;

namespace SDS.Controllers
{
    [Route("[controller]")]
    public class GeneratePdfController : Controller
    {

        private readonly SdsDbContext _context;
        private readonly IAntiforgery _antiforgery;
        private readonly IWebHostEnvironment _env;
        private readonly SdsService _sdsService;

        // Combined constructor that takes both dependencies
        public GeneratePdfController(SdsDbContext context,
            IAntiforgery antiforgery,
            IWebHostEnvironment env,
            SdsService sdsService)
        {
            _context = context;
            _antiforgery = antiforgery;
            _env = env;
            _sdsService = sdsService;
        }

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet("Generate/{productNo}")]
        public async Task<IActionResult> Generate(string productNo)
        {
            try
            {
                if (string.IsNullOrEmpty(productNo))
                {
                    return BadRequest("Product ID is required.");
                }

                var vm = await _sdsService.GetSdsViewModelByProductIdAsync(productNo);
                if (vm == null || string.IsNullOrEmpty(vm.ProductId))
                {
                    return NotFound($"Product {productNo}  not found.");
                }

                // Generate URL to PDF template endpoint
                var pdfHtmlUrl = Url.Action("Pdf", "GeneratePdf", new { productNo }, Request.Scheme);

                if (string.IsNullOrEmpty(pdfHtmlUrl))
                {
                    return BadRequest("Failed to generate PDF template URL");
                }

                // Configure PDF options
                var now = DateTime.Now;
                var imgsrc = ImageLogo.Prooli
                    ?? Functions.GetBase64Image("prooil.jpg", Path.Combine(_env.WebRootPath, "images"));

                var pdfOptions = new PdfOptions
                {
                    Format = PaperFormat.A4,
                    PrintBackground = true,
                    DisplayHeaderFooter = true,
                    MarginOptions = new MarginOptions
                    {
                        Top = "25mm",
                        Bottom = "40mm",
                    },
                    HeaderTemplate = @$"
                    <div style='width: 100%; font-size: 10px; border-bottom: 1px solid #333; padding: 2mm 5mm; line-height: 1.2;'>
                        <div style='display: flex; justify-content: space-between; align-items: center; height: 45px;'>
                            
                            <!-- Left Text -->
                            <div style='flex: 1;'>
                                <strong style='font-size: 11px;'>PRO-OILS AROMATHERAPY</strong><br>
                                MATERIAL SAFETY DATA SHEET
                            </div>

                            <!-- Centered Logo -->
                            <div style='width: 190px; height: 60px; display: flex; align-items: center; justify-content: center; flex-shrink: 0;'>
                                <img src='{imgsrc}' style='max-height: 100%; max-width: 100%; object-fit: contain;' />
                            </div>

                            <!-- Right Page Info -->
                            <div style='flex: 1; text-align: right;'>
                                Page <span class='pageNumber'></span> of <span class='totalPages'></span>
                            </div>
                            
                        </div>
                    </div>

",
                    FooterTemplate = @$"
                    <div style='width:100%; font-size:9px; color:#222; border-top:1px solid #333; padding-top:3mm; line-height:1.6; font-family:Arial,sans-serif;'>
                        <div style='display:grid; grid-template-columns:1fr 1fr; gap:8mm; margin:0 5mm 2mm;'>
                            <div style='line-height:1.4;'>
                                <strong style='display:block; margin-bottom:1mm;'>PRO-OILS AROMATHERAPY</strong>
                                 <div style='margin-top:1mm;'>Tel: {Functions.RemoveHtmlTags(vm.EmergencyPhone)}</div>
                                <div >info@prooils.com.au</div>
                                <div>Unit 6, 163 Newbridge Road,Chipping Norton NSW 2170 </div>                               
                            </div>
                            <div style='text-align:right; line-height:1.4;'>
                                <strong style='display:block; margin-bottom:3mm;'>REVISION DETAILS</strong>
                                <div>
                                <p style='display:block;> Date: {vm.RevisionDate ?? DateTime.Now:yyyy-MM-dd}</p>
                                 <p style='display:block;> Rev No: {vm.RevNo}</p>
                                </div>
                            </div>
                        </div>
                        <div style='font-size:10px; font-weight:600; text-align:center; margin-top:2mm; padding-bottom:1mm;'>
                            PAGE <span class='pageNumber'></span> OF <span class='totalPages'></span>
                        </div>
                    </div>"
                };

                // Generate PDF using helper
                var pdfBytes = await HtmlToPdfGenerateHelper.GenerateAsync(pdfHtmlUrl, pdfOptions);

                // Return PDF file
                return File(pdfBytes, "application/pdf", $"{productNo}_{(vm.RevisionDate ?? DateTime.Now):yyyy-mm-dd}.pdf");
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

        [HttpGet("Pdf/{productNo}")]
        public async Task<IActionResult> Pdf(string productNo)
        {
            try
            {
                var vm = await _sdsService.GetSdsViewModelByProductIdAsync(productNo);
                if (vm == null) return NotFound();
                return View("~/Views/GeneratePdf/GeneratePdf.cshtml", vm);
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

        [HttpGet("GenPdf/{productNo}")]
        public async Task<IActionResult> GenPdf(string productNo)
        {
            try
            {
                if (string.IsNullOrEmpty(productNo))
                {
                    return BadRequest("Product No is required");
                }

                var vm = await _sdsService.GetSdsViewModelByProductIdAsync(productNo);
                if (vm == null || string.IsNullOrEmpty(vm.ProductId))
                {
                    return NotFound($"Product {productNo} not found.");
                }

                //Configure QuestPDF
                QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

                // Generate PDF using QuestPDF
                var pdfBytes = await Task.Run(() =>
                {
                    using var stream = new MemoryStream();

                    // create document
                    var document = new SdsDocuments(vm, _env);
                    document.GeneratePdf(stream);

                    return stream.ToArray();
                });

                // Return PDF file
                return File(pdfBytes, "application/pdf", $"{productNo}_{(vm.RevisionDate ?? DateTime.Now):yyyy-MM-dd}.pdf");
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

        [HttpGet("Preview/{productNo}")]
        public async Task<IActionResult> Preview(string productNo)
        {
            try
            {
                if (string.IsNullOrEmpty(productNo))
                {
                    return BadRequest("Product ID is required.");
                }

                var vm = await _sdsService.GetSdsViewModelByProductIdAsync(productNo);
                if (vm == null || string.IsNullOrEmpty(vm.ProductId))
                {
                    return NotFound($"Product {productNo} not found.");
                }

                // enable debugging 
                QuestPDF.Settings.EnableDebugging = true;

                // Configure QuestPDF
                QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;



                // Generate PDF using QuestPDF
                var pdfBytes = await Task.Run(() =>
                {
                    using var stream = new MemoryStream();

                    // Create a document
                    var document = new SdsDocuments(vm, _env);
                    document.GeneratePdf(stream);

                    return stream.ToArray();
                });

                // Return PDF file with inline content disposition to display in browser
                return File(pdfBytes, "application/pdf", fileDownloadName: null);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error generating preview: " + ex.Message);
            }
        }

        [HttpGet("PreviewPage/{productNo}")]
        public IActionResult PreviewPage(string productNo)
        {
            ViewBag.ProductNo = productNo;
            return View();
        }

    }

}
