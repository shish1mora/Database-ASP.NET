using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace publishing.Models
{
    public class TypeProduct
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Введите тип продукции")]
        [DataType(DataType.Text)]
        [StringLength(30, MinimumLength = 5, ErrorMessage = "Длина строки не входит в заданный диапазон:[5,30]")]
        [DisplayName("Тип")]
        [RegularExpression("^[А-Яа-я ]+$", ErrorMessage = "Разрешается ввести только русские буквы")]
        public string Type { get; set; }

        [Required(ErrorMessage = "Введите наценку в %")]
        [DisplayName("Наценка в %")]
        [Range(typeof(double), "0,1", "1000", ErrorMessage = "Стоимость должна входить в диапазон: [0.1,1000]")]
        public double Margin { get; set; }

        public virtual ICollection<Product> Products { get; set; }

        public TypeProduct() 
        {
            Products = new List<Product>();
        }    
    }
}
