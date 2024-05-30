using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using publishing.Areas.Identity.Data;
using publishing.Models;
using Quartz;

namespace publishing.Infrastructure
{
    public class ExecutionBookingReminder: IJob
    {
        private readonly PublishingDBContext _context;
        public ExecutionBookingReminder(PublishingDBContext context)
        {
            _context = context;
        }



        public async Task Execute(IJobExecutionContext context)
        {
            var bookings = _context.Bookings.Include(b => b.BookingProducts).ThenInclude(bp => bp.Product).ThenInclude(p => p.Customer).ToList();
            foreach (Booking booking in bookings)
            {

                if (booking.Status == "Выполняется" && (booking.End.Value.Date - DateTime.Now.Date).Days == 1)
                {
                    string subject = $"Уведомления о выполнении заказа №{booking.Id}";
                    string message = $"Уважаемый(-ая) {booking.BookingProducts.First().Product.Customer.Name}! Завтра должно завершиться выполнение заказа №{booking.Id} <br>С уважением, \"Издательство\".";
                    EmailSender emailSender = new EmailSender();
                    emailSender.SendEmail(booking.BookingProducts.First().Product.Customer.Email, subject, message);
                }
            }
        }
    }
}
