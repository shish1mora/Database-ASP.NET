namespace publishing.Models.ViewModels
{
    public class BookingIndexViewModel
    {
        public List<Booking> Bookings { get; set; }
        public List<Product> Products { get; set; }

        public BookingIndexViewModel() 
        {
            Bookings = new List<Booking>();
            Products = new List<Product>();
        }
    }
}
