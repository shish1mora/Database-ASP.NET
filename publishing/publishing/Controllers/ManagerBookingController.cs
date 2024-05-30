using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using publishing.Infrastructure;
using publishing.Models;
using System.Linq;

namespace publishing.Controllers
{
    [Authorize (Roles="manager,admin")]
    public class ManagerBookingController : Controller
    {
        private readonly PublishingDBContext _context;

        public ManagerBookingController(PublishingDBContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View( _context.Bookings.Where(b => b.Status != "Выполнен"));
        }

        [HttpPost]
        public async Task<IActionResult> ChooseEmployees(int? bookingId, string? bookingStatus)
        {
            if (bookingId == null || bookingStatus == null)
                return NotFound();

            Booking booking = (from b in _context.Bookings where b.Id == bookingId select b).Single();
            if (booking == null)
                return NotFound();

            if (bookingStatus == "Выполняется")
            {
                if(booking.End.Value.Date > DateTime.Now.Date)
                    booking.End = DateTime.Now.Date;

                booking.Status = "Выполнен";
                _context.SaveChanges();

                Customer customer = GetCustomerBooking(bookingId.Value);
                string subject = $"Выполнение заказа №{bookingId.Value}";
                string message = $"Уважаемый(-ая) {customer.Name}! Ваш заказ успешно выполен!<br>С уважением, \"Издательство\".";
                EmailSender emailSender = new EmailSender();
                emailSender.SendEmail(customer.Email, subject, message);

                //return Redirect(Request.Headers["Referer"].ToString());
                return RedirectToAction("Index");
            }

            ViewBag.bookingId = bookingId;
            return View(await _context.Employees.ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> ChoosePrintHouse(int? bookingId, int[]? selectedEmployees)
        {
            if (bookingId == null || selectedEmployees == null || selectedEmployees.Length == 0)
                return NotFound();

            Booking booking = (from b in _context.Bookings where b.Id == bookingId select b).Single();
            Employee[] employees = (from emp in _context.Employees where selectedEmployees.Contains(emp.Id) select emp).ToArray();

            if (booking == null || selectedEmployees.Length != employees.Length)
                return NotFound();


            ViewBag.bookingId = bookingId;
            ViewBag.selectedEmployees = selectedEmployees;

            return View(await _context.PrintingHouses.ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> ChooseDateOfComplete(int? bookingId, int? printHouseId, int[]? employees)
        {
            if (bookingId == null || printHouseId == null || employees == null || employees.Length == 0)
                return NotFound();

            PrintingHouse printHouse = (from pr in _context.PrintingHouses where pr.Id == printHouseId select pr).Single();
            if (printHouse == null)
                return NotFound();

            ViewBag.bookingId = bookingId;
            ViewBag.printHouseId = printHouseId;

            ViewBag.employees = employees;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Confirmation(int? bookingId, int? printHouseId, DateTime finishDate, int[]? employees)
        {
            if (bookingId == null || printHouseId == null || employees == null || employees.Length == 0)
                return NotFound();

            PrintingHouse printHouse = (from pr in _context.PrintingHouses where pr.Id == printHouseId select pr).Single();
            Employee[] selectedEmployees = (from emp in _context.Employees where employees.Contains(emp.Id) select emp).ToArray();

            if (printHouse == null || selectedEmployees.Length != employees.Length)
                return NotFound();

            ViewBag.printHouseId = printHouse.Id;
            ViewBag.printHouseName = printHouse.Name;

            ViewBag.employees = selectedEmployees;

            ViewBag.bookingId = bookingId;
            ViewBag.finishDate = finishDate.ToShortDateString();
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> ConfirmRegistration(int? bookingId, int? printHouseId, int[]? employees, string finishDate)
        {
            if (bookingId == null || printHouseId == null || employees == null || employees.Length == 0)
                return NotFound();

            DateTime dateFinish = DateTime.ParseExact(finishDate, "d", null);
            Booking booking = (from b in _context.Bookings where b.Id == bookingId select b).Single();

            booking.Status = "Выполняется";
            booking.End = dateFinish;
            booking.PrintingHouseId = (int)printHouseId;

            //var bookingsEmployees = _context.Set<BookingEmployee>();

            for (int i = 0; i < employees.Length; i++)
            {
                Employee employee = (from e in _context.Employees where e.Id == employees[i] select e).Single();
                booking.Employees.Add(employee);
                employee.Bookings.Add(booking);

                //bookingsEmployees.Add(new BookingEmployee { BookingId = (int)bookingId, EmployeeId = employees[i] });
            }

            _context.SaveChanges();

            Customer customer = GetCustomerBooking(bookingId.Value);
            string subject = $"Успешная регистрация заказа №{bookingId.Value}";
            string message = $"Уважаемый(-ая) {customer.Name}! Ваш заказ успешно зарегистрирован! Ориентировочное время выполнения: {finishDate}.<br>С уважением, \"Издательство\".";
            EmailSender emailSender = new EmailSender();
            emailSender.SendEmail(customer.Email, subject, message);

            return Redirect("~/ManagerBooking/Index");

        }

        private Customer GetCustomerBooking(int bookingId) 
        {
            var product = (from bp in _context.BookingProducts.Include(bp => bp.Product).ThenInclude(p => p.Customer) where bp.BookingId == bookingId select bp.Product).First();

            if (product != null)
                return product.Customer;
            else
                return null;
        }
    }
}