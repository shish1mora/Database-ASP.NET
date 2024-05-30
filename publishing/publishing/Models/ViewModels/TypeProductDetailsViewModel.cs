namespace publishing.Models.ViewModels
{
    public class TypeProductDetailsViewModel
    {
        public TypeProduct TypeProduct { get; set; }    

        public List<Product> Products { get; set; }

        public TypeProductDetailsViewModel() 
        { 
            Products = new List<Product>();
        }
    }
}
