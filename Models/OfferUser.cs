using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TMS.Models
{
    public class OfferUser
    {
        [Key]
        public int id { get; set; }

        [ForeignKey("Offer")]
        public int? OfferId { get; set; }
        public Offer Offer { get; set; } = default!;

        [ForeignKey("User")]
        public string UserId { get; set; } = default!;
        public User User { get; set; } = default!;


        public OfferUser() { }
        
    }
}
