using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations.Schema;

namespace publishing.Models
{
    public class VisualProduct
    {
        public int Id { get; set; }
        public byte[] Photo { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

    }
}
