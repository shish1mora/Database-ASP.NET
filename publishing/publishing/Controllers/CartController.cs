using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using publishing.Areas.Identity.Data;
using publishing.Models;
using publishing.Models.ViewModels;
using publishing.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata.Ecma335;
using Microsoft.AspNetCore.Authorization;
using System.IO;
using System.Drawing;

namespace publishing.Controllers
{
    [Authorize(Roles ="customer")]
    public class CartController : Controller
    {
        private readonly PublishingDBContext _context;
        private readonly UserManager<publishingUser> _userManager;
        private readonly IWebHostEnvironment _appEnvironment;
        private string JsonProduct { get { return $"{_userManager.GetUserAsync(HttpContext.User).Result.Email}_Product"; } }
        private string JsonMaterial { get { return $"{_userManager.GetUserAsync(HttpContext.User).Result.Email}_Material"; } }

        private string NameProduct { get { return $"ProductBy_{_userManager.GetUserAsync(HttpContext.User).Result.Email}"; } }

        private string NameBooking { get { return $"BookingBy_{_userManager.GetUserAsync(HttpContext.User).Result.Email}";} }

        private string NameVisualProducts { get { return $"Visual_Products_{_userManager.GetUserAsync(HttpContext.User).Result.Email}"; } }

        public CartController(PublishingDBContext context, UserManager<publishingUser> userManager, IWebHostEnvironment appEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _appEnvironment = appEnvironment;
        }

        public IActionResult Index()
        {
            int bookingId = HttpContext.Session.GetJson<int>(NameBooking);
            if (bookingId == 0)
                return NotFound();

            ViewBag.bookingId = bookingId;

            List<CartItem> cart = HttpContext.Session.GetJson<List<CartItem>>(JsonProduct) ?? new List<CartItem>();
            CartViewModel smallCartModel = new()
            {
                CartItems = cart,
                GrandTotal = cart.Sum(x => x.Quantity * x.Product.Cost)
            };
            return View(smallCartModel);
        }

        public IActionResult IndexMaterials()
        {
            List<CartItem> cart = HttpContext.Session.GetJson<List<CartItem>>(JsonMaterial) ?? new List<CartItem>();
            Product product = HttpContext.Session.GetJson<Product>(NameProduct);
            if (product == null)
                return NotFound();

            ViewBag.product = product;

            CartViewModel smallCartModel = new()
            {
                CartItems = cart,
                GrandTotal = cart.Sum(x => x.Quantity * x.Material.Cost)
            };
            return View(smallCartModel);
        }

        public async Task<IActionResult> Add(int id, string type)
        {
            if (type == "product")
            {
                Product product = _context.Products.FirstOrDefault(p => p.Id == id);
                if (product == null)
                    return NotFound();

                List<CartItem> cart = HttpContext.Session.GetJson<List<CartItem>>(JsonProduct) ?? new List<CartItem>();
                CartItem cartItem = cart.Where(c => c.Product.Id == id).FirstOrDefault();
                if (cartItem == null)
                    cart.Add(new CartItem(product));
                else
                    cartItem.Quantity += 1;

                HttpContext.Session.SetJson(JsonProduct, cart);
            }
            if (type == "material") 
            {
                Material material = _context.Materials.FirstOrDefault(p => p.Id == id);
                if (material == null)
                    return NotFound();

                List<CartItem> cart = HttpContext.Session.GetJson<List<CartItem>>(JsonMaterial) ?? new List<CartItem>();
                CartItem cartItem = cart.Where(c => c.Material.Id == id).FirstOrDefault();
                if (cartItem == null)
                    cart.Add(new CartItem(material));
                else
                    cartItem.Quantity += 1;

                HttpContext.Session.SetJson(JsonMaterial, cart);                
            }

            return Redirect(Request.Headers["Referer"].ToString());

        }

        public async Task<IActionResult> Decrease(int id,string type)
        {
            if (type == "product")
            {
                List<CartItem> cart = HttpContext.Session.GetJson<List<CartItem>>(JsonProduct);
                CartItem cartItem = cart.Where(c => c.Product.Id == id).FirstOrDefault();

                if (cartItem.Quantity > 1)
                    --cartItem.Quantity;
                else
                    cart.RemoveAll(p => p.Product.Id == id);

                if (cart.Count == 0)
                    HttpContext.Session.Remove(JsonProduct);
                else
                    HttpContext.Session.SetJson(JsonProduct, cart);
            }
            if (type == "material") 
            {
                List<CartItem> cart = HttpContext.Session.GetJson<List<CartItem>>(JsonMaterial);
                CartItem cartItem = cart.Where(c => c.Material.Id == id).FirstOrDefault();

                if (cartItem.Quantity > 1)
                    --cartItem.Quantity;
                else
                    cart.RemoveAll(p => p.Material.Id == id);

                if (cart.Count == 0)
                    HttpContext.Session.Remove(JsonMaterial);
                else
                    HttpContext.Session.SetJson(JsonMaterial, cart);
            }


            return Redirect(Request.Headers["Referer"].ToString());
        }

        public async Task<IActionResult> Remove(int id,string type)
        {
            if (type == "product")
            {
                List<CartItem> cart = HttpContext.Session.GetJson<List<CartItem>>(JsonProduct);
                cart.RemoveAll(p => p.Product.Id == id);

                if (cart.Count == 0)
                    HttpContext.Session.Remove(JsonProduct);
                else
                    HttpContext.Session.SetJson(JsonProduct, cart);
            }
            if (type == "material") 
            {
                List<CartItem> cart = HttpContext.Session.GetJson<List<CartItem>>(JsonMaterial);
                cart.RemoveAll(p => p.Material.Id == id);

                if (cart.Count == 0)
                    HttpContext.Session.Remove(JsonMaterial);
                else
                    HttpContext.Session.SetJson(JsonMaterial, cart);
            }
            return Redirect(Request.Headers["Referer"].ToString());
        }

        public IActionResult Clear(string type)
        {
            if (type == "product")
                HttpContext.Session.Remove(JsonProduct);
            if(type== "material")
                HttpContext.Session.Remove(JsonMaterial);

            return Redirect(Request.Headers["Referer"].ToString());
        }

        [HttpPost]
        public async Task<IActionResult> SetBooking(double? grandTotal)
        {
            int bookingId = HttpContext.Session.GetJson<int>(NameBooking);
 
            if (bookingId == 0 || grandTotal == null)
                return NotFound();

            List<CartItem> cartItems = HttpContext.Session.GetJson<List<CartItem>>(JsonProduct);

            if (bookingId == -1)
            {
                Booking booking = new Booking();
                booking.Start = DateTime.Now.Date;
                booking.Status = "Ожидание";
                booking.Cost = grandTotal.Value;
                
                _context.Bookings.Add(booking);
                _context.SaveChanges();

                Booking lastBooking = _context.Bookings.Include(b=> b.BookingProducts).OrderByDescending(b => b.Id).First();
                foreach (var cartItem in cartItems)
                {
                    BookingProduct bookingProduct = new BookingProduct();
                    bookingProduct.Product = _context.Products.First(p => p.Id == cartItem.Product.Id);
                    bookingProduct.ProductId = cartItem.Product.Id;
                    bookingProduct.Booking = lastBooking;
                    bookingProduct.BookingId = lastBooking.Id;
                    bookingProduct.Edition = cartItem.Quantity;

                    _context.BookingProducts.Add(bookingProduct);
                }
            }
            else 
            {
                Booking existBooking = _context.Bookings.First(b => b.Id == bookingId);
                if (existBooking == null)
                    return NotFound();

                existBooking.Cost += grandTotal.Value;

                foreach (var cartItem in cartItems)
                {
                    BookingProduct existBookingProduct = _context.BookingProducts.FirstOrDefault(bp => bp.ProductId == cartItem.Product.Id && bp.BookingId == existBooking.Id);
                    if (existBookingProduct != null)
                    {
                        existBookingProduct.Edition += cartItem.Quantity;
                    }
                    else 
                    {
                        existBookingProduct = new BookingProduct();
                        existBookingProduct.Product = _context.Products.First(p => p.Id == cartItem.Product.Id);
                        existBookingProduct.ProductId = cartItem.Product.Id;
                        existBookingProduct.Booking = existBooking;
                        existBookingProduct.BookingId = existBooking.Id;
                        existBookingProduct.Edition = cartItem.Quantity;

                        _context.BookingProducts.Add(existBookingProduct);
                    }
                }
            }

            HttpContext.Session.Remove(JsonProduct);
            HttpContext.Session.Remove(NameBooking);
            _context.SaveChanges();
            return RedirectToAction("CustomerBookings", "Customers", new {emailCustomer = _userManager.GetUserAsync(HttpContext.User).Result.Email});
        }

        [HttpPost]
        public async Task<IActionResult> SetProduct(double? totalCost) 
        {
            if (totalCost == null)
                return NotFound();

            List<CartItem> cartItems = HttpContext.Session.GetJson<List<CartItem>>(JsonMaterial);
            Product product = HttpContext.Session.GetJson<Product>(NameProduct);
            if (product == null)
                return NotFound();

            var userEmail = _userManager.GetUserAsync(HttpContext.User).Result.Email;
            //return NotFound($"{product.TypeProduct.Type}, {product.TypeProductId}");


            if (product.Id == 0)
            {
                product.Cost = totalCost.Value;
              
                Customer customer = _context.Customers.First(c => c.Email == userEmail);
                product.Customer = customer;
                product.CustomerId = customer.Id;

                _context.Entry(product).State = EntityState.Added;
                //_context.Products.Add(product);
                _context.SaveChanges();

                Product lastProduct = _context.Products.Include(b => b.ProductMaterials).OrderByDescending(b => b.Id).First();
                foreach (var cartItem in cartItems)
                {
                    ProductMaterial productMaterial = new ProductMaterial();
                    productMaterial.Material = _context.Materials.First(m => m.Id == cartItem.Material.Id);
                    productMaterial.MaterialId = cartItem.Material.Id;
                    productMaterial.Product = lastProduct;
                    productMaterial.ProductId = lastProduct.Id;
                    productMaterial.CountMaterials = cartItem.Quantity;

                    _context.ProductMaterials.Add(productMaterial);
                }

                List<VisualProduct> visualProducts = HttpContext.Session.GetJson<List<VisualProduct>>(NameVisualProducts);
                if (visualProducts.Count != 0) 
                {
                    foreach (var visualProduct in visualProducts)
                    {
                        visualProduct.ProductId = lastProduct.Id;
                        visualProduct.Product = lastProduct;
                        
                        _context.Entry(visualProduct).State = EntityState.Added;
                    }
                }
            }
            else
            {
                //Product existProduct = _context.Products.Find(product.Id);
                product.Cost += totalCost.Value;

                foreach (var cartItem in cartItems)
                {
                    ProductMaterial existProductMaterial = _context.ProductMaterials.FirstOrDefault(bp => bp.ProductId == product.Id && bp.MaterialId == cartItem.Material.Id);
                    if (existProductMaterial != null)
                    {
                        existProductMaterial.CountMaterials += cartItem.Quantity;
                    }
                    else
                    {
                        ProductMaterial productMaterial = new ProductMaterial();
                        productMaterial.Material = _context.Materials.First(m => m.Id == cartItem.Material.Id);
                        productMaterial.MaterialId = cartItem.Material.Id;
                        //productMaterial.Product = existProduct;
                        productMaterial.ProductId = product.Id;
                        productMaterial.CountMaterials = cartItem.Quantity;

                        _context.ProductMaterials.Add(productMaterial);
                    }
                }

                _context.Entry(product).State = EntityState.Modified;
            }

            _context.SaveChanges();
            HttpContext.Session.Remove(JsonMaterial);
            HttpContext.Session.Remove(NameProduct);
            HttpContext.Session.Remove(NameVisualProducts);

            CostController costController = new CostController(_context);
            //costController.SetCostProduct(product.Id);
            costController.SetCostBookings(product.Id);

            //CostController costController = new CostController(_context);
            //costController.SetCostProduct(product.Id);
            
            return RedirectToAction("CustomerProducts", "Customers", new { emailCustomer = userEmail});
        }
    }
}
