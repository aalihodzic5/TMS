using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TMS.Models.Enums;

namespace TMS.Models
{
    public class Driver
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("User")]
        public string? UserID { get; set; }
        public User? User { get; set; } = default!;

        [ForeignKey("Truck")]
        public int? TruckId { get; set; }
        public Truck? Truck { get; set; } = default!;

        [Required]
        [Display(Name = "First Name")]
        [RegularExpression(@"^[A-ZČĆŽŠĐ][a-zčćžšđA-ZČĆŽŠĐ'\-]{1,50}$",
            ErrorMessage = "First name must start with a capital letter and contain only letters, apostrophes, or hyphens.")]
        public string FirstName { get; set; } = default!;

        [Required]
        [Display(Name = "Last Name")]
        [RegularExpression(@"^[A-ZČĆŽŠĐ][a-zčćžšđA-ZČĆŽŠĐ'\-]{1,50}$",
            ErrorMessage = "Last name must start with a capital letter and contain only letters, apostrophes, or hyphens.")]
        public string LastName { get; set; } = default!;

        [Required]
        [Display(Name = "Phone Number")]
        [RegularExpression(@"^\+?[0-9]{1,3}?[-. ]?[0-9]{2,4}[-. ]?[0-9]{2,4}[-. ]?[0-9]{3,4}$",
            ErrorMessage = "Invalid phone number format.")]
        public string PhoneNumber { get; set; } = default!;

        [Required]
        [Display(Name = "Date Of Birth")]
        [DataType(DataType.Date)]
        [RegularExpression(@"^\d{4}-(0[1-9]|1[0-2])-(0[1-9]|[12][0-9]|3[01])$",
            ErrorMessage = "Date must be in YYYY-MM-DD format.")]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [Display(Name = "Driver Status")]
        [EnumDataType(typeof(DriverStatus))]
        public DriverStatus DriverStatus { get; set; } = DriverStatus.AVAILABLE;

        // Relacija: jedan vozač ima više licenci
        [Display(Name = "Driver Licences")]
        public ICollection<DriverLicence> DriverLicences { get; set; } = new List<DriverLicence>();

        public Driver() { }
    }
}
