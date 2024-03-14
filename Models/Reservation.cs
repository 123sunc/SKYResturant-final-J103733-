
using System.ComponentModel.DataAnnotations;

namespace SKYResturant.Models
{
    public class Reservation
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public int NumberOfPeople { get; set; }

        [Required]
        public string ContactNumber { get; set; }

        public string SpecialRequests { get; set; }

    }
}
