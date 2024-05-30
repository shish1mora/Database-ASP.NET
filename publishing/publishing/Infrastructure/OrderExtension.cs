using Microsoft.EntityFrameworkCore;
using publishing.Models;
using Quartz;

namespace publishing.Infrastructure
{
    public class OrderExtension: IJob
    {
        private readonly PublishingDBContext _context;

        public OrderExtension(PublishingDBContext context) 
        {
            _context = context;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var bookings = _context.Bookings.Include(b => b.BookingProducts).ThenInclude(bp => bp.Product).ThenInclude(p => p.Customer).ToList();

            foreach (Booking booking in bookings)
            {
                if (booking.Status == "Выполняется" && (DateTime.Now.Date - booking.End.Value.Date).Days > 0)
                {
                    string pastEnd = booking.End.Value.ToString("d");
                    booking.End = booking.End.Value.AddDays(7);
                    string subject = $"Изменение в заказе №{booking.Id}";
                    string message = $"Уважаемый(-ая) {booking.BookingProducts.First().Product.Customer.Name}! Дата выполнения вашего заказа перенесена с {pastEnd} на {booking.End.Value.ToString("dd/MM/yyyy")}. Просим прощения за неудобства.<br>С уважением, \"Издательство\".";
                    EmailSender emailSender = new EmailSender();
                    emailSender.SendEmail(booking.BookingProducts.First().Product.Customer.Email, subject, message);
                }
            }

           await _context.SaveChangesAsync();
        }
    }
}
