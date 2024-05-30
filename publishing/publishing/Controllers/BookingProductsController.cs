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
    public class BookingProductsController : Controller
    {
        private readonly PublishingDBContext _context;

        public BookingProductsController(PublishingDBContext context)
        {
            _context = context;
        }

        // GET: BookingProducts
        public async Task<IActionResult> Index()
        {
            var publishingDBContext = _context.BookingProducts.Include(b => b.Booking).Include(b => b.Product);
            return View(await publishingDBContext.ToListAsync());
        }

        // GET: BookingProducts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.BookingProducts == null)
            {
                return NotFound();
            }

            var bookingProduct = await _context.BookingProducts
                .Include(b => b.Booking)
                .Include(b => b.Product)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (bookingProduct == null)
            {
                return NotFound();
            }

            return View(bookingProduct);
        }

        // GET: BookingProducts/Create
        public IActionResult Create()
        {
            ViewData["BookingId"] = new SelectList(_context.Bookings, "Id", "Id");
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Name");
            return View();
        }

        // POST: BookingProducts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Edition,BookingId,ProductId")] BookingProduct bookingProduct)
        {
            if (ModelState.IsValid)
            {
                _context.Add(bookingProduct);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["BookingId"] = new SelectList(_context.Bookings, "Id", "Id", bookingProduct.BookingId);
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Name", bookingProduct.ProductId);
            return View(bookingProduct);
        }

        // GET: BookingProducts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.BookingProducts == null)
            {
                return NotFound();
            }

            var bookingProduct = await _context.BookingProducts.FindAsync(id);
            if (bookingProduct == null)
            {
                return NotFound();
            }
            ViewData["BookingId"] = new SelectList(_context.Bookings, "Id", "Id", bookingProduct.BookingId);
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Name", bookingProduct.ProductId);
            return View(bookingProduct);
        }

        // POST: BookingProducts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Edition,BookingId,ProductId")] BookingProduct bookingProduct)
        {
            if (id != bookingProduct.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(bookingProduct);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookingProductExists(bookingProduct.Id))
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
            ViewData["BookingId"] = new SelectList(_context.Bookings, "Id", "Id", bookingProduct.BookingId);
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Name", bookingProduct.ProductId);
            return View(bookingProduct);
        }

        // GET: BookingProducts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.BookingProducts == null)
            {
                return NotFound();
            }

            var bookingProduct = await _context.BookingProducts
                .Include(b => b.Booking)
                .Include(b => b.Product)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (bookingProduct == null)
            {
                return NotFound();
            }

            return View(bookingProduct);
        }

        // POST: BookingProducts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.BookingProducts == null)
            {
                return Problem("Entity set 'PublishingDBContext.BookingProducts'  is null.");
            }
            var bookingProduct = await _context.BookingProducts.FindAsync(id);
            if (bookingProduct != null)
            {
                _context.BookingProducts.Remove(bookingProduct);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookingProductExists(int id)
        {
          return (_context.BookingProducts?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
