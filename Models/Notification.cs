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

        [Required]
        [StringLength(50)]
        public String Message { get; set; }

        [Required]
        [FutureOrTodayDate(ErrorMessage = "Notification date cannot be in the past.")]
        public DateTime NotificationDate { get; set; }

        [Required]
        [EnumDataType(typeof(NotificationStatus))]
        public NotificationStatus status { get; set; }

        public string? Link { get; set; }

        public Notification() {}
    }
}
