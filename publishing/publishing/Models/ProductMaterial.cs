using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace publishing.Models
{
    public class ProductMaterial
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Введите количество материала")]
        [DisplayName("Количество материала")]
        [DataType(DataType.Text)]
        [Range(typeof(int), "1", "10000", ErrorMessage = "Количество материала должно входить в диапазон: [1,10000]")]
        [RegularExpression(@"\d+", ErrorMessage = "Введите целое число")]
        public int CountMaterials { get; set; }

        public int MaterialId { get; set; }

        public Material? Material { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }
    }
}
