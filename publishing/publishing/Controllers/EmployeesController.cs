using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OfficeOpenXml;
using publishing.Areas.Identity.Data;
using publishing.Infrastructure;
using publishing.Models;

namespace publishing.Controllers
{
    [Authorize(Roles ="manager,admin")]
    public class EmployeesController : Controller
    {
        private readonly PublishingDBContext _context;
        private readonly IWebHostEnvironment _appEnvironment;
        private readonly UserManager<publishingUser> _userManager;

        public EmployeesController(PublishingDBContext context, IWebHostEnvironment appEnvironment, UserManager<publishingUser> userManager)
        {
            _context = context;
            _appEnvironment = appEnvironment;
            _userManager = userManager;
        }

        // GET: Employees
        public IActionResult Index(string? surname, string? name, string? middlename, string? type, DateTime? startBirthday, DateTime? endBirthday, string? phone, string? email)
        {
            List<Employee> employees = _context.Employees.ToList();

            if (surname != null)
                employees = employees.Where(e => e.Surname == surname).ToList();

            if (name != null)
                employees = employees.Where(e => e.Name == name).ToList();

            if (middlename != null)
                employees = employees.Where(e => e.Middlename != null && e.Middlename == middlename).ToList();

            if (type != null)
                employees = employees.Where(e => e.Type == type).ToList();


            if (startBirthday != null & endBirthday == null)
                employees = employees.Where(e => e.Birthday.Date >= startBirthday.Value.Date).ToList();
            else if (startBirthday == null & endBirthday != null)
                employees = employees.Where(e => e.Birthday.Date <= endBirthday.Value.Date).ToList();
            else if (startBirthday != null & endBirthday != null)
                employees = employees.Where(e => e.Birthday.Date >= startBirthday.Value.Date && e.Birthday.Date <= endBirthday.Value.Date).ToList();

            if (phone != null)
                employees = employees.Where(e => e.Phone == phone).ToList();

            if (email != null)
                employees = employees.Where(e => e.Email == email).ToList();


            return View(employees);
        }

        // GET: Employees/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Employees == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees.Include(e=> e.Bookings).Where(e=> e.Id == id).FirstOrDefaultAsync();

            if (employee == null) 
            {
                return NotFound();
            }

            if (!employee.Visual.IsNullOrEmpty())
            {
                byte[] photodata = System.IO.File.ReadAllBytes(_appEnvironment.WebRootPath + employee.Visual);
                ViewBag.Photodata = photodata;
            }
            else
            {
                ViewBag.Photodata = null;
            }

            return View(employee);
        }

        // GET: Employees/Create
        [Authorize(Roles ="admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Employees/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Surname,Middlename,Type,Phone,Email,Birthday,Description")] Employee employee, IFormFile photo)
        {
            if (ModelState.IsValid)
            {
                if (!IsNewEmployee(employee,"create"))
                    return RedirectToAction("Create");

                if (photo != null)
                {
                    string extension = Path.GetExtension(photo.FileName);
                    string[] extensions = { ".jpg", ".jpeg", ".png", ".bmp"};
                    if (!extensions.Contains(extension))
                        return RedirectToAction("Index", "Error", new { errorMessage = "Файл для изображения должен иметь одно из следующих расширений: .jpg, .jpeg, .png, .tif, .bmp"});

                    Random random = new Random();
                    string path = "/Files/" + $"{employee.Surname}_{employee.Name}_{random.Next(1, int.MaxValue - 1)}{extension}";
                    using (var fileStream = new FileStream(_appEnvironment.WebRootPath + path, FileMode.Create))
                    {
                        await photo.CopyToAsync(fileStream);
                    }
                    employee.Visual = path;
                }
                _context.Add(employee);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            //else 
            //{
            //    string erorrs = "";
            //    foreach (var item in ModelState)
            //    {
            //        foreach (var error in item.Value.Errors)
            //        {
            //            erorrs += error.ErrorMessage + "\t";
            //        }
            //    }
            //    return NotFound(erorrs);
            //}
            return View(employee);
        }

        // GET: Employees/Edit/5
        [Authorize(Roles ="admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Employees == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            if (!employee.Visual.IsNullOrEmpty())
            {
                byte[] photodata = System.IO.File.ReadAllBytes(_appEnvironment.WebRootPath + employee.Visual);
                ViewBag.Photodata = photodata;
            }
            else
            {
                ViewBag.Photodata = null;
            }

            return View(employee);
        }

        // POST: Employees/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Surname,Middlename,Type,Phone,Email,Birthday,Visual,Description")] Employee employee, IFormFile? photo)
        {
            if (id != employee.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (!IsNewEmployee(employee,"edit"))
                    return RedirectToAction("Edit",new {id});

                if (photo != null)
                {
                    string extension = Path.GetExtension(photo.FileName);
                    string[] extensions = { ".jpg", ".jpeg", ".png", ".bmp" };
                    if (!extensions.Contains(extension))
                        return RedirectToAction("Index", "Error", new { errorMessage = "Файл для изображения должен иметь одно из следующих расширений: .jpg, .jpeg, .png, .tif, .bmp" });

                    Random random = new Random();
                    string path = "/Files/" + $"{employee.Surname}_{employee.Name}_{random.Next(1, int.MaxValue - 1)}{extension}";
                    using (var fileStream = new FileStream(_appEnvironment.WebRootPath + path, FileMode.Create))
                    {
                        await photo.CopyToAsync(fileStream);
                    }

                    if (!employee.Visual.IsNullOrEmpty())
                    {
                        System.IO.File.Delete(_appEnvironment.WebRootPath + employee.Visual);
                    }

                    employee.Visual = path;
                }
                try
                {
                    _context.Update(employee);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeExists(employee.Id))
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
            return View(employee);
        }

        // GET: Employees/Delete/5
        [Authorize(Roles ="admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Employees == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .FirstOrDefaultAsync(m => m.Id == id);
            if (employee == null)
            {
                return NotFound();
            }

            if (!employee.Visual.IsNullOrEmpty())
            {
                byte[] photodata = System.IO.File.ReadAllBytes(_appEnvironment.WebRootPath + employee.Visual);
                ViewBag.Photodata = photodata;
            }
            else
            {
                ViewBag.Photodata = null;
            }

            return View(employee);
        }

        // POST: Employees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Employees == null)
            {
                return Problem("Entity set 'PublishingDBContext.Employees'  is null.");
            }

            var employee = _context.Employees.Include(e => e.Bookings).FirstOrDefault(e => e.Id == id);
            if (employee != null)
            {
                if (employee.Bookings.Count != 0)
                    return RedirectToAction("Delete", new { id });
                
                if (!employee.Visual.IsNullOrEmpty())
                {
                    System.IO.File.Delete(_appEnvironment.WebRootPath + employee.Visual);
                }

                _context.Employees.Remove(employee);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EmployeeExists(int id)
        {
          return (_context.Employees?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        public IActionResult LinkEmployeeWithBooking(int? id)
        {
            if(id == null)
                return NotFound();

            Employee employee = _context.Employees.Include(e => e.Bookings).Single(e => e.Id == id);
            if(employee == null)
                return NotFound();

            ViewBag.employeeId = id;
            return View(_context.Bookings.Where(b => b.Status == "Выполняется" && !employee.Bookings.Contains(b)));
        }

        [HttpPost]
        public async Task<IActionResult> LinkEmployeeWithBooking(int? employeeId, int[]? selectedBookings)
        {
            if (employeeId == null || selectedBookings == null)
                return NotFound();

            Employee employee = _context.Employees.Find(employeeId);
            if (employee == null)
                return NotFound();

            foreach (var bookingId in selectedBookings)
            {
                Booking booking = _context.Bookings.Find(bookingId);
                
                if (booking == null)
                    return NotFound();

                booking.Employees.Add(employee);
                //employee.Bookings.Add(booking);
            }
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = employeeId });
        }

        private bool IsNewEmployee(Employee employee,string typeAction) 
        {
            Employee existEmployee = null;

            if(typeAction == "create")
                existEmployee = _context.Employees.FirstOrDefault(e => e.Email == employee.Email | e.Phone == employee.Phone);
            else if(typeAction == "edit")
                existEmployee = _context.Employees.FirstOrDefault(e => (e.Email == employee.Email | e.Phone == employee.Phone) & e.Id != employee.Id);

            if (existEmployee == null)
                return true;

            return false;
        }

        public IActionResult ReportAboutWork(int? id) 
        { 
            if(id == null)
                return NotFound();

            Employee employee = _context.Employees.Include(e=>e.Bookings).FirstOrDefault(e => e.Id == id);
            if (employee == null) 
                return NotFound();

            return View(employee);
        }


        [HttpPost]
        public async Task<IActionResult> ReportAboutWork(DateTime? startDate, DateTime? endDate, string statusBooking, string radioForReport, int? employeeId)
        {
            if (startDate == null | endDate == null | employeeId == null)
                return NotFound();

            var user = _userManager.GetUserAsync(HttpContext.User);
            string email = user.Result.Email;
            string userName = _userManager.IsInRoleAsync(user.Result, "manager").Result ? "менеджер" : "администратор";

            Employee employee = _context.Employees.Include(e => e.Bookings).ThenInclude(b=>b.BookingProducts).ThenInclude(bp=>bp.Product).ThenInclude(p=>p.Customer).FirstOrDefault(e => e.Id == employeeId);
            //Employee employee = _context.Employees.Include(e => e.Bookings).FirstOrDefault(e => e.Id == employeeId);
            if (employee == null)
                return NotFound();

            string employeeFIO = $"{employee.Surname} {employee.Name} {employee.Middlename}";
            List<Booking> bookings = new List<Booking>();
            if (statusBooking == "all")
                bookings = employee.Bookings.Where(b => startDate <= b.Start & b.Start <= endDate).ToList();
            else if (statusBooking == "processing")
                bookings = employee.Bookings.Where(b => b.Status == "Выполняется" & startDate <= b.Start & b.Start <= endDate).ToList();
            else if(statusBooking == "done")
                bookings = employee.Bookings.Where(b => b.Status == "Выполнен" & startDate <= b.Start & b.Start <= endDate).ToList();

            if (bookings.Count > 0)
            {
                if (radioForReport == "email")
                {
                    string pathToReport = GetReportAboutWork(bookings, startDate.Value, endDate.Value, employeeFIO, false).ToString();
                    EmailSender emailSender = new EmailSender();
                    string subject = $"Отчёт о работе сотрудника {employee.Surname} {employee.Name} {employee.Middlename}";
                    string message = $"Уважаемый(-ая), {userName}! Отчёт о работе сотрудника {employeeFIO} с {startDate.Value.ToString("d")} по {endDate.Value.ToString("d")} готов!<br>С уважением, \"Издательство\".";
                    emailSender.SendEmailWithDocument(pathToReport, email, subject, message);
                }
                else if (radioForReport == "download")
                {
                    FileResult report = (FileResult)GetReportAboutWork(bookings, startDate.Value, endDate.Value, employeeFIO, true);
                    return report;
                }
            }
            else
            {
                return RedirectToAction("Index", "Error", new { errorMessage = $"С {startDate.Value.ToString("d")} по {endDate.Value.ToString("d")} у {employeeFIO} нет ни одного заказа. Пожалуйста, выберите другой временной интервал и повторите попытку"});
            }

            //return RedirectToAction("GetReportAboutCompletedBookings", "Bookings");
            return RedirectToAction("ReportAboutWork", new {id = employee.Id});
        }

        private object GetReportAboutWork(List<Booking> bookings, DateTime startDate, DateTime endDate, string employee, bool download)
        {
            string pathToWorkPiece = "/Reports/employee_bookings_draft.xlsx";
            string result = $"/Reports/{employee}_bookings_result.xlsx";

            FileInfo workPiece = new FileInfo(_appEnvironment.WebRootPath + pathToWorkPiece);
            FileInfo fileInfoResult = new FileInfo(_appEnvironment.WebRootPath + result);

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (ExcelPackage excelPackage = new ExcelPackage(workPiece))
            {
                //устанавливаем поля документа
                excelPackage.Workbook.Properties.Author = "Издательство";
                excelPackage.Workbook.Properties.Title = $"Отчёт о работе {employee}";
                excelPackage.Workbook.Properties.Subject = "Отчёт о работе";
                excelPackage.Workbook.Properties.Created = DateTime.Now;
                //плучаем лист по имени.
                ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets["bookings"];
                //получаем списко пользователей и в цикле заполняем лист данными
                worksheet.Cells[2, 4].Value = employee;
                worksheet.Cells[3, 3].Value = $"с {startDate.ToString("d")}";
                worksheet.Cells[3, 5].Value = $"по {endDate.ToString("d")}";
                int startLine = 5;

                foreach (Booking booking in bookings)
                {
                    worksheet.Cells[startLine, 1].Value = $"{startLine - 3}";
                    worksheet.Cells[startLine, 2].Value = $"{booking.Id}";
                    worksheet.Cells[startLine, 3].Value = booking.Start.ToString("d");
                    worksheet.Cells[startLine, 4].Value = booking.End.Value.ToString("d");
                    worksheet.Cells[startLine, 5].Value = booking.Status;
                    worksheet.Cells[startLine, 6].Value = $"{booking.Cost} ₽";
                    worksheet.Cells[startLine, 7].Value = booking.BookingProducts.First().Product.Customer.Name;
                    startLine++;
                }
                //созраняем в новое место
                excelPackage.SaveAs(fileInfoResult);
            }

            string file_type = "application/vnd.openxmlformatsofficedocument.spreadsheetml.sheet";
            // Имя файла - необязательно
            string file_name = $"{employee}_bookings_result.xlsx";

            if (!download)
                return _appEnvironment.WebRootPath + result;
            else
                return File(result, file_type, file_name);
        }
    }
}
