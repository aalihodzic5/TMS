using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TMS.Models.Enums;

namespace TMS.Models
{
    public class Subscription
    {
        [Key]
        public string Id { get; set; } = default!;

        [ForeignKey("User")]
        public string UserId { get; set; } = default!;

        public User User { get; set; } = default!;


        [ForeignKey("Payment")]
        public string PaymentId { get; set; } 
        public Payment Payment { get; set; } = default!;



        public DateTime date { get; set; }

        public DateTime expirationDate { get; set; }

        [EnumDataType(typeof(SubscriptionStatus))]
        public SubscriptionStatus subscriptionStatus { get; set; }

        [Precision(18, 2)]
        public decimal subPrice { get; set; }


        public Subscription() { }


    }
}
