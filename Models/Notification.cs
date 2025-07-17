using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TMS.Models.Enums;

namespace TMS.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; }
        public User User { get; set; } = default!;

        public String Message { get; set; }

        public DateTime NotificationDate { get; set; }

        [EnumDataType(typeof(NotificationStatus))]
        public NotificationStatus status { get; set; }


        public Notification() {}
    }
}
