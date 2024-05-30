using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using publishing.Areas.Identity.Data;
using publishing.Infrastructure;
using publishing.Models;
using publishing.Models.ViewModels;

namespace publishing.Controllers
{
    [Authorize]
    public class BookingsController : Controller
    {
        private readonly PublishingDBContext _context;
        private readonly UserManager<publishingUser> _userManager;
        private readonly IWebHostEnvironment _appEnvironment;

        public BookingsController(PublishingDBContext context, UserManager<publishingUser> userManager, IWebHostEnvironment appEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _appEnvironment = appEnvironment;
        }

        // GET: Bookings
        [Authorize(Roles ="admin,manager")]
        public async Task<IActionResult> Index(string? status, string? date, DateTime? startDate, DateTime? endDate, int? startNumber, int? endNumber, double? startCost, double? endCost)
        {
            List<Booking> bookings = _context.Bookings.Include(b => b.PrintingHouse).Include(b => b.Employees).ToList();
            FilterController filterController = new FilterController(_context);


            if (status != null && status != "Все") 
            {
               bookings = filterController.GetBookingWithCertainStatus(status,bookings);
            }

            if (startDate != null || endDate != null) 
            {
                bookings = filterController.GetBookingWithSertainDate(date, startDate, endDate, bookings);
            }


            if (startNumber != null || endNumber != null) 
            {
                bookings = filterController.GetBookingWithSertainNumber(startNumber, endNumber, bookings);
            }

            if (startCost != null || endCost != null) 
            { 
                bookings = filterController.GetBookingsWithCertainCost(startCost, endCost, bookings);
            }

            SelectList statuses = new SelectList(new List<string> { "Все", "Ожидание", "Выполняется", "Выполнен" });
            SelectList dates = new SelectList(new List<string> { "Дата приёма", "Дата выполнения" });
            ViewBag.statuses = statuses;
            ViewBag.dates = dates;


            return View(bookings);
        }

        // GET: Bookings/Detail
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Bookings == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings
                .Include(b => b.PrintingHouse).Include(b=>b.Employees).Include(b=>b.BookingProducts)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (booking == null)
            {
                return NotFound();
            }

            if (!IsUserBooking(booking.Id))
                return new StatusCodeResult(403);

            var products = (from bp in _context.BookingProducts.Include(bp => bp.Product).Include(bp => bp.Booking) where bp.BookingId == id select bp.Product).ToList();
            if (products == null)
                return NotFound();

            var modelProducts = _context.Products.Include(p => p.Customer).Where(p => products.Contains(p)).ToList();
            if (modelProducts == null)
                return NotFound();

            //var bookingProducts = (from bp in _context.BookingProducts.Include(bp => bp.Product).Include(bp => bp.Booking) where bp.BookingId == id select bp).ToList();
            //_context.Products.Include(p => p.Customer);
            BookingDetailsViewModel model = new BookingDetailsViewModel();
            model.Booking = booking;
            model.BookingProducts = (from bp in _context.BookingProducts.Include(bp => bp.Product).Include(bp => bp.Booking) where bp.BookingId == id select bp).ToList();
            //model.Products = modelProducts;

            return View(model);
        }

        //// GET: Bookings/Create
        //[Authorize(Roles ="customer")]
        //public IActionResult Create()
        //{
        //    ViewData["PrintingHouseId"] = new SelectList(_context.PrintingHouses, "Id", "Name");
        //    return View();
        //}

        //// POST: Bookings/Create
        //// To protect from overposting attacks, enable the specific properties you want to bind to.
        //// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("Id,Start,End,Status,Cost,PrintingHouseId")] Booking booking)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _context.Add(booking);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }
        //    ViewData["PrintingHouseId"] = new SelectList(_context.PrintingHouses, "Id", "Name", booking.PrintingHouseId);
        //    return View(booking);
        //}

        // GET: Bookings/Edit/5
        [Authorize(Roles ="manager,admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Bookings == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            if(booking.Status != "Выполняется")
                return new StatusCodeResult(403);

            ViewData["PrintingHouseId"] = new SelectList(_context.PrintingHouses, "Id", "Name", booking.PrintingHouseId);
            return View(booking);
        }

        // POST: Bookings/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Start,End,Status,Cost,PrintingHouseId")] Booking booking)
        {
            if (id != booking.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    Customer customer = GetBookingCustomer(id);
                    string subject = $"Изменение в заказе №{id}";
                    string message = $"Уважаемый(-ая) {customer.Name}! Дата выполнения вашего заказа перенесена на {booking.End.Value.ToString("dd/MM/yyyy")}.<br>С уважением, \"Издательство\".";
                    EmailSender emailSender = new EmailSender();
                    emailSender.SendEmail(customer.Email, subject, message);

                    _context.Update(booking);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookingExists(booking.Id))
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
            ViewData["PrintingHouseId"] = new SelectList(_context.PrintingHouses, "Id", "Name", booking.PrintingHouseId);
            return View(booking);
        }

        // GET: Bookings/Delete/5
        [Authorize(Roles ="customer")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Bookings == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings
                .Include(b => b.PrintingHouse)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (booking == null)
            {
                return NotFound();
            }

            //var user = await _userManager.GetUserAsync(HttpContext.User);
            //if (!await _userManager.IsInRoleAsync(user, "admin"))
            //{
            //    if (!IsUserBooking(booking.Id, user.Email))
            //        return new StatusCodeResult(403);
            //}


            if (booking.Status != "Ожидание")
                return new StatusCodeResult(403);

            if (!IsUserBooking(booking.Id))
                return new StatusCodeResult(403);

            return View(booking);
        }

        // POST: Bookings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Bookings == null)
            {
                return Problem("Entity set 'PublishingDBContext.Bookings'  is null.");
            }
            var booking = await _context.Bookings.FindAsync(id);
            if (booking != null)
            {
                _context.Bookings.Remove(booking);
            }
            
            await _context.SaveChangesAsync();
            //return RedirectToAction(nameof(Index));
            return RedirectToAction("CustomerBookings", "Customers", new { emailCustomer = _userManager.GetUserAsync(HttpContext.User).Result.Email });
        }

        private bool BookingExists(int id)
        {
          return (_context.Bookings?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        [Authorize(Roles ="customer")]
        public async Task<IActionResult> UnpinProduct(int? bookingId, int? productId)
        {
            if (bookingId == null || productId == null)
                return NotFound();

            //BookingProduct bookingProduct = _context.BookingProducts.Single(bp => bp.ProductId == productId && bp.BookingId == bookingId);
            Booking booking = _context.Bookings.Find(bookingId);
            Product product = _context.Products.Find(productId);
            if (booking == null || product == null)
                return NotFound();

            if (!IsUserBooking(booking.Id))
                return new StatusCodeResult(403);

            if (booking.Status != "Ожидание")
                return new StatusCodeResult(403);

            if (_context.BookingProducts.Count(bp => bp.BookingId == bookingId) > 1)
            {
                _context.BookingProducts.Remove(_context.BookingProducts.Single(bp => bp.ProductId == productId && bp.BookingId == bookingId));
                _context.SaveChanges();

                CostController costController = new CostController(_context);
                costController.SetCostBooking(booking);
            }

             //return RedirectToAction("Details", new { id = bookingId });
             return Redirect(Request.Headers["Referer"].ToString());
        }

        [Authorize(Roles ="admin,manager")]
        public async Task<IActionResult> UnpinEmployee(int? bookingId, int? employeeId)
        {
            if (bookingId == null || employeeId == null)
                return NotFound();

            Booking booking = _context.Bookings.Include(b => b.Employees).Where(b => b.Id == bookingId).First();
            if (booking == null)
                return NotFound();

            if (booking.Status != "Выполняется")
                return new StatusCodeResult(403);

            if (booking.Employees.Count > 1)
            {
                booking.Employees.Remove(_context.Employees.Find(employeeId));
                _context.SaveChanges();
            }

            return Redirect(Request.Headers["Referer"].ToString()); // мб RedirectToAction
            //return View("Details", _context.Employees.Include(e => e.Bookings).Where(e => e.Id == employeeId).First());
        }

        [Authorize(Roles ="admin,manager")]
        public IActionResult LinkEmployeeWithBooking(int? id)
        {
            if (id == null)
                return NotFound();

            Booking booking = _context.Bookings.Include(b => b.Employees).Single(b => b.Id == id);
            if (booking == null)
                return NotFound();

            if(booking.Status != "Выполняется")
                return new StatusCodeResult(403);

            ViewBag.bookingId = id;
            return View(_context.Employees.Where(e=> !booking.Employees.Contains(e)));
        }

        [HttpPost]
        public async Task<IActionResult> LinkEmployeeWithBooking(int? bookingId, int[]? selectedEmployees) 
        { 
            if(bookingId == null || selectedEmployees == null)
                return NotFound();

            Booking booking = _context.Bookings.Find(bookingId);
            if (booking == null)
                return NotFound();

            foreach (var employeeId in selectedEmployees)
            {
                Employee employee = _context.Employees.Find(employeeId);

                if (employee == null)
                    return NotFound();

                booking.Employees.Add(employee);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = bookingId });
        }

        private bool IsUserBooking(int bookingId)
        {
            //var user =  _userManager.GetUserAsync(HttpContext.User);
            var user = _userManager.GetUserAsync(HttpContext.User);
            //var user = await _userManager.GetUserAsync(HttpContext.User);
            if (_userManager.IsInRoleAsync(user.Result, "customer").Result)
            {
                var product = _context.BookingProducts.Where(bp => bp.BookingId == bookingId).Select(bp => bp.Product).First();
                var customerProduct = _context.Products.Include(p => p.Customer).Single(p => p.Id == product.Id);

                if (customerProduct.Customer.Email != user.Result.Email)
                    return false;
            }
            return true;
        }

        private Customer GetBookingCustomer(int bookingId)
        {
            var product = (from bp in _context.BookingProducts.Include(bp => bp.Product).ThenInclude(p => p.Customer) where bp.BookingId == bookingId select bp.Product).First();

            if (product != null)
                return product.Customer;
            else
                return null;
        }

        public IActionResult GetReportAboutCompletedBookings()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetReportAboutCompletedBookings(DateTime? startDate, DateTime? endDate, string radioForReport)
        {
            if (startDate == null | endDate == null)
                return NotFound();

            var user = _userManager.GetUserAsync(HttpContext.User);
            string email = user.Result.Email;
            string userName = _userManager.IsInRoleAsync(user.Result, "manager").Result ? "менеджер" : "администратор";

            List<Booking> completedBookings = _context.Bookings.Include(b=>b.BookingProducts).ThenInclude(bp=>bp.Product).ThenInclude(p=>p.Customer).Where(b => b.Status == "Выполнен" & startDate <= b.End & b.End <= endDate).ToList();

            if (completedBookings.Count > 0)
            {
                if (radioForReport == "email")
                {
                    string pathToReport = GetReportCompletedBookings(completedBookings, startDate.Value.ToString("d"), endDate.Value.ToString("d"), false).ToString();
                    EmailSender emailSender = new EmailSender();
                    string subject = "Отчёт о выполненных заказах";
                    string message = $"Уважаемый, {userName}! Отчёт о выполненных заказах с {startDate.Value.ToString("d")} по {endDate.Value.ToString("d")} готов!<br>С уважением, \"Издательство\".";
                    emailSender.SendEmailWithDocument(pathToReport, email, subject, message);
                }
                else if (radioForReport == "download")
                {
                    FileResult report = (FileResult)GetReportCompletedBookings(completedBookings, startDate.Value.ToString("d"), endDate.Value.ToString("d"), true);
                    //return NotFound(report.);
                    return report;
                }
            }
            else
            {
                return RedirectToAction("Index", "Error", new { errorMessage = $"С {startDate.Value.ToString("d")} по {endDate.Value.ToString("d")} не был выполнен ни один заказ. Выберите другой временной интервал"});
            }

            //return RedirectToAction("GetReportAboutCompletedBookings", "Bookings");
            return RedirectToAction("GetReportAboutCompletedBookings", "Bookings");
        }

        private object GetReportCompletedBookings(List<Booking> bookings, string startDate, string endDate, bool download)
        {
            string pathToWorkPiece = "/Reports/completed_bookings_draft.xlsx";
            string result = $"/Reports/completed_bookings.xlsx";

            FileInfo workPiece = new FileInfo(_appEnvironment.WebRootPath + pathToWorkPiece);
            FileInfo fileInfoResult = new FileInfo(_appEnvironment.WebRootPath + result);

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (ExcelPackage excelPackage = new ExcelPackage(workPiece))
            {
                //устанавливаем поля документа
                excelPackage.Workbook.Properties.Author = "Издательство";
                excelPackage.Workbook.Properties.Title = "Список выполненных заказов";
                excelPackage.Workbook.Properties.Subject = "Выполненные заказы";
                excelPackage.Workbook.Properties.Created = DateTime.Now;
                //плучаем лист по имени.
                ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets["bookings"];
                //получаем списко пользователей и в цикле заполняем лист данными
                worksheet.Cells[2, 2].Value = $"с {startDate}";
                worksheet.Cells[2, 4].Value = $"по {endDate}";
                int startLine = 4;
                foreach (Booking booking in bookings)
                {
                    worksheet.Cells[startLine, 1].Value = startLine - 3;
                    worksheet.Cells[startLine, 2].Value = booking.Id;
                    worksheet.Cells[startLine, 3].Value = booking.Start.ToString("d");
                    worksheet.Cells[startLine, 4].Value = booking.End.Value.ToString("d");
                    worksheet.Cells[startLine, 5].Value = $"{booking.Cost} ₽";
                    worksheet.Cells[startLine, 6].Value = booking.BookingProducts.First().Product.Customer.Name;
                    startLine++;
                }
                //созраняем в новое место

                excelPackage.SaveAs(fileInfoResult);
                //excelPackage.Dispose();
            }

            string file_type = "application/vnd.openxmlformatsofficedocument.spreadsheetml.sheet";
            // Имя файла - необязательно
            string file_name = $"completed_bookings.xlsx";

            if (!download)
                return _appEnvironment.WebRootPath + result;
            else
                return File(result, file_type, file_name);
        }

        public IActionResult GetReportAboutCostCompletedBookings()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetReportAboutCostCompletedBookings(DateTime? startDate, DateTime? endDate, string radioForReport) 
        {
            if (startDate == null | endDate == null)
                return NotFound();

            var user = _userManager.GetUserAsync(HttpContext.User);
            string email = user.Result.Email;
            string userName = _userManager.IsInRoleAsync(user.Result, "manager").Result ? "менеджер" : "администратор";

            List<Booking> completedBookings = _context.Bookings.Where(b => b.Status == "Выполнен" & startDate <= b.End & b.End <= endDate).ToList();

            if (completedBookings.Count > 0)
            {
                if (radioForReport == "email")
                {
                    string pathToReport = GetReportCostCompletedBookings(completedBookings, startDate.Value, endDate.Value, false).ToString();
                    EmailSender emailSender = new EmailSender();
                    string subject = "Отчёт о доходах от заказов";
                    string message = $"Уважаемый(-ая), {userName}! Отчёт о доходах от заказов с {startDate.Value.ToString("d")} по {endDate.Value.ToString("d")} готов!<br>С уважением, \"Издательство\".";
                    emailSender.SendEmailWithDocument(pathToReport, email, subject, message);
                }
                else if (radioForReport == "download")
                {
                    FileResult report = (FileResult)GetReportCostCompletedBookings(completedBookings, startDate.Value, endDate.Value, true);
                    //return NotFound(report.);
                    return report;
                }
            }
            else
            {
                return RedirectToAction("Index", "Error", new { errorMessage = $"С {startDate.Value.ToString("d")} по {endDate.Value.ToString("d")} не был выполнен ни один заказ. Выберите другой временной интервал" });
            }

            //return RedirectToAction("GetReportAboutCompletedBookings", "Bookings");
            return RedirectToAction("GetReportAboutCostCompletedBookings", "Bookings");
        }

        private object GetReportCostCompletedBookings(List<Booking> bookings, DateTime startDate, DateTime endDate, bool download)
        {
            string pathToWorkPiece = "/Reports/employee_cost_bookings.xlsx";
            string result = $"/Reports/employee_cost_bookings_result.xlsx";

            FileInfo workPiece = new FileInfo(_appEnvironment.WebRootPath + pathToWorkPiece);
            FileInfo fileInfoResult = new FileInfo(_appEnvironment.WebRootPath + result);

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (ExcelPackage excelPackage = new ExcelPackage(workPiece))
            {
                //устанавливаем поля документа
                excelPackage.Workbook.Properties.Author = "Издательство";
                excelPackage.Workbook.Properties.Title = "Отчёт о расходах от заказов";
                excelPackage.Workbook.Properties.Subject = "Расходы от заказов";
                excelPackage.Workbook.Properties.Created = DateTime.Now;
                //плучаем лист по имени.
                ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets["bookings"];
                //получаем списко пользователей и в цикле заполняем лист данными
                worksheet.Cells[2, 2].Value = $"с {startDate.ToString("d")}";
                worksheet.Cells[2, 4].Value = $"по {endDate.ToString("d")}";
                int startLine = 4;

                DateTime endCount = new DateTime();
                while (startDate.Date < endDate.Date)
                {
                    if ((endDate - startDate).TotalDays <= 6)
                    {
                        endCount = endDate;
                    }
                    else
                    {
                        endCount = startDate.AddDays(6);
                    }

                    //cost = bookings.Where(b => startDate <= b.End & b.End <= endCount).Sum(b => b.Cost);

                    worksheet.Cells[startLine, 1].Value = startLine - 3;
                    worksheet.Cells[startLine, 2].Value = startDate.ToString("d");
                    worksheet.Cells[startLine, 3].Value = endCount.ToString("d");
                    worksheet.Cells[startLine, 4].Value = $"{bookings.Where(b => startDate <= b.End & b.End <= endCount).Count()}";
                    worksheet.Cells[startLine, 5].Value = $"{bookings.Where(b => startDate <= b.End & b.End <= endCount).Sum(b => b.Cost)} ₽";

                    startDate = startDate.AddDays(7);
                    startLine++;
                }
                //созраняем в новое место

                excelPackage.SaveAs(fileInfoResult);
            }

            string file_type = "application/vnd.openxmlformatsofficedocument.spreadsheetml.sheet";
            // Имя файла - необязательно
            string file_name = "employee_cost_bookings_result.xlsx";

            if (!download)
                return _appEnvironment.WebRootPath + result;
            else
                return File(result, file_type, file_name);
        }
    }
}
