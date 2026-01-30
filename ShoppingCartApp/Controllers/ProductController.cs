using Microsoft.AspNetCore.Mvc;
using ShoppingCartApp.Models;
using Microsoft.EntityFrameworkCore;

namespace ShoppingCartApp.Controllers
{
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;

        public ProductController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _context.Products.ToListAsync();
            ViewBag.CartCount = _context.Items.Sum(i => i.Quantity);
            return View(products);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Product product, IFormFile imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    var fileName = Path.GetFileNameWithoutExtension(imageFile.FileName);
                    var extension = Path.GetExtension(imageFile.FileName);
                    var uniqueFileName = $"{fileName}_{Guid.NewGuid()}{extension}";

                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", uniqueFileName);

                    // Save the file to the specified path
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    // Save the file path to the database
                    product.ProductImage = "/images/" + uniqueFileName;
                }
                else
                {
                    ViewBag.CartCount = _context.Items.Any() ? _context.Items.Sum(i => i.Quantity) : 0;
                    ModelState.AddModelError("ProductImage", "Please upload an image.");
                    return View(product);
                }

                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            ViewBag.CartCount = _context.Items.Any() ? _context.Items.Sum(i => i.Quantity) : 0;
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Product product, IFormFile? imageFile)
        {
            if (id != product.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        var fileName = Path.GetFileNameWithoutExtension(imageFile.FileName);
                        var extension = Path.GetExtension(imageFile.FileName);
                        var uniqueFileName = $"{fileName}_{Guid.NewGuid()}{extension}";
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", uniqueFileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(stream);
                        }
                        product.ProductImage = "/images/" + uniqueFileName;
                    }
                    else
                    {
                        _context.Entry(product).Property(x => x.ProductImage).IsModified = false;
                    }

                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Products.Any(e => e.Id == product.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.CartCount = _context.Items.Any() ? _context.Items.Sum(i => i.Quantity) : 0;
            return View(product);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FirstOrDefaultAsync(m => m.Id == id);
            if (product == null) return NotFound();

            ViewBag.CartCount = _context.Items.Any() ? _context.Items.Sum(i => i.Quantity) : 0;
            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}