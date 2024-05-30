using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using publishing.Areas.Identity.Data;
using publishing.Models;

namespace publishing.Controllers
{
    public class FilterController : Controller
    {
        private readonly PublishingDBContext _context;

        public FilterController(PublishingDBContext context)
        {
            _context = context;

        }

        // Join Bookings

        public List<Booking> GetBookingWithCertainStatus(string status, List<Booking> bookings)
        {
            List<Booking> newListBookings = bookings.Where(b => b.Status == status).ToList();
            return newListBookings;
        }

        public List<Booking> GetBookingWithSertainDate(string date, DateTime? startDate, DateTime? endDate, List<Booking> bookings)
        {
            List<Booking> newListBookings = new List<Booking>();

            if (date == "Дата приёма")
            {
                if (startDate != null & endDate == null)
                    newListBookings = bookings.Where(b => b.Start.Date >= startDate.Value.Date).ToList();
                else if (startDate == null & endDate != null)
                    newListBookings = bookings.Where(b => b.Start.Date <= endDate.Value.Date).ToList();
                else if (startDate != null & endDate != null)
                    newListBookings = bookings.Where(b => b.Start.Date >= startDate.Value.Date && b.Start.Date <= endDate.Value.Date).ToList();
            }
            else if (date == "Дата выполнения")
            {
                if (startDate != null & endDate == null)
                    newListBookings = bookings.Where(b => b.Status != "Ожидание" && b.End.Value.Date >= startDate.Value.Date).ToList();
                else if (startDate == null & endDate != null)
                    newListBookings = bookings.Where(b => b.Status != "Ожидание" && b.End.Value.Date <= endDate.Value.Date).ToList();
                else if (startDate != null & endDate != null)
                    newListBookings = bookings.Where(b => b.Status != "Ожидание" && b.End.Value.Date >= startDate.Value.Date && b.End.Value.Date <= endDate.Value.Date).ToList();
            }


            return newListBookings;
        }

        public List<Booking> GetBookingWithSertainNumber(int? startNumber, int? endNumber, List<Booking> bookings)
        {
            List<Booking> newListBookings = new List<Booking>();

            if (startNumber != null & endNumber == null)
                newListBookings = bookings.Where(b => b.Id >= startNumber.Value).ToList();
            else if (startNumber == null & endNumber != null)
                newListBookings = bookings.Where(b => b.Id <= endNumber.Value).ToList();
            else if (startNumber != null & endNumber != null)
                newListBookings = bookings.Where(b => b.Id >= startNumber.Value && b.Id <= endNumber.Value).ToList();


            return newListBookings;

        }

        public List<Booking> GetBookingsWithCertainCost(double? startCost, double? endCost, List<Booking> bookings)
        {
            List<Booking> newListBookings = new List<Booking>();

            if (startCost != null & endCost == null)
                newListBookings = bookings.Where(b => b.Cost >= startCost.Value).ToList();
            else if (startCost == null & endCost != null)
                newListBookings = bookings.Where(b => b.Cost <= endCost.Value).ToList();
            else if (startCost != null & endCost != null)
                newListBookings = bookings.Where(b => b.Cost >= startCost.Value && b.Cost <= endCost.Value).ToList();


            return newListBookings;
        }


        // Join Products

        public List<Product> GetProductsWithCertainCost(double? startCost, double? endCost, List<Product> products) 
        {
            List<Product> newListProducts = new List<Product>();

            if (startCost != null & endCost == null)
                newListProducts = products.Where(p => p.Cost >= startCost.Value).ToList();
            else if (startCost == null & endCost != null)
                newListProducts = products.Where(p => p.Cost <= endCost.Value).ToList();
            else if (startCost != null & endCost != null)
                newListProducts = products.Where(p => p.Cost >= startCost.Value && p.Cost <= endCost.Value).ToList();


            return newListProducts;
        }


        public List<Product> GetProductsWithCertainName(string name, List<Product> products) 
        {
            List<Product> newListProducts = new List<Product>();

            newListProducts = products.Where(p => p.Name == name).ToList();

            return newListProducts;
        }

        public List<Product> GetProductsWithCertainType(string type, List<Product> products)
        {
            List<Product> newListProducts = new List<Product>();

            newListProducts = products.Where(p => p.TypeProduct.Type == type).ToList();

            return newListProducts;
        }
    }
}
