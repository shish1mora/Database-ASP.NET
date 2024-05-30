namespace publishing.Models.ViewModels
{
    public class CartViewModel
    {
        public List<CartItem> CartItems { get; set; }
        public double GrandTotal { get; set; }

        public CartViewModel() 
        { 
            CartItems = new List<CartItem>();
        }
    }
}
