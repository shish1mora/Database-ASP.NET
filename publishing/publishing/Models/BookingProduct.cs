using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace publishing.Models
{
    public class BookingProduct
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Введите тираж")]
        [DisplayName("Тираж")]
        [DataType(DataType.Text)]
        [Range(typeof(int), "1", "10000", ErrorMessage = "Тираж должен входить в диапазон: [1,10000]")]
        [RegularExpression(@"\d+", ErrorMessage = "Введите целое число")]
        public int Edition { get; set; }

        public int? BookingId { get; set; }

        public Booking? Booking { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

    }
}
