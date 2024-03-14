using System.ComponentModel.DataAnnotations;

namespace SKYResturant.Models
{
    public class CheckoutItem
    {
        [Key, Required]
        public int Id { get; set; }
        [Required]
        public Decimal Price { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
    }
}
