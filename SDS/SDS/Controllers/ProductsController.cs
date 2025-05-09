using Microsoft.AspNetCore.Mvc;

using Microsoft.EntityFrameworkCore;
using System.Data;
using SDS.Data;
using SDS.Models;

namespace MVCCRUD.Controllers
{
    public class ProductsController : Controller
    {
        private readonly SdsDbContext _context;
        private const string CurrentUser = "DefaultUser";

        public ProductsController(SdsDbContext context)
        {
            _context = context;
        }

        // GET: List all products
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .Select(p => new ProductViewModel
                {
                    Id = p.Id,
                    ProductCode = p.ProductCode,
                    ProductName = p.ProductName,

                }).ToListAsync();

            return View(products);
        }

        // GET: Show upload form
        public IActionResult ExcelUpload()
        {
            return View();
        }



        // GET: Product/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductViewModel model)
        {
            if (ModelState.IsValid)
            {
                var entity = new Product
                {
                    ProductCode = model.ProductCode,
                    ProductName = model.ProductName,
                    ProductNo = model.ProductNo,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Products.Add(entity);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: Product/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var p = await _context.Products.FindAsync(id);
            if (p == null) return NotFound();
            var model = new ProductViewModel
            {
                Id = p.Id,
                ProductCode = p.ProductCode,
                ProductName = p.ProductName,
                ProductNo = p.ProductNo,
            };
            return View(model);
        }

        // POST: Product/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductViewModel model)
        {
            if (id != model.Id) return BadRequest();

            if (ModelState.IsValid)
            {
                var entity = await _context.Products.FindAsync(id);
                if (entity == null) return NotFound();

                entity.ProductCode = model.ProductCode;
                entity.ProductName = model.ProductName;
                entity.ProductNo = model.ProductNo;
                entity.UpdatedAt = DateTime.UtcNow;

                _context.Products.Update(entity);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: Product/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var p = await _context.Products.FindAsync(id);
            if (p == null) return NotFound();

            var model = new ProductViewModel
            {
                Id = p.Id,
                ProductCode = p.ProductCode,
                ProductName = p.ProductName,
                ProductNo = p.ProductNo,
            };
            return View(model);
        }

        // POST: Product/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var entity = await _context.Products.FindAsync(id);
            if (entity == null) return NotFound();

            _context.Products.Remove(entity);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
