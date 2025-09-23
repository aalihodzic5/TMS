using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TMS.Models.Enums;

namespace TMS.Models
{
    public class Offer
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Job")]
        public int JobId { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; } = default!;
        public User User { get; set; } = default!;

        public Job Job { get; set; } = default!;
        public String report { get; set; } = default!;
        public DateTime offerDate { get; set; }

        [Precision(18, 2)]
        [DisplayName("Offer Price")]
        public decimal price { get; set; }

        [EnumDataType(typeof(OfferState))]
        public OfferState offerState { get; set; }

        public Offer() { }
    }
}
