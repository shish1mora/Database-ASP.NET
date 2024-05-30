using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace publishing.Models
{
    public class Material
    {

        public int Id { get; set; }

        [Required(ErrorMessage = "Введите тип материала")]
        [DataType(DataType.Text)]
        [StringLength(50, MinimumLength = 5, ErrorMessage = "Длина строки не входит в заданный диапазон:[5,50]")]
        [DisplayName("Тип материала")]
        [RegularExpression("^[А-Яа-я ]+$", ErrorMessage = "Разрешается ввести только русские буквы")]
        public string Type { get; set; }

        [Required(ErrorMessage = "Введите цвет материала")]
        [DataType(DataType.Text)]
        [StringLength(15, MinimumLength = 6, ErrorMessage = "Длина строки не входит в заданный диапазон:[5,15]")]
        [DisplayName("Цвет материала")]
        [RegularExpression("\\s*(0|[1-9]\\d?|1\\d\\d?|2[0-4]\\d|25[0-5])%?\\s*,\\s*(0|[1-9]\\d?|1\\d\\d?|2[0-4]\\d|25[0-5])%?\\s*,\\s*(0|[1-9]\\d?|1\\d\\d?|2[0-4]\\d|25[0-5])%?\\s*$", ErrorMessage = "Введите цвет материала как RGB. Пример:255,11,103")]
        public string Color { get; set; }

        [Required(ErrorMessage = "Введите размер материала")]
        [DataType(DataType.Text)]
        [DisplayName("Размер материала")]
        [StringLength(2, ErrorMessage = "Формат бумаги состоит из двух символов")]
        [RegularExpression(@"[A-C]{1}[0-9]{1}", ErrorMessage = "Введите стандартный размер бумаги. Пример: A4")]
        public string Size { get; set; }

        [Required(ErrorMessage = "Введите стоимость материала")]
        [DisplayName("Стоимость материала")]
        [DataType(DataType.Currency)] // Игнорируется мой ErrorMessage
        [Range( typeof(double),"0,1","100", ErrorMessage = "Стоимость должна входить в диапазон: [0.1,100]")]
        public double Cost { get; set; }

        public virtual ICollection<ProductMaterial> ProductMaterials { get; set; }

        public Material()
        {
            ProductMaterials = new List<ProductMaterial>();
        }

    }
}
