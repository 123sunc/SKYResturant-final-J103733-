using System.ComponentModel.DataAnnotations;

namespace SKYResturant.Models
{
    public class OrderHistory
    {
        [Key]
        public int OrderNo { get; set; }
        public string Email { get; set; }
        public List<OrderItem> OrderItems { get; set; }
    }
}
