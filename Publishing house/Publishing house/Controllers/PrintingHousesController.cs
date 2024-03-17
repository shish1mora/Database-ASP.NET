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
    public class PrintingHousesController : Controller
    {
        private readonly PublishingDBcontext _context;

        public PrintingHousesController(PublishingDBcontext context)
        {
            _context = context;
        }

        // GET: PrintingHouses
        public async Task<IActionResult> Index()
        {
              return _context.PrintingHouses != null ? 
                          View(await _context.PrintingHouses.ToListAsync()) :
                          Problem("Entity set 'PublishingDBcontext.PrintingHouses'  is null.");
        }

        // GET: PrintingHouses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.PrintingHouses == null)
            {
                return NotFound();
            }

            var printingHouse = await _context.PrintingHouses
                .FirstOrDefaultAsync(m => m.Id == id);
            if (printingHouse == null)
            {
                return NotFound();
            }

            return View(printingHouse);
        }

        // GET: PrintingHouses/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: PrintingHouses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Phone,Email,State")] PrintingHouse printingHouse)
        {
            if (ModelState.IsValid)
            {
                _context.Add(printingHouse);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(printingHouse);
        }

        // GET: PrintingHouses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.PrintingHouses == null)
            {
                return NotFound();
            }

            var printingHouse = await _context.PrintingHouses.FindAsync(id);
            if (printingHouse == null)
            {
                return NotFound();
            }
            return View(printingHouse);
        }

        // POST: PrintingHouses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Phone,Email,State")] PrintingHouse printingHouse)
        {
            if (id != printingHouse.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(printingHouse);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PrintingHouseExists(printingHouse.Id))
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
            return View(printingHouse);
        }

        // GET: PrintingHouses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.PrintingHouses == null)
            {
                return NotFound();
            }

            var printingHouse = await _context.PrintingHouses
                .FirstOrDefaultAsync(m => m.Id == id);
            if (printingHouse == null)
            {
                return NotFound();
            }

            return View(printingHouse);
        }

        // POST: PrintingHouses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.PrintingHouses == null)
            {
                return Problem("Entity set 'PublishingDBcontext.PrintingHouses'  is null.");
            }
            var printingHouse = await _context.PrintingHouses.FindAsync(id);
            if (printingHouse != null)
            {
                _context.PrintingHouses.Remove(printingHouse);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PrintingHouseExists(int id)
        {
          return (_context.PrintingHouses?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
