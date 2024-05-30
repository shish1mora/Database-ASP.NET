using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace publishing.Models.ViewModels
{
    public class LinkProductWithBookingViewModel
    {
        public int productId { get; set; }

        [Required(ErrorMessage = "Введите тираж")]
        [DisplayName("Тираж")]
        [DataType(DataType.Text)]
        [Range(typeof(int), "1", "10000", ErrorMessage = "Тираж должен входить в диапазон: [1,10000]")]
        [RegularExpression(@"\d+", ErrorMessage = "Введите целое число")]
        public int Edition { get; set; } = 1;

        public SelectList Bookings { get; set; }

    }
}
