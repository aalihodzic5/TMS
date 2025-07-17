using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TMS.Models.Enums;

namespace TMS.Models
{
    public class Payment
    {
        [Key]
        public string Id { get; set; } = default!;

        [ForeignKey("Offer")]
        public int? OfferId { get; set; }
        public Offer? Offer { get; set; } = default!;

        [ForeignKey("User")]
        public string UserId { get; set; } = default!;
        public User User { get; set; } = default!;

        [EnumDataType(typeof(PaymentStatus))]
        public PaymentStatus paymentStatus { get; set; }

        public DateTime paymentDate { get; set; }

     
        public Payment() { }

    }
}
