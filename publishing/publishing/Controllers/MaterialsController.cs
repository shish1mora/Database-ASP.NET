using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using publishing.Models;
using publishing.Models.ViewModels;

namespace publishing.Controllers
{
    [Authorize(Roles ="admin,manager")]
    public class MaterialsController : Controller
    {
        private readonly PublishingDBContext _context;

        public MaterialsController(PublishingDBContext context)
        {
            _context = context;
        }

        // GET: Materials
        public IActionResult Index(string? type, string? color, string? size, double? startCost, double? endCost)
        {
            List<Material> materials = _context.Materials.Include(m => m.ProductMaterials).ToList();

            if (type != null)
                materials = materials.Where(m => m.Type == type).ToList();

            if(color != null)
                materials = materials.Where(m => m.Color == color).ToList();

            if(size != null)
                materials = materials.Where(m => m.Size == size).ToList();

            if (startCost != null & endCost == null)
                materials = materials.Where(m => m.Cost >= startCost.Value).ToList();
            else if (startCost == null & endCost != null)
                materials = materials.Where(m => m.Cost <= endCost.Value).ToList();
            else if (startCost != null & endCost != null)
                materials = materials.Where(m => m.Cost >= startCost.Value && m.Cost <= endCost.Value).ToList();

            return View(materials);          
        }

        // GET: Materials/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Materials == null)
            {
                return NotFound();
            }

            var material = await _context.Materials.Include(m=> m.ProductMaterials)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (material == null)
            {
                return NotFound();
            }

            var products = (from bp in _context.BookingProducts.Include(b => b.Product) where bp.BookingId != null select bp.Product).ToList();
            if(products == null)
                return NotFound();

            MaterialDetailsViewModel model = new MaterialDetailsViewModel();
            model.Material = material;

            foreach (var pr in material.ProductMaterials)
            {
                model.Products.AddRange(_context.Products.Include(p =>p.Customer).Where(p=> p.Id == pr.ProductId && products.Contains(p)));
            }

            return View(model);
            //return View(material);
        }

        // GET: Materials/Create
        [Authorize(Roles ="admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Materials/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Type,Color,Size,Cost")] Material material)
        {
            if (ModelState.IsValid)
            {
                if (ExistMaterial(material,"create"))
                    return RedirectToAction("Create");

                _context.Add(material);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(material);
        }

        private bool ExistMaterial(Material material, string typeAction) 
        {
            Material existMaterial = null;
            if (typeAction == "create")
                existMaterial = _context.Materials.FirstOrDefault(m => m.Type == material.Type && m.Color == material.Color && m.Size == material.Size && m.Cost == material.Cost);
            else if (typeAction == "edit")
                existMaterial = _context.Materials.FirstOrDefault(m => m.Type == material.Type && m.Color == material.Color && m.Size == material.Size && m.Cost == material.Cost && m.Id != material.Id);

            if (existMaterial != null)
                return true;

            return false;

        }

        // GET: Materials/Edit/5
        [Authorize(Roles ="admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Materials == null)
            {
                return NotFound();
            }

            var material =  _context.Materials.Include(m=>m.ProductMaterials).FirstOrDefault(m=>m.Id == id);
            if (material == null)
            {
                return NotFound();
            }

            if(material.ProductMaterials.Count > 0)
                return new StatusCodeResult(403);

            return View(material);
        }

        // POST: Materials/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Type,Color,Size,Cost")] Material material)
        {
            if (id != material.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (ExistMaterial(material,"edit"))
                        return RedirectToAction("Edit", new {id});

                    _context.Update(material);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MaterialExists(material.Id))
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
            return View(material);
        }

        // GET: Materials/Delete/5
        [Authorize(Roles ="admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Materials == null)
            {
                return NotFound();
            }

            var material = await _context.Materials.Include(m=>m.ProductMaterials)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (material == null)
            {
                return NotFound();
            }

            if (material.ProductMaterials.Count > 0)
                return new StatusCodeResult(403);

            return View(material);
        }

        // POST: Materials/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Materials == null)
            {
                return Problem("Entity set 'PublishingDBContext.Materials'  is null.");
            }
            var material = await _context.Materials.FindAsync(id);
            if (material != null)
            {
                _context.Materials.Remove(material);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MaterialExists(int id)
        {
          return (_context.Materials?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
