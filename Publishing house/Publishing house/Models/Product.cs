using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Publishing_house.Models
{
    public class Product
    {
        public int Id { get; set; }
        [DisplayName("Номер тиража")]
        [DataType(DataType.Text)]
        [Range(typeof(int), "1", "1000", ErrorMessage = "Номер тиража должен входить в диапазон: [1, 1000]")]
        [RegularExpression(@"\d+", ErrorMessage = "Введите целое число")]
        public int? NumEdition { get; set; }
        [Required(ErrorMessage = "Введите название продукции")]
        [DataType(DataType.Text)]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Длина строки не входит в заданный диапазон: [1, 50]")]
        [DisplayName("Название продукции")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Выберите фото продукции")]
        [DataType(DataType.ImageUrl)]
        [DisplayName("Фото")]
        public string Visual { get; set; }
        [Required(ErrorMessage = "Введите тираж")]
        [DisplayName("Тираж")]
        [DataType(DataType.Text)]
        [Range(typeof(int), "1", "1000000", ErrorMessage = "Тираж должен входить в диапазон: [1, 1000000]")]   
        [RegularExpression(@"\d+", ErrorMessage = "Введите целое число")]
        public int Edition { get; set; }
        [Required(ErrorMessage = "Введите стоимость продукции")]
        [DisplayName("Стоимость")]
        [DataType(DataType.Currency)]
        [Range(typeof(double), "1", "10000", ErrorMessage = "Стоимость должна входить в  диапазон: [1, 10000]")]
        public double Cost { get; set; }
        [Required(ErrorMessage = "Введите тип продукции")]
        [DataType(DataType.Text)]
        [StringLength(30, MinimumLength = 5, ErrorMessage = "Длина строки не входит в заданный диапазон: [5, 30]")]
        [DisplayName("Тип")]
        [RegularExpression("^[А-Яа-я ]+$", ErrorMessage = "Разрешается ввести только русские буквы")]
        public string Type { get; set; }
        [Required(ErrorMessage = "Введите наценку в %")]
        [DisplayName("Наценка в %")]
        [Range(typeof(double), "0,1", "1000", ErrorMessage = "Стоимость должна входить в диапазон: [0.1, 1000]")]
        public double Margin { get; set; }
        public int BookingId { get; set; }
        public Booking? Booking { get; set; }
        public virtual ICollection<ProductMaterial> ProductMaterials { get; set; }
        public Product()
        {
            ProductMaterials = new List<ProductMaterial>();
        }
    }
}
    
