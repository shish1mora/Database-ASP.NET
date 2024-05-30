using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace publishing.Models.ViewModels
{
    public class ProductDetailsViewModel
    {
        public Product Product { get; set; }
        public List<ProductMaterial> ProductMaterials { get; set; }
        public List<BookingProduct> BookingProducts { get; set; }

        [DisplayName("Общая стоимость")]
        [DataType(DataType.Currency)]
        [Range(typeof(double), "1", "1000000", ErrorMessage = "Стоимость должна входить в диапазон: [1,1000000]")]
        public double Cost { get; set; }
        public ProductDetailsViewModel() 
        {
            ProductMaterials = new List<ProductMaterial>();
            BookingProducts = new List<BookingProduct>();
        }
    }
}
