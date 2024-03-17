using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Publishing_house.Models;

namespace Publishing_house.Controllers
{
    public class PublishingsController : Controller
    {
        private readonly PublishingDBcontext _context;

        public PublishingsController(PublishingDBcontext context)
        {
            _context = context;
        }

        // GET: Publishings
        public async Task<IActionResult> Index()
        {
              return _context.Publishings != null ? 
                          View(await _context.Publishings.ToListAsync()) :
                          Problem("Entity set 'PublishingDBcontext.Publishings'  is null.");
        }

        // GET: Publishings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Publishings == null)
            {
                return NotFound();
            }

            var publishing = await _context.Publishings
                .FirstOrDefaultAsync(m => m.ID == id);
            if (publishing == null)
            {
                return NotFound();
            }

            return View(publishing);
        }

        // GET: Publishings/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Publishings/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Year,Price")] Publishing publishing)
        {
            if (ModelState.IsValid)
            {
                _context.Add(publishing);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(publishing);
        }

        // GET: Publishings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Publishings == null)
            {
                return NotFound();
            }

            var publishing = await _context.Publishings.FindAsync(id);
            if (publishing == null)
            {
                return NotFound();
            }
            return View(publishing);
        }

        // POST: Publishings/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Year,Price")] Publishing publishing)
        {
            if (id != publishing.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(publishing);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PublishingExists(publishing.ID))
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
            return View(publishing);
        }

        // GET: Publishings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Publishings == null)
            {
                return NotFound();
            }

            var publishing = await _context.Publishings
                .FirstOrDefaultAsync(m => m.ID == id);
            if (publishing == null)
            {
                return NotFound();
            }

            return View(publishing);
        }

        // POST: Publishings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Publishings == null)
            {
                return Problem("Entity set 'PublishingDBcontext.Publishings'  is null.");
            }
            var publishing = await _context.Publishings.FindAsync(id);
            if (publishing != null)
            {
                _context.Publishings.Remove(publishing);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PublishingExists(int id)
        {
          return (_context.Publishings?.Any(e => e.ID == id)).GetValueOrDefault();
        }
    }
}
