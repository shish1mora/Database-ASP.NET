using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace publishing.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Введите название продукции")]
        [DataType(DataType.Text)]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Длина строки не входит в заданный диапазон:[1,50]")]
        [DisplayName("Название продукции")]
        public string Name { get; set; }

        //[Required(ErrorMessage = "Введите стоимость продукции")]
        [DisplayName("Стоимость продукции")]
        [DataType(DataType.Currency)]
        //[Range(typeof(double), "1", "10000", ErrorMessage = "Стоимость должна входить в диапазон: [1,10000]")]
        public double Cost { get; set; }

        [Required(ErrorMessage = "Введите описание продукции")]
        [DisplayName("Описание продукции")]
        public string Description { get; set; }

        public int TypeProductId { get; set; }
        public TypeProduct? TypeProduct { get; set; }

        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }
 
        public virtual ICollection<ProductMaterial> ProductMaterials { get; set; }
        public virtual ICollection<BookingProduct> BookingProducts { get; set; }

        public Product()
        {
            ProductMaterials = new List<ProductMaterial>(); 
            BookingProducts = new List<BookingProduct>();
        }

    }
}
