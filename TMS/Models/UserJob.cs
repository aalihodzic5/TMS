using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TMS.Models
{
    public class UserJob
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; } = default!;

        public User User { get; set; } = default!;

        [ForeignKey("Job")]
        public int JobId { get; set; }
        public Job Job { get; set; } = default!;

        public UserJob() { }
    }
}
