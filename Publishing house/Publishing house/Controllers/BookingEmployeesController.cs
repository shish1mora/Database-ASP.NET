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
    public class BookingEmployeesController : Controller
    {
        private readonly PublishingDBcontext _context;

        public BookingEmployeesController(PublishingDBcontext context)
        {
            _context = context;
        }

        // GET: BookingEmployees
        public async Task<IActionResult> Index()
        {
            var publishingDBcontext = _context.BookingEmployees.Include(b => b.Booking).Include(b => b.Employee);
            return View(await publishingDBcontext.ToListAsync());
        }

        // GET: BookingEmployees/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.BookingEmployees == null)
            {
                return NotFound();
            }

            var bookingEmployee = await _context.BookingEmployees
                .Include(b => b.Booking)
                .Include(b => b.Employee)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (bookingEmployee == null)
            {
                return NotFound();
            }

            return View(bookingEmployee);
        }

        // GET: BookingEmployees/Create
        public IActionResult Create()
        {
            ViewData["BookingId"] = new SelectList(_context.Bookings, "Id", "Status");
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "Email");
            return View();
        }

        // POST: BookingEmployees/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,BookingId,EmployeeId")] BookingEmployee bookingEmployee)
        {
            if (ModelState.IsValid)
            {
                _context.Add(bookingEmployee);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["BookingId"] = new SelectList(_context.Bookings, "Id", "Status", bookingEmployee.BookingId);
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "Email", bookingEmployee.EmployeeId);
            return View(bookingEmployee);
        }

        // GET: BookingEmployees/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.BookingEmployees == null)
            {
                return NotFound();
            }

            var bookingEmployee = await _context.BookingEmployees.FindAsync(id);
            if (bookingEmployee == null)
            {
                return NotFound();
            }
            ViewData["BookingId"] = new SelectList(_context.Bookings, "Id", "Status", bookingEmployee.BookingId);
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "Email", bookingEmployee.EmployeeId);
            return View(bookingEmployee);
        }

        // POST: BookingEmployees/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,BookingId,EmployeeId")] BookingEmployee bookingEmployee)
        {
            if (id != bookingEmployee.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(bookingEmployee);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookingEmployeeExists(bookingEmployee.Id))
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
            ViewData["BookingId"] = new SelectList(_context.Bookings, "Id", "Status", bookingEmployee.BookingId);
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "Email", bookingEmployee.EmployeeId);
            return View(bookingEmployee);
        }

        // GET: BookingEmployees/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.BookingEmployees == null)
            {
                return NotFound();
            }

            var bookingEmployee = await _context.BookingEmployees
                .Include(b => b.Booking)
                .Include(b => b.Employee)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (bookingEmployee == null)
            {
                return NotFound();
            }

            return View(bookingEmployee);
        }

        // POST: BookingEmployees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.BookingEmployees == null)
            {
                return Problem("Entity set 'PublishingDBcontext.BookingEmployees'  is null.");
            }
            var bookingEmployee = await _context.BookingEmployees.FindAsync(id);
            if (bookingEmployee != null)
            {
                _context.BookingEmployees.Remove(bookingEmployee);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookingEmployeeExists(int id)
        {
          return (_context.BookingEmployees?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
