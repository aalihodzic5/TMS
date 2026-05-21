using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TMS.Models.Enums;

namespace TMS.Models
{
    public class Trailer
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Truck")]
        public int TruckId { get; set; }
        public Truck? Truck { get; set; } = default!;

        [Required]
        [StringLength(50)]
        [Display(Name = "Brand")]
        public string brand { get; set; } = default!;

        [Required]
        [StringLength(50)]
        [Display(Name = "Trailer Type")]
        [EnumDataType(typeof(TrailerTypes))]
        public TrailerTypes trailerType { get; set; }

        [Required(ErrorMessage = "License plate is required.")]
        [StringLength(20, ErrorMessage = "License plate cannot be longer than 20 characters.")]
        [RegularExpression(@"^[A-Za-z0-9ČĆŽŠĐčćžšđ\-\s\.]+$",
        ErrorMessage = "License plate can contain only letters, numbers, spaces, dashes, and dots.")]
        public string licensePlate { get; set; } = default!;

        [Required]
        [Display(Name = "Registration Date")]
        public DateTime registration { get; set; }

        [Required]
        [Display(Name = "Next Service Date")]
        public DateTime nextServiceDate { get; set; }

        [Required]
        [Display(Name = "Specification")]
        [StringLength(1000, ErrorMessage = "Description can not be longer than 1000 characters")]
        public string specification { get; set; } = default!;


        public Trailer() { }

    }
}
