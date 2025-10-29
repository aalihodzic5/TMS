using System.ComponentModel.DataAnnotations;

namespace TMS.Models
{
    public class Truck
    {
        [Key]
        public int Id { get; set; }

        public string? UserID { get; set; } 
        public User? User { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Brand")]
        public string brand { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Model")]
        public string model { get; set; }

    
        [Display(Name = "License Plate")]
        [Required(ErrorMessage = "License plate is required.")]
        [StringLength(20, ErrorMessage = "License plate cannot be longer than 20 characters.")]
        [RegularExpression(@"^[A-Za-z0-9ČĆŽŠĐčćžšđ\-\s\.]+$",
        ErrorMessage = "License plate can contain only letters, numbers, spaces, dashes, and dots.")]
        public string licensePlate { get; set; }

        [Required]
        [Display(Name = "Specification")]
        [StringLength(1000, ErrorMessage = "Description can not be longer than 1000 characters")]
        public string specification { get; set; }

        [Required]
        [Display(Name = "Next Service Date")]
        public DateTime nextServiceDate { get; set; }

        [Required]
        [Display(Name = "Registration Date")]
        public DateTime registration { get; set; }


        public Truck() { }


    }
}
