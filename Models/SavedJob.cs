using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace TMS.Models
{
    public class SavedJob
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("User")]
        [Required]
        public string UserId { get; set; }
        public User User { get; set; } = default!;

        [Required]
        public int JobId { get; set; }
        public Job Job { get; set; } = default!;

        [Required]  
        public DateTime savedDate { get; set; } = DateTime.Now;
        

    }
}
