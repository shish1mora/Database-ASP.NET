using System.Collections;

namespace publishing.Models.ViewModels
{
    public class MaterialDetailsViewModel
    {
        public Material Material { get; set; }

        public List <Product> Products { get; set; }

        public MaterialDetailsViewModel() 
        { 
            Products = new List<Product>();
        }
    }
}
