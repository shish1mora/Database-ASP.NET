using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using publishing.Models;


namespace publishing.Controllers
{
    [Authorize(Roles ="admin,manager")]
    public class PrintingHousesController : Controller
    {
        private readonly PublishingDBContext _context;

        public PrintingHousesController(PublishingDBContext context)
        {
            _context = context;
        }

        // GET: PrintingHouses
        public IActionResult Index(string? name, string? phone, string? email, string? state, string? city, string? street, string? house)
        {
            List<PrintingHouse> printingHouses = _context.PrintingHouses.ToList();

            if (name != null)
                printingHouses = printingHouses.Where(p => p.Name == name).ToList();

            if (phone != null)
                printingHouses = printingHouses.Where(p => p.Phone == phone).ToList();

            if (email != null)
                printingHouses = printingHouses.Where(p => p.Email == email).ToList();

            if (state != null)
                printingHouses = printingHouses.Where(p => p.State == state).ToList();

            if (city != null)
                printingHouses = printingHouses.Where(p => p.City == city).ToList();

            if (street != null)
                printingHouses = printingHouses.Where(p => p.Street == street).ToList();

            if (house != null)
                printingHouses = printingHouses.Where(p => p.House == house).ToList();


            return View(printingHouses);
        }

        // GET: PrintingHouses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.PrintingHouses == null)
            {
                return NotFound();
            }

            var printingHouse = await _context.PrintingHouses.Include(b=> b.Bookings)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (printingHouse == null)
            {
                return NotFound();
            }

            //PrintingHouseDetailsViewModel viewModel = new PrintingHouseDetailsViewModel();
            //viewModel.PrintingHouse = printingHouse;
            //viewModel.Bookings = _context.Bookings.Where(b=> b.PrintingHouseId == printingHouse.Id);

            return View(printingHouse);
            //return View(viewModel);
        }

        // GET: PrintingHouses/Create
        [Authorize(Roles = "admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: PrintingHouses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Phone,Email,State,City,Street,House,Description")] PrintingHouse printingHouse)
        {
            if (ModelState.IsValid)
            {
                //if (!UniqueAddress(printingHouse,"create"))
                //    return RedirectToAction("Create");

                _context.Add(printingHouse);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(printingHouse);
        }

        // GET: PrintingHouses/Edit/5
        [Authorize(Roles ="admin")]
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
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Phone,Email,State,City,Street,House,Description")] PrintingHouse printingHouse)
        {
            if (id != printingHouse.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                //if (!UniqueAddress(printingHouse, "edit"))
                //    return RedirectToAction("Edit", new {id});

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
        [Authorize(Roles = "admin")]
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
                return Problem("Entity set 'PublishingDBContext.PrintingHouses'  is null.");
            }

            //var bookings = _context.Bookings.Where(b => b.PrintingHouseId == id).ToList();

            //foreach (var booking in bookings)
            //{
            //    booking.PrintingHouseId = null;
            //}

            var printingHouse = _context.PrintingHouses.Include(p=>p.Bookings).FirstOrDefault(p=>p.Id == id);

            if (printingHouse != null)
            {
                foreach (var booking in printingHouse.Bookings)
                {
                    booking.PrintingHouseId = null;
                }

                _context.PrintingHouses.Remove(printingHouse);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PrintingHouseExists(int id)
        {
            return (_context.PrintingHouses?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        public IActionResult LinkPrintingHouseWithBooking(int? id) 
        {
            if(id == null)
                return NotFound();

            PrintingHouse printingHouse = _context.PrintingHouses.Find(id);
            if (printingHouse == null)
                return NotFound();

            ViewBag.printHouseId = id;
            return View(_context.Bookings.Where(b => b.PrintingHouseId == null && b.Status == "Выполняется"));
        }

        [HttpPost]
        public async Task<IActionResult> LinkPrintingHouseWithBooking(int? printHouseId, int[]? selectedBookings)
        {
            if (printHouseId == null || selectedBookings == null)
                return NotFound();

            foreach (var bookingId in selectedBookings)
            {
                Booking booking = _context.Bookings.Find(bookingId);
                if (booking == null)
                    return NotFound();

                booking.PrintingHouseId = printHouseId;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = printHouseId});
        }

        public async Task<IActionResult> UnpinOrder(int? id)
        {
            if (id == null)
                return NotFound();

            Booking booking = _context.Bookings.Find(id);
            if (booking == null)
                return NotFound();

            booking.PrintingHouseId = null;
            await _context.SaveChangesAsync();

            return Redirect(Request.Headers["Referer"].ToString());
        }

        //private bool UniqueAddress(PrintingHouse printingHouse, string typeAction) 
        //{
        //    PrintingHouse existPrintingHouse = null;

        //    if (typeAction == "create")
        //        existPrintingHouse = _context.PrintingHouses.FirstOrDefault(p => p.State == printingHouse.State && p.City == printingHouse.City && p.Street == printingHouse.Street && p.House == printingHouse.House);
        //    else if(typeAction == "edit")
        //        existPrintingHouse = _context.PrintingHouses.FirstOrDefault(p => (p.State == printingHouse.State && p.City == printingHouse.City && p.Street == printingHouse.Street && p.House == printingHouse.House) & p.Id != printingHouse.Id);

        //    if (existPrintingHouse == null)
        //        return true;

        //    return false;

        //}
    }
}


