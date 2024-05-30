namespace publishing.Models.ViewModels
{
    public class CustomerDetailsViewModel
    {
        public Customer Customer { get; set; }

        public List<Booking> Bookings { get; set; }

        public List<Product> Products { get; set; }
        public CustomerDetailsViewModel() 
        { 
            Bookings = new List<Booking>();
            Products = new List<Product>();
        }
    }
}
