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
        public string FirstName { get; set; } = default!;

        [Required]
        public string LastName { get; set; } = default!;

        [Required]
        public string PhoneNumber { get; set; } = default!;

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [EnumDataType(typeof(DriverStatus))]
        public DriverStatus DriverStatus { get; set; } = DriverStatus.AVAILABLE;

        // Relacija: jedan vozač ima više licenci
        public ICollection<DriverLicence> DriverLicences { get; set; } = new List<DriverLicence>();
    }
}
