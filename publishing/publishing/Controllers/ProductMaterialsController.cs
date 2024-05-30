using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using publishing.Models;

namespace publishing.Controllers
{
    [Authorize(Roles ="admin,manager")]
    public class ProductMaterialsController : Controller
    {
        private readonly PublishingDBContext _context;

        public ProductMaterialsController(PublishingDBContext context)
        {
            _context = context;
        }

        // GET: ProductMaterials
        public async Task<IActionResult> Index()
        {
            var publishingDBContext = _context.ProductMaterials.Include(p => p.Material).Include(p => p.Product);
            return View(await publishingDBContext.ToListAsync());
        }

        // GET: ProductMaterials/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.ProductMaterials == null)
            {
                return NotFound();
            }

            var productMaterial = await _context.ProductMaterials
                .Include(p => p.Material)
                .Include(p => p.Product)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (productMaterial == null)
            {
                return NotFound();
            }

            return View(productMaterial);
        }

        // GET: ProductMaterials/Create
        public IActionResult Create()
        {
            ViewData["MaterialId"] = new SelectList(_context.Materials, "Id", "Type");
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Name");
            return View();
        }

        // POST: ProductMaterials/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CountMaterials,MaterialId,ProductId")] ProductMaterial productMaterial)
        {
            if (ModelState.IsValid)
            {
                _context.Add(productMaterial);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MaterialId"] = new SelectList(_context.Materials, "Id", "Type", productMaterial.MaterialId);
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Name", productMaterial.ProductId);
            return View(productMaterial);
        }

        // GET: ProductMaterials/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.ProductMaterials == null)
            {
                return NotFound();
            }

            var productMaterial = await _context.ProductMaterials.FindAsync(id);
            if (productMaterial == null)
            {
                return NotFound();
            }
            ViewData["MaterialId"] = new SelectList(_context.Materials, "Id", "Type", productMaterial.MaterialId);
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Name", productMaterial.ProductId);
            return View(productMaterial);
        }

        // POST: ProductMaterials/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CountMaterials,MaterialId,ProductId")] ProductMaterial productMaterial)
        {
            if (id != productMaterial.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(productMaterial);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductMaterialExists(productMaterial.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["MaterialId"] = new SelectList(_context.Materials, "Id", "Type", productMaterial.MaterialId);
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Name", productMaterial.ProductId);
            return View(productMaterial);
        }

        // GET: ProductMaterials/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.ProductMaterials == null)
            {
                return NotFound();
            }

            var productMaterial = await _context.ProductMaterials
                .Include(p => p.Material)
                .Include(p => p.Product)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (productMaterial == null)
            {
                return NotFound();
            }

            return View(productMaterial);
        }

        // POST: ProductMaterials/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.ProductMaterials == null)
            {
                return Problem("Entity set 'PublishingDBContext.ProductMaterials'  is null.");
            }
            var productMaterial = await _context.ProductMaterials.FindAsync(id);
            if (productMaterial != null)
            {
                _context.ProductMaterials.Remove(productMaterial);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductMaterialExists(int id)
        {
          return (_context.ProductMaterials?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
