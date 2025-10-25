using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using STOCKER.Data;
using STOCKER.Models;

namespace STOCKER.Controllers
{
    [Authorize(Roles = "Admin")]

    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Inventory)
                .ToListAsync();

            return View(products);
        }

        // CREATE (GET)
        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(_context.ProductCategories, "CategoryId", "Name");
            return View();
        }

        // CREATE (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Products product, int QuantityInStock)
        {
            if (ModelState.IsValid)
            {
                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                var inventory = new Inventory
                {
                    ProductId = product.ProductId,
                    QuantityInStock = QuantityInStock
                };

                _context.Inventories.Add(inventory);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = new SelectList(_context.ProductCategories, "ProductCategoryId", "Name", product.ProductCategoryId);
            return View(product);
        }

        // GET: Edit
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound();

            ViewBag.Categories = new SelectList(_context.ProductCategories, "CategoryId", "Name", product.ProductCategoryId);
            return View(product);
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Products product)
        {
            if (ModelState.IsValid)
            {
                _context.Products.Update(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = new SelectList(_context.ProductCategories, "CategoryId", "Name", product.ProductCategoryId);
            return View(product);
        }

        // GET: AddStock
        public async Task<IActionResult> AddStock(int id)
        {
            var product = await _context.Products
                .Include(p => p.Inventory)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            return product == null ? NotFound() : View(product);
        }

        // POST: AddStock
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddStock(int ProductId, int additionalStock, decimal newBuyingPrice)
        {
            var product = await _context.Products
                .Include(p => p.Inventory)
                .FirstOrDefaultAsync(p => p.ProductId == ProductId);

            if (product == null || product.Inventory == null)
            {
                return NotFound();
            }

            int currentStock = product.Inventory.QuantityInStock;
            decimal currentPrice = product.BuyingPrice;

            decimal totalCost = (currentPrice * currentStock) + (newBuyingPrice * additionalStock);
            int newTotalStock = currentStock + additionalStock;

            decimal updatedBuyingPrice = newTotalStock > 0
                ? totalCost / newTotalStock
                : newBuyingPrice;

            product.Inventory.QuantityInStock = newTotalStock;
            product.BuyingPrice = updatedBuyingPrice;

            // ✅ Record the stock addition
            var transaction = new InventoryTransaction
            {
                ProductId = ProductId,
                QuantityAdded = additionalStock,
                Timestamp = DateTime.Now,
                Type = "Add",
                BuyingPrice = newBuyingPrice // 🔥 This is the price at the time of addition
            };

            _context.InventoryTransaction.Add(transaction);

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products
                .Include(p => p.Inventory)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
                return NotFound();

            return View(product);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products
                .Include(p => p.Inventory)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
                return NotFound();

            // Delete related inventory if it exists
            if (product.Inventory != null)
            {
                _context.Inventories.Remove(product.Inventory);
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


    }
}