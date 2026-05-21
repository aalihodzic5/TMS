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

        [DisplayName("Report")]
        [StringLength(1000, ErrorMessage = "Report cannot be longer than 1000 characters.")]
        public String report { get; set; } = default!;

        [DisplayName("Offer Date")]
        public DateTime offerDate { get; set; }

        [Precision(18, 2)]
        [DisplayName("Offer Price")]
        public decimal price { get; set; }

        [DisplayName("Offer State")]
        [EnumDataType(typeof(OfferState))]
        public OfferState offerState { get; set; }

        public Offer() { }
    }
}
