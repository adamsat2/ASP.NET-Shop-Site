using Microsoft.AspNetCore.Mvc;
using ShoppingCartApp.Models;
using Microsoft.EntityFrameworkCore;

namespace ShoppingCartApp.Controllers
{
    public class ItemController : Controller
    {
        private readonly AppDbContext _context;

        public ItemController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var cartItems = await _context.Items.ToListAsync();

            ViewBag.Products = await _context.Products.ToListAsync();

            ViewBag.TotalPrice = cartItems.Sum(i => i.Price * i.Quantity);
            ViewBag.CartCount = cartItems.Sum(i => i.Quantity);

            return View(cartItems);
        }

        public async Task<IActionResult> Add(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            var item = new Item
            {
                Name = product.Name,
                Price = product.Price,
                Quantity = 1
            };

            ViewBag.CartCount = _context.Items.Any() ? _context.Items.Sum(i => i.Quantity) : 0;
            return View(item);
        }

        [HttpPost]
        public async Task<IActionResult> Add(Item item)
        {
            if (item.Quantity < 1 || item.Quantity > 100)
            {
                ModelState.AddModelError("Quantity", "The field Quantity must be between 1 and 100");
            }

            if (ModelState.IsValid)
            {
                var existingItem = await _context.Items
                    .FirstOrDefaultAsync(i => i.Name == item.Name);

                if (existingItem != null)
                {
                    existingItem.Quantity += item.Quantity;
                }
                else
                {
                    _context.Items.Add(item);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Home");
            }

            ViewBag.CartCount = _context.Items.Any() ? _context.Items.Sum(i => i.Quantity) : 0;
            return View(item);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var item = await _context.Items.FindAsync(id);
            if (item == null) return NotFound();

            ViewBag.CartCount = _context.Items.Any() ? _context.Items.Sum(i => i.Quantity) : 0;
            return View(item);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Item item)
        {
            if (id != item.Id) return NotFound();

            if (item.Quantity < 1 || item.Quantity > 100)
            {
                ModelState.AddModelError("Quantity", "The field Quantity must be between 1 and 100");
            }

            if (ModelState.IsValid)
            {
                _context.Update(item);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.CartCount = _context.Items.Any() ? _context.Items.Sum(i => i.Quantity) : 0;
            return View(item);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.Items.FindAsync(id);
            if (item != null)
            {
                _context.Items.Remove(item);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}