using Microsoft.EntityFrameworkCore;

namespace Publishing_house.Models
{
    public class PublishingDBcontext: DbContext
    {
        public PublishingDBcontext(DbContextOptions<PublishingDBcontext> options) : base(options)
        {

        }

        public DbSet<Publishing> Publishings { get; set; }
        public DbSet<Material> Materials { get; set; }
        public DbSet<PrintingHouse> PrintingHouses { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<BookingEmployee> BookingEmployees { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductMaterial> ProductMaterials { get; set; }
    }
    
}
