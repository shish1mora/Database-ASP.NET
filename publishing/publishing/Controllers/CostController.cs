using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using publishing.Models;

namespace publishing.Controllers
{
    public class CostController
    {
        private readonly PublishingDBContext _context;

        public CostController(PublishingDBContext context)
        {
            _context = context;
        }

        public void SetCostBookings(int productId) 
        {
            var bookings = _context.BookingProducts.Include(b=>b.Booking).Where(bp => bp.ProductId == productId).ToList();
            if (bookings.Count == 0)
                return;
            else 
            { 
                foreach (var book in bookings) 
                {
                    
                    SetCostBooking(book.Booking);
                }
            }
        }


        public void SetCostBooking(Booking? booking) {
            
            double cost = 0;
            if (booking != null)
            {
                var bookingsProducts = _context.BookingProducts.Include(bp => bp.Product).Include(bp => bp.Booking).Where(bp => bp.BookingId == booking.Id).ToList();
                //var check = (from bp in _context.BookingProducts.Include(bp => bp.Product).Include(bp => bp.Booking) where bp.BookingId == id select bp).ToList();
                if (bookingsProducts.Count > 0)
                {
                    foreach (var bookingProduct in bookingsProducts)
                    {
                        cost += bookingProduct.Product.Cost * bookingProduct.Edition;
                    }

                    booking.Cost = cost;
                    _context.SaveChanges();
                }
            }


        }

        public void SetCostProduct(int? productId) 
        {
            double cost = 0;
            if (productId != null)
            {
                Product product = _context.Products.Include(p=>p.TypeProduct).Single(p=> p.Id == productId);
                if (product != null) 
                {
                    var productMaterials = _context.ProductMaterials.Include(pm => pm.Material).Include(pm => pm.Product).Where(pm => pm.ProductId == productId).ToList();
                    if (productMaterials.Count > 0) 
                    {
                        foreach (var productMaterial in productMaterials)
                        {
                            cost += productMaterial.Material.Cost * productMaterial.CountMaterials;
                        }

                        cost = cost * ((100 + product.TypeProduct.Margin) / 100);

                        product.Cost = cost;
                        _context.SaveChanges();
                    }
                }
            }
        }
    }
}
