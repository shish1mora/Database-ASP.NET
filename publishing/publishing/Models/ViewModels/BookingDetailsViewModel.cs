using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace publishing.Models.ViewModels
{
    public class BookingDetailsViewModel
    {
        public Booking Booking { get; set; }
        //public List<Product> Products { get; set; }
        public List<BookingProduct> BookingProducts { get; set; }

        [DisplayName("Общая стоимость")]
        [DataType(DataType.Currency)]
        [Range(typeof(double), "1", "1000000", ErrorMessage = "Стоимость должна входить в диапазон: [1,1000000]")]
        public double Cost { get; set; }

        public BookingDetailsViewModel() 
        { 
            //Products= new List<Product>();
            BookingProducts = new List<BookingProduct>();
        }
    }
}
