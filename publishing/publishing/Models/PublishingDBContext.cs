using Microsoft.EntityFrameworkCore;

namespace publishing.Models
{
    public class PublishingDBContext: DbContext
    {
        public PublishingDBContext(DbContextOptions<PublishingDBContext> options): base(options)
        {
                
        }

        public DbSet<Material> Materials { get; set; }
        public DbSet<PrintingHouse> PrintingHouses { get; set; }

        public DbSet<Customer> Customers { get; set; }

        public DbSet<Employee> Employees { get; set; }

        public DbSet<Booking> Bookings { get; set; }

        public DbSet<BookingProduct> BookingProducts { get; set; }

        public DbSet<Product> Products { get; set; }

        public DbSet<ProductMaterial> ProductMaterials { get; set; }

        public DbSet<TypeProduct> TypeProducts { get; set; }

        public DbSet<VisualProduct> VisualProducts { get; set; }
    }
}
