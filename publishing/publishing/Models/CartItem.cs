namespace publishing.Models
{
    public class CartItem
    {
       public Product Product { get; set; }

       public Material Material { get; set; }
        public int Quantity { get; set; }

        

        //public double Total {
        //    get { return Product.Cost * Quantity; }
        //}

        public CartItem()
        {
            
        }

        public CartItem(Product product) 
        {
            Product = product;
            Quantity = 1;
        }

        public CartItem(Material material) 
        { 
            Material = material;
            Quantity = 1;
        }
    }
}
