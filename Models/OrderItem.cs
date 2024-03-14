using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SKYResturant.Models
{
    public class OrderItem
    {
        [Key]
        public int OrderItemId { get; set; }

        public int OrderNo { get; set; }

        [ForeignKey("FoodItem")]
        public int StockID { get; set; }
        public int Quantity { get; set; }

        public Menu FoodItem { get; set; }
    }
}
