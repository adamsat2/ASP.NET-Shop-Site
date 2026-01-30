using System.ComponentModel.DataAnnotations;

namespace ShoppingCartApp.Models
{
    public class Item
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }

        [Range(1, 100, ErrorMessage = "The field Quantity must be between 1 and 100")]
        public int Quantity { get; set; }
    }
}