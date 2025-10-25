using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using STOCKER.Data;
using STOCKER.Models;

namespace STOCKER.Controllers
{
    [Authorize(Roles = "Admin,Cashier")]

    public class ProductCategoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductCategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Show all categories
        public IActionResult Index()
        {
            var categories = _context.ProductCategories.ToList();
            return View(categories);
        }

        // GET: show form
        public IActionResult Create()
        {
            return View();
        }

        // POST: Add category
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ProductCategory category)
        {
            if (ModelState.IsValid)
            {
                _context.ProductCategories.Add(category);
                _context.SaveChanges();

                return RedirectToAction("Create", "Products");
            }

            // Log validation errors (use Console, logger, or debugger)
            foreach (var state in ModelState)
            {
                foreach (var error in state.Value.Errors)
                {
                    Console.WriteLine($"Field: {state.Key}, Error: {error.ErrorMessage}");
                }
            }

            return View(category);
        }


        // Optional: use this if you want to POST from a modal and stay on Product form
        [HttpPost]
        public IActionResult AddQuickCategory(string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                var category = new ProductCategory { Name = name.Trim() };
                _context.ProductCategories.Add(category);
                _context.SaveChanges();

                return Ok(new { success = true, id = category.CategoryId, name = category.Name });
            }

            return BadRequest("Invalid category name");
        }

        // GET: /ProductCategory/Delete/5
        public IActionResult Delete(int id)
        {
            var category = _context.ProductCategories.Find(id);
            if (category == null) return NotFound();

            _context.ProductCategories.Remove(category);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}
