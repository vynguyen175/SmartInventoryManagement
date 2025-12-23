using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartInventoryManagement.Data;
using SmartInventoryManagement.Models;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;

namespace SmartInventoryManagement.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        public IActionResult Index(string search, int? categoryFilter, string sortOrder)
        {
            ViewData["searchQuery"] = search;
            ViewData["categoryFilter"] = categoryFilter;
            var products = _context.Products.Include(p => p.Category).AsQueryable();

            ViewBag.NameSortParam = sortOrder == "name" ? "name_desc" : "name";
            ViewBag.PriceSortParam = sortOrder == "price" ? "price_desc" : "price";
            ViewBag.QuantitySortParam = sortOrder == "quantity" ? "quantity_desc" : "quantity";

            if (!string.IsNullOrEmpty(search))
                products = products.Where(p => p.Name.Contains(search));

            if (categoryFilter.HasValue)
                products = products.Where(p => p.CategoryId == categoryFilter.Value);

            products = sortOrder switch
            {
                "name_desc" => products.OrderByDescending(p => p.Name),
                "price" => products.OrderBy(p => p.Price),
                "price_desc" => products.OrderByDescending(p => p.Price),
                "quantity" => products.OrderBy(p => p.QuantityInStock),
                "quantity_desc" => products.OrderByDescending(p => p.QuantityInStock),
                _ => products.OrderBy(p => p.Name),
            };

            ViewBag.Categories = _context.Categories.ToList();
            ViewBag.LowStockProducts = products.Where(p => p.QuantityInStock < p.LowStockThreshold).ToList();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_ProductList", products.ToList());

            return View(products.ToList());
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            var categories = _context.Categories.ToList();
            if (!categories.Any())
                ViewBag.Message = "⚠ No categories found. Please add categories first.";

            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Product product)
        {
            if (!_context.Categories.Any())
                ModelState.AddModelError("", "⚠ You must add at least one category before creating a product.");

            if (ModelState.IsValid)
            {
                try
                {
                    var category = _context.Categories.Find(product.CategoryId);
                    if (category == null)
                    {
                        ModelState.AddModelError("CategoryId", "Selected category does not exist.");
                        return View(product);
                    }

                    _context.Add(product);
                    _context.SaveChanges();
                    return Json(new { success = true, message = "Product created successfully!" });
                }
                catch
                {
                    ModelState.AddModelError("", "An unexpected error occurred while saving the product.");
                    return View(product);
                }
            }

            return View(product);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Edit(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null)
                return NotFound();

            ViewBag.Categories = new SelectList(_context.Categories.ToList(), "Id", "Name", product.CategoryId);
            return View(product);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Product product)
        {
            if (id != product.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    _context.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Products.Any(e => e.Id == product.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var product = _context.Products.Include(p => p.Category).FirstOrDefault(m => m.Id == id);
            if (product == null)
                return NotFound();

            return View(product);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var product = _context.Products.Find(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
