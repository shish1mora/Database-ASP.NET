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
using publishing.Infrastructure;
using publishing.Migrations;
using publishing.Models;
using publishing.Models.ViewModels;
using Quartz.Core;

namespace publishing.Controllers
{
    [Authorize]
    public class ProductsController : Controller
    {
        private readonly PublishingDBContext _context;
        private readonly UserManager<publishingUser> _userManager;
        private readonly IWebHostEnvironment _appEnvironment;
        public ProductsController(PublishingDBContext context, UserManager<publishingUser> userManager, IWebHostEnvironment appEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _appEnvironment = appEnvironment;
        }

        // GET: Products
        [Authorize(Roles ="admin,manager")]
        public IActionResult Index(string? name, string? type, string? customer, double? startCost, double? endCost)
        {
            List<Product> products = _context.Products.Include(p => p.Customer).Include(p => p.TypeProduct).Include(p => p.BookingProducts).Where(p => p.BookingProducts.Count > 0).ToList();

            if (customer != null)
                products = products.Where(p => p.Customer.Name == customer).ToList();

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

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Customer)
                .Include(p => p.TypeProduct)
                .Include(p=> p.ProductMaterials)
                .Include(p=> p.BookingProducts)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (product == null)
            {
                return NotFound();
            }

            if (!IsCustomerProduct(product.Id))
                return new StatusCodeResult(403);

            ProductDetailsViewModel model = new ProductDetailsViewModel();
            model.Product = product;
            model.ProductMaterials = _context.ProductMaterials.Include(pm => pm.Material).Where(pm => pm.ProductId == id).ToList();
            model.BookingProducts = _context.BookingProducts.Include(bp => bp.Booking).Where(bp => bp.ProductId == id).ToList();

            List<byte[]> visualProducts = (from vp in _context.VisualProducts where vp.ProductId == product.Id select vp.Photo).ToList();
            ViewBag.visualProducts = visualProducts;

            //return View(product);
            return View(model);
        }

        // GET: Products/Create
        [Authorize(Roles="customer")]
        public IActionResult Create()
        {
            //ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "Name");
            ViewData["TypeProductId"] = new SelectList(_context.TypeProducts, "Id", "Type");
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,TypeProductId")] Product product, IFormFile[] photo)
        {
            if (ModelState.IsValid)
            {              
                product.TypeProduct = _context.TypeProducts.Find(product.TypeProductId);

                List<VisualProduct> visualProducts = new List<VisualProduct>();

                foreach (var item in photo)
                {
                    if (!CorrectExtensions(item.FileName))
                        return RedirectToAction("Index", "Error", new { errorMessage = "Файл для изображения должен иметь одно из следующих расширений: .jpg, .jpeg, .png, .tif, .bmp" });
                    else 
                    {                       
                        VisualProduct visualProduct = new VisualProduct();
                        using (var stream = item.OpenReadStream())
                        {
                            visualProduct.Photo = new byte[item.Length];
                            stream.Read(visualProduct.Photo, 0, (int)item.Length);
                        }
                        visualProducts.Add(visualProduct);
                    }
                }          
                HttpContext.Session.SetJson($"ProductBy_{_userManager.GetUserAsync(HttpContext.User).Result.Email}", product);
                HttpContext.Session.SetJson($"Visual_Products_{_userManager.GetUserAsync(HttpContext.User).Result.Email}", visualProducts);
                //return RedirectToAction("SelectMaterials");
                return Redirect("SelectMaterials");
                //_context.Add(product);
                //await _context.SaveChangesAsync();
                //return RedirectToAction(nameof(Index));
            }
            //ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "Name", product.CustomerId);
            ViewData["TypeProductId"] = new SelectList(_context.TypeProducts, "Id", "Type", product.TypeProductId);
            //return View(product);
            return View();
        }

        public async Task<IActionResult> TransitionToSelectMaterials(int productId) 
        {
            Product product = _context.Products.Include(p=>p.TypeProduct).FirstOrDefault(p=>p.Id == productId);
            if (product == null)
                return NotFound();

            HttpContext.Session.SetJson($"ProductBy_{_userManager.GetUserAsync(HttpContext.User).Result.Email}", product);
            return Redirect("SelectMaterials");
        }

        [Authorize(Roles ="customer")]
        public IActionResult SelectMaterials()
        {
            Product product = HttpContext.Session.GetJson<Product>($"ProductBy_{_userManager.GetUserAsync(HttpContext.User).Result.Email}");
            if (product == null)
                return NotFound();

            ViewBag.product = product;
            // Корзина
            List<CartItem> cart = HttpContext.Session.GetJson<List<CartItem>>($"{_userManager.GetUserAsync(HttpContext.User).Result.Email}_Material");
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
                    TotalAmount = cart.Sum(x => x.Quantity * x.Material.Cost)

                };
            }
            //smallCartModel = null;            

            ViewBag.smallCartModel = smallCartModel;
            return View(_context.Materials.ToList());
        }

        // GET: Products/Edit/5
        [Authorize(Roles ="customer")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var product = _context.Products.Include(p => p.BookingProducts).ThenInclude(bp => bp.Booking).FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            if (product.BookingProducts.Any(bp => bp.Booking.Status != "Ожидание"))
                return new StatusCodeResult(403);


            if (!IsCustomerProduct(product.Id))
                return new StatusCodeResult(403);

            List<byte[]> visualProducts = (from vp in _context.VisualProducts where vp.ProductId == product.Id select vp.Photo).ToList();
            ViewBag.visualProducts = visualProducts;

            ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "Name", product.CustomerId);
            ViewData["TypeProductId"] = new SelectList(_context.TypeProducts, "Id", "Type", product.TypeProductId);
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Cost,Description,TypeProductId,CustomerId")] Product product, IFormFile[]? photo, string? radioForPhoto)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    //Customer customer = _context.Customers.First(c => c.Email == _userManager.GetUserAsync(HttpContext.User).Result.Email);
                    //product.CustomerId = customer.Id;
                    _context.Update(product);
                    await _context.SaveChangesAsync();

                    CostController costController = new CostController(_context);
                    costController.SetCostProduct(product.Id);
                    costController.SetCostBookings(product.Id);

                    if (photo != null && photo.Count() > 0 && radioForPhoto != null)
                    {
                        foreach (var item in photo)
                        {
                            if (!CorrectExtensions(item.FileName))
                                return RedirectToAction("Index", "Error", new { errorMessage = "Файл для изображения должен иметь одно из следующих расширений: .jpg, .jpeg, .png, .tif, .bmp" });
                        }

                        if (radioForPhoto == "overwrite")
                        {
                            _context.VisualProducts.RemoveRange(_context.VisualProducts.Where(vp => vp.ProductId == product.Id));
                        }

                        foreach (var item in photo)
                        {
                            VisualProduct visualProduct = new VisualProduct();
                            using (var stream = item.OpenReadStream())
                            {
                                visualProduct.Photo = new byte[item.Length];
                                stream.Read(visualProduct.Photo, 0, (int)item.Length);
                                visualProduct.Product = product;
                                visualProduct.ProductId = product.Id;
                                _context.VisualProducts.Add(visualProduct);
                            }
                        }

                        _context.SaveChanges();
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                //return RedirectToAction(nameof(Index));
                return RedirectToAction("CustomerProducts", "Customers", new { emailCustomer = _userManager.GetUserAsync(HttpContext.User).Result.Email });
            }
            ViewData["TypeProductId"] = new SelectList(_context.TypeProducts, "Id", "Type", product.TypeProductId);
            return View(product);
        }

        // GET: Products/Delete/5
        [Authorize(Roles ="customer")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Customer)
                .Include(p => p.TypeProduct)
                .Include(p=>p.BookingProducts)
                .ThenInclude(bp=>bp.Booking)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            if (product.BookingProducts.Any(bp => bp.Booking.Status != "Ожидание"))
                return new StatusCodeResult(403);

            if (!IsCustomerProduct(product.Id))
                return new StatusCodeResult(403);

            List<byte[]> visualProducts = (from vp in _context.VisualProducts where vp.ProductId == product.Id select vp.Photo).ToList();
            ViewBag.visualProducts = visualProducts;

            return View(product);
            
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Products == null)
            {
                return Problem("Entity set 'PublishingDBContext.Products'  is null.");
            }
            var product = _context.Products.FirstOrDefault(p=> p.Id == id);
            List<Booking> productBookings = new List<Booking>();
            if (product != null)
            {
                bool isOneProductInBooking = false;
                foreach (var bookingProduct in product.BookingProducts)
                {
                    if (bookingProduct.Booking.BookingProducts.Count == 1)
                    {
                        isOneProductInBooking = true;
                        break;
                    }
                }

                if (isOneProductInBooking)
                    return RedirectToAction("Delete", new { id });

                productBookings = (from bp in _context.BookingProducts.Include(bp => bp.Booking) where bp.ProductId == id select bp.Booking).ToList();
                _context.Products.Remove(product);
            }
            
            await _context.SaveChangesAsync();
            if (productBookings.Count > 0)
            {
                CostController costController = new CostController(_context);
                foreach (var booking in productBookings)
                {
                    costController.SetCostBooking(booking);
                }
            }

            //return RedirectToAction(nameof(Index));
            return RedirectToAction("CustomerProducts", "Customers", new { emailCustomer = _userManager.GetUserAsync(HttpContext.User).Result.Email });
        }

        private bool ProductExists(int id)
        {
          return (_context.Products?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        [Authorize(Roles ="customer")]
        public IActionResult LinkProductWithBooking(int? customerId, int? productId) 
        {
            if (customerId == null || productId == null)
                return NotFound();

            if (!IsCustomerProduct(productId.Value))
                return new StatusCodeResult(403);

            Customer customer = _context.Customers.Include(c=>c.Products).Single(c=> c.Id == customerId);
            //Product product = _context.Products.Include(p => p.BookingProducts).Single(p => p.Id == productId);
            if(customer == null) //|| product == null)
                return NotFound();

            //return NotFound(product.BookingProducts.Count);

            var products = _context.Products.Where(p => customer.Products.Contains(p) && p.Id != productId).ToList();
            if (products == null)
                return NotFound();

            //return NotFound(products.Count);

            //List<Booking> bookings = new List<Booking>();

            var productBookings = from bp in _context.BookingProducts.Include(bp => bp.Product).Include(bp => bp.Booking) where bp.ProductId == productId select bp.Booking;
            var bookings = (from bp in _context.BookingProducts.Include(bp => bp.Product).Include(bp => bp.Booking) where products.Contains(bp.Product) && !productBookings.Contains(bp.Booking) && bp.Booking.Status == "Ожидание" select bp.Booking).Distinct().ToList();

            //foreach (var customerProduct in products)
            //{
            //    bookings.AddRange(from bp in _context.BookingProducts.Include(bp => bp.Booking) where bp.ProductId == customerProduct.Id && customerProduct.Id != productId && !bookings.Contains(bp.Booking) && bp.Booking.Status == "Ожидание" && !product.BookingProducts.Contains(bp) select bp.Booking);
            //}

            //foreach (var bookingProduct in product.BookingProducts)
            //{
            //    bookings.AddRange((from bp in _context.BookingProducts.Include(bp => bp.Booking).Include(bp => bp.Product) where bp.Booking.Status == "Ожидание" && bookingProduct.BookingId != bp.BookingId && !bookings.Contains(bp.Booking) select bp.Booking));
            //} 
            //var bookings = (from bp in _context.BookingProducts.Include(bp=> bp.Booking).Include(bp=> bp.Product) where bp.Booking.Status == "Ожидание" &&  select bp.Booking).ToList();


            //ViewData["bookings"] = new SelectList(bookings, "Id", "Id");
            LinkProductWithBookingViewModel model = new LinkProductWithBookingViewModel();
            model.Bookings = new SelectList(bookings.OrderBy(b=>b.Id), "Id", "Id");
            model.productId = (int)productId;

            return View(model);
        
        }

        [HttpPost]
        public async Task<IActionResult> LinkProductWithBooking(int? productId, int? bookingId, int? edition) 
        {
            if (productId == null || bookingId == null || edition == null)
                return NotFound();

            Product product = _context.Products.Find(productId);
            Booking booking = _context.Bookings.Find(bookingId);
            if (product == null || booking == null)
                return NotFound();

            //return NotFound($"bk status{booking.Status}, product name {product.Name}");
            BookingProduct bookingProduct = new BookingProduct();
            bookingProduct.Product = product;
            bookingProduct.Booking = booking;
            bookingProduct.BookingId = bookingId;
            bookingProduct.ProductId = product.Id;
            bookingProduct.Edition = (int)edition;
            //_context.BookingProducts.Attach(bookingProduct);
            _context.BookingProducts.Add(bookingProduct);
            _context.SaveChanges();

            CostController costController = new CostController(_context);
            costController.SetCostBooking(booking);

            return RedirectToAction("Details", new { id = productId });
        }

        [Authorize(Roles ="customer")]
        public async Task<IActionResult> UnpinMaterial(int? productId, int? materialId)
        {
            if (productId == null || materialId == null)
                return NotFound();

            if (!IsCustomerProduct(productId.Value))
                return new StatusCodeResult(403);

            ProductMaterial productMaterial = _context.ProductMaterials.Single(pm=> pm.ProductId == productId && pm.MaterialId == materialId);
            if (productMaterial == null) 
                return NotFound();

            _context.ProductMaterials.Remove(productMaterial);
            _context.SaveChanges();

            CostController costController = new CostController(_context);
            costController.SetCostProduct(productId);

            var bookings = (from bp in _context.BookingProducts.Include(bp => bp.Product).Include(bp => bp.Booking) where bp.ProductId == productId select bp.Booking).ToList();

            foreach (var booking in bookings)
            {
                costController.SetCostBooking(booking);
            }

            return RedirectToAction("Details", new {id=productId});
            //return Redirect(Request.Headers["Referer"].ToString());
        }

        private bool IsCustomerProduct(int productId) 
        {
            //var user =  _userManager.GetUserAsync(HttpContext.User);

            var user = _userManager.GetUserAsync(HttpContext.User);
            //var user = await _userManager.GetUserAsync(HttpContext.User);
            if (_userManager.IsInRoleAsync(user.Result, "customer").Result)
            {
                var product = _context.Products.FirstOrDefault(p => p.Id == productId);
                if (product == null) 
                    return false;

                var customer = _context.Customers.Include(c=>c.Products).FirstOrDefault(c => c.Email == user.Result.Email);
                if (customer == null)
                    return false;

                if (customer.Products.Contains(product))
                    return true;
                else
                    return false;

            }
            return true;
        }

        private bool CorrectExtensions(string filename) 
        {
            string[] extensions = { ".jpg", ".jpeg", ".png", ".bmp" };

            string extension = Path.GetExtension(filename);

            if (extensions.Contains(extension))
                return true;
            else
                return false;

        }
    }
}
