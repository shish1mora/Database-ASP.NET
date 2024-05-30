using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using publishing.Areas.Identity.Data;
using publishing.Models;
using publishing.Models.ViewModels;
using publishing.Infrastructure;
using OfficeOpenXml;
using System.IO.Pipelines;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace publishing.Controllers
{
    [Authorize]
    public class CustomersController : Controller
    {
        private readonly PublishingDBContext _context;
        private readonly UserManager<publishingUser> _userManager;
        private readonly IWebHostEnvironment _appEnvironment;

        public CustomersController(PublishingDBContext context, UserManager<publishingUser> userManager, IWebHostEnvironment appEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _appEnvironment = appEnvironment;
        }

        // GET: Customers
        [Authorize(Roles = "manager,admin")]
        public IActionResult Index(string? name, string? phoneNumber, string? email)
        {
            List<Customer> customers = _context.Customers.ToList();

            if (name != null)
                customers = customers.Where(c => c.Name == name).ToList();

            if(phoneNumber != null)
                customers = customers.Where(c => c.Phone == phoneNumber).ToList();

            if (email != null)
                customers = customers.Where(c => c.Email == email).ToList();

            return View(customers);
        }

        // GET: Customers/Details/5
        [Authorize(Roles = "manager,admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Customers == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers.Include(c => c.Products)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (customer == null)
            {
                return NotFound();
            }

            CustomerDetailsViewModel model = new CustomerDetailsViewModel();
            model.Customer = customer;

            //var products = _context.Products.Include(p => p.TypeProduct).Where(p => customer.Products.Contains(p)).ToList();
            //if(products == null)
            //    return NotFound();

            model.Bookings = GetBookings(customer);
            model.Products = GetProducts(customer,true);
            //model.Bookings.AddRange(from bp in _context.BookingProducts.Include(bp => bp.Booking).Include(bp => bp.Product) where products.Contains(bp.Product) && !model.Bookings.Contains(bp.Booking) && bp.Booking != null select bp.Booking);
            //model.Products.AddRange((from bp in _context.BookingProducts.Include(bp => bp.Product) where products.Contains(bp.Product) && bp.Booking != null select bp.Product).Distinct());

            //foreach (var product in products)
            //{
            //    model.Bookings.AddRange(from bp in _context.BookingProducts.Include(bp => bp.Booking) where bp.ProductId == product.Id && !model.Bookings.Contains(bp.Booking) && bp.Booking != null select bp.Booking);
            //    model.Products.AddRange((from bp in _context.BookingProducts.Include(bp => bp.Product) where bp.ProductId == product.Id && bp.Booking != null select bp.Product).Distinct());
            //}

            return View(model);
        }

        //// GET: Customers/Create
        //[Authorize(Roles = "admin")]
        //public IActionResult Create()
        //{
        //    return View();
        //}

        //// POST: Customers/Create
        //// To protect from overposting attacks, enable the specific properties you want to bind to.
        //// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("Id,Name,Phone,Email")] Customer customer)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _context.Add(customer);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }
        //    return View(customer);
        //}

        //// GET: Customers/Edit/5
        //[Authorize(Roles = "admin")]
        //public async Task<IActionResult> Edit(int? id)
        //{
        //    if (id == null || _context.Customers == null)
        //    {
        //        return NotFound();
        //    }

        //    var customer = await _context.Customers.FindAsync(id);
        //    if (customer == null)
        //    {
        //        return NotFound();
        //    }
        //    return View(customer);
        //}

        //// POST: Customers/Edit/5
        //// To protect from overposting attacks, enable the specific properties you want to bind to.
        //// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Phone,Email")] Customer customer)
        //{
        //    if (id != customer.Id)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            _context.Update(customer);
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!CustomerExists(customer.Id))
        //            {
        //                return NotFound();
        //            }
        //            else
        //            {
        //                throw;
        //            }
        //        }
        //        return RedirectToAction(nameof(Index));
        //    }
        //    return View(customer);
        //}

        //// GET: Customers/Delete/5
        //[Authorize(Roles = "admin")]
        //public async Task<IActionResult> Delete(int? id)
        //{
        //    if (id == null || _context.Customers == null)
        //    {
        //        return NotFound();
        //    }

        //    var customer = await _context.Customers
        //        .FirstOrDefaultAsync(m => m.Id == id);
        //    if (customer == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(customer);
        //}

        //// POST: Customers/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    if (_context.Customers == null)
        //    {
        //        return Problem("Entity set 'PublishingDBContext.Customers'  is null.");
        //    }
        //    var customer = await _context.Customers.FindAsync(id);
        //    if (customer != null)
        //    {
        //        _context.Customers.Remove(customer);
        //    }

        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}

        private bool CustomerExists(int id)
        {
            return (_context.Customers?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        private List<Booking> GetBookings(Customer customer)
        {
            List<Booking> bookings = new List<Booking>();
            var customerProducts = _context.Products.Include(p => p.TypeProduct).Where(p => customer.Products.Contains(p)).ToList();
            if (customerProducts != null)
            {
                bookings.AddRange((from bp in _context.BookingProducts.Include(bp => bp.Booking).Include(bp => bp.Product) where customerProducts.Contains(bp.Product) && bp.Booking != null select bp.Booking).Distinct());
            }

            return bookings;
        }

        private List<Product> GetProducts(Customer customer,bool InBooking)
        {
            List<Product> products = new List<Product>();
            var customerProducts = _context.Products.Include(p => p.TypeProduct).Include(p=>p.BookingProducts).ThenInclude(bp=>bp.Booking).Where(p => customer.Products.Contains(p)).ToList();
            if (customerProducts != null)
            {
                if (InBooking)
                    products.AddRange((from bp in _context.BookingProducts.Include(bp => bp.Product) where customerProducts.Contains(bp.Product) && bp.Booking != null select bp.Product).Distinct());

                else
                    products = customerProducts;
            }
            return products;
        }

        [Authorize(Roles = "customer")]
        public async Task<IActionResult> CustomerBookings(string emailCustomer, string? status, string? date, DateTime? startDate, DateTime? endDate, int? startNumber, int? endNumber, double? startCost, double? endCost)
        {
            var customer = _context.Customers.Include(c => c.Products).FirstOrDefault(c => c.Email == emailCustomer);
            if (customer == null)
                return NotFound();

            if (!IsCustomerEmail(emailCustomer))
                return new StatusCodeResult(403);


            List<Booking> bookings = GetBookings(customer);
            FilterController filterController = new FilterController(_context);


            if (status != null && status != "Все")
            {
                bookings = filterController.GetBookingWithCertainStatus(status, bookings);
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

        [Authorize(Roles = "customer")]
        public async Task<IActionResult> CustomerProducts(string emailCustomer, string? name, string? type, double? startCost, double? endCost)
        {
            var customer = _context.Customers.Include(c => c.Products).FirstOrDefault(c => c.Email == emailCustomer);
            if (customer == null)
                return NotFound();

            if (!IsCustomerEmail(emailCustomer))
                return new StatusCodeResult(403);


            List<Product> products = GetProducts(customer, false);

            FilterController filterController = new FilterController(_context);

            if (name != null)
            {
                products = filterController.GetProductsWithCertainName(name, products);
            }

            if (type != null)
            {
                products = filterController.GetProductsWithCertainType(type, products);
            }

            if (startCost != null || endCost != null)
            {
                products = filterController.GetProductsWithCertainCost(startCost, endCost, products);
            }

            return View(products);
        }

        private bool IsCustomerEmail(string email)
        {
            //var user =  _userManager.GetUserAsync(HttpContext.User);

            var user = _userManager.GetUserAsync(HttpContext.User);
            //var user = await _userManager.GetUserAsync(HttpContext.User);
            if (_userManager.IsInRoleAsync(user.Result, "customer").Result)
            {
                if (email == user.Result.Email)
                    return true;
            }
            return false;
        }


        public IActionResult TransitionToSelectProducts(int bookingId)
        {
            HttpContext.Session.SetJson($"BookingBy_{_userManager.GetUserAsync(HttpContext.User).Result.Email}", bookingId);
            return Redirect("SelectProducts");
        }


        [Authorize(Roles = "customer")]
        public IActionResult SelectProducts(string typeProduct = "", int p = 1)
        {
            //if (bookingId == null)
            //    return NotFound();
            int bookingId = HttpContext.Session.GetJson<int>($"BookingBy_{_userManager.GetUserAsync(HttpContext.User).Result.Email}");
            if (bookingId == 0)
                return NotFound();

            //if (bookingId != -1)
            //{
            //    BookingsController bookingsController = new BookingsController();
            //    if (!bookingsController.IsUserBooking(bookingId.Value))
            //        return new StatusCodeResult(403);
            //}

            int pageSize = 3;
            ViewBag.pageNumber = p;
            ViewBag.pageRange = pageSize;
            ViewBag.typeProduct = typeProduct;
            //ViewBag.bookingId = bookingId;

            var user = _userManager.GetUserAsync(HttpContext.User);
            var customer = _context.Customers.Include(c => c.Products).FirstOrDefault(c => c.Email == user.Result.Email);
            if (customer == null)
                return NotFound();

            var customerProducts = _context.Products.Include(p => p.TypeProduct).Where(p => customer.Products.Contains(p));
            List<string> typesProducts = new List<string>();
            foreach (var product in customerProducts)
            {
                if (!typesProducts.Contains(product.TypeProduct.Type))
                    typesProducts.Add(product.TypeProduct.Type);
            }
            ViewBag.typeProducts = typesProducts;

            // Корзина
            List<CartItem> cart = HttpContext.Session.GetJson<List<CartItem>>($"{user.Result.Email}_Product");
            SmallCartViewModel smallCartModel = null;

            if (cart != null && cart.Count > 0)
            {
                //var customerCart = _context.Customers.Include(c => c.Products).FirstOrDefault(c => c.Products.Contains(cart.First().Product));
                //if (customerCart == null)
                //    return NotFound();

                //if (customerCart.Email != user.Result.Email) 
                //{
                //    //Очистка Json
                //    HttpContext.Session.Remove("Cart");
                //}
                //else
                //{
                smallCartModel = new()
                {
                    NumberOfItems = cart.Sum(x => x.Quantity),
                    TotalAmount = cart.Sum(x => x.Quantity * x.Product.Cost)

                };
            }
            //smallCartModel = null;            

            ViewBag.smallCartModel = smallCartModel;
            //Конец корзины

            if (typeProduct == "")
            {
                ViewBag.totalPages = (int)Math.Ceiling((decimal)customer.Products.Count / pageSize);
                ViewBag.productsPhoto = GetVisualProducts(customer.Products.ToList());
                return View(customer.Products.OrderBy(p => p.Name).Skip((p - 1) * pageSize).Take(pageSize).ToList());
            }

            //TypeProduct type = _context.TypeProducts.FirstOrDefault(tp => tp.Type == typeProduct);
            //if(type == null)
            //    return Redirect(Request.Headers["Referer"].ToString());

            var productsByType = _context.Products.Include(p => p.TypeProduct).Where(p => p.TypeProduct.Type == typeProduct && customer.Products.Contains(p)).ToList();
            if (productsByType.Count == 0)
                return Redirect(Request.Headers["Referer"].ToString());

                ViewBag.productsPhoto = GetVisualProducts(productsByType);

            ViewBag.totalPages = (int)Math.Ceiling((decimal)productsByType.Count / pageSize);

            return View(productsByType.OrderBy(p => p.Name).Skip((p - 1) * pageSize).Take(pageSize).ToList());
            
        }

        private List<VisualProduct> GetVisualProducts(List<Product> products) 
        {
            List<VisualProduct> productsPhoto = new List<VisualProduct>();

            if (products != null && products.Count > 0)
            {
                foreach (var product in products)
                {
                    productsPhoto.AddRange((from vp in _context.VisualProducts where vp.ProductId == product.Id select vp));
                }
            }

            return productsPhoto;
        }

        //[Authorize(Roles ="customer")]
        //public IActionResult GetReportAboutCompletedBookings() 
        //{
        //    return View();
        //}

        [HttpPost]
        public async Task<IActionResult> GetReportAboutCompletedBookings(DateTime? startDate, DateTime? endDate,string radioForReport) 
        {
            if (startDate == null | endDate == null)
                return NotFound();

            string emailCustomer = _userManager.GetUserAsync(HttpContext.User).Result.Email;
            Customer customer = _context.Customers.Include(c => c.Products).FirstOrDefault(c => c.Email == emailCustomer);
            if (customer == null)
                return NotFound();

            List<Booking> customerBooking = GetBookings(customer);
            List<Booking> completeCustomerBooking = customerBooking.Where(b => b.Status == "Выполнен" & startDate <= b.End & b.End <= endDate).ToList();

            if (completeCustomerBooking.Count > 0)
            {
                if (radioForReport == "email")
                {
                    string pathToReport = GetReportCompletedBookings(completeCustomerBooking, startDate.Value.ToString("d"), endDate.Value.ToString("d"), customer.Name, false).ToString();
                    EmailSender emailSender = new EmailSender();
                    string subject = "Отчёт о выполненных заказах";
                    string message = $"Уважаемый(-ая), {customer.Name}! Отчёт о выполненных заказах с {startDate.Value.ToString("d")} по {endDate.Value.ToString("d")} готов!<br>С уважением, \"Издательство\".";
                    emailSender.SendEmailWithDocument(pathToReport, emailCustomer, subject, message);
                }
                else if (radioForReport == "download")
                {
                    FileResult report = (FileResult)GetReportCompletedBookings(completeCustomerBooking, startDate.Value.ToString("d"), endDate.Value.ToString("d"), customer.Name, true);
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

        private object GetReportCompletedBookings(List<Booking> bookings, string startDate, string endDate, string customerName, bool download)
        {
            string pathToWorkPiece = "/Reports/customer_bookings.xlsx";
            string result = $"/Reports/{customerName}_bookings.xlsx";

            FileInfo workPiece = new FileInfo(_appEnvironment.WebRootPath + pathToWorkPiece);
            FileInfo fileInfoResult = new FileInfo(_appEnvironment.WebRootPath + result);

            //if (fileInfoResult.Exists) 
            //{
            //    fileInfoResult.Delete();
            //    fileInfoResult = new FileInfo(_appEnvironment.WebRootPath + result);
            //}

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
                    startLine++;
                }
                //созраняем в новое место

                excelPackage.SaveAs(fileInfoResult);
            }

            string file_type = "application/vnd.openxmlformatsofficedocument.spreadsheetml.sheet";
            // Имя файла - необязательно
            string file_name = $"{customerName}_bookings.xlsx";

            if (!download)
                return _appEnvironment.WebRootPath + result;
            else
                return File(result, file_type, file_name);
        }

        [HttpPost]
        public async Task<IActionResult> GetReportAboutCostCompletedBookings(DateTime? startDate, DateTime? endDate, string radioForReport)
        {
            if (startDate == null | endDate == null)
                return NotFound();

            string emailCustomer = _userManager.GetUserAsync(HttpContext.User).Result.Email;
            Customer customer = _context.Customers.Include(c => c.Products).FirstOrDefault(c => c.Email == emailCustomer);
            if (customer == null)
                return NotFound();

            List<Booking> customerBooking = GetBookings(customer);
            List<Booking> completeCustomerBooking = customerBooking.Where(b => b.Status == "Выполнен" & startDate <= b.End & b.End <= endDate).ToList();

            if (completeCustomerBooking.Count > 0)
            {
                if (radioForReport == "email")
                {
                    string pathToReport = GetReportCostCompletedBookings(completeCustomerBooking, startDate.Value, endDate.Value, customer.Name, false).ToString();
                    EmailSender emailSender = new EmailSender();
                    string subject = "Отчёт о расходах от заказов";
                    string message = $"Уважаемый(-ая), {customer.Name}! Отчёт о расходах от заказов с {startDate.Value.ToString("d")} по {endDate.Value.ToString("d")} готов!<br>С уважением, \"Издательство\".";
                    emailSender.SendEmailWithDocument(pathToReport, emailCustomer, subject, message);
                }
                else if (radioForReport == "download")
                {
                    FileResult report = (FileResult)GetReportCostCompletedBookings(completeCustomerBooking, startDate.Value, endDate.Value, customer.Name, true);
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

        [HttpPost]
        private object GetReportCostCompletedBookings(List<Booking> bookings, DateTime startDate, DateTime endDate, string customerName, bool download)
        {
            string pathToWorkPiece = "/Reports/customer_cost_bookings.xlsx";
            string result = $"/Reports/{customerName}_cost_bookings.xlsx";

            FileInfo workPiece = new FileInfo(_appEnvironment.WebRootPath + pathToWorkPiece);
            FileInfo fileInfoResult = new FileInfo(_appEnvironment.WebRootPath + result);

            //if (fileInfoResult.Exists) 
            //{
            //    fileInfoResult.Delete();
            //    fileInfoResult = new FileInfo(_appEnvironment.WebRootPath + result);
            //}

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

                //double cost = 0;
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
                    worksheet.Cells[startLine, 4].Value = worksheet.Cells[startLine, 4].Value = $"{bookings.Where(b => startDate <= b.End & b.End <= endCount).Sum(b => b.Cost)} ₽";

                    startDate = startDate.AddDays(7);
                    startLine++;
                }
                //созраняем в новое место

                excelPackage.SaveAs(fileInfoResult);
            }

            string file_type = "application/vnd.openxmlformatsofficedocument.spreadsheetml.sheet";
            // Имя файла - необязательно
            string file_name = $"{customerName}_cost_bookings.xlsx";

            if (!download)
                return _appEnvironment.WebRootPath + result;
            else
                return File(result, file_type, file_name);
        }

    }
}
