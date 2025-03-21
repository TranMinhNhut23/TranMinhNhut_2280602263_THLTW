using System.ComponentModel.DataAnnotations;

namespace TranMinhNhut_2280602263.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public decimal Price { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public List<ProductImage>? Images { get; set; }
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
        public int Quantity { get; set; } // Add this line to include the Quantity property
    }

}
