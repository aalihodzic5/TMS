using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TMS.Models.Enums;

namespace TMS.Models
{
    public class DriverLicence
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Driver")]
        public int driverId { get; set; }
        public Driver Driver { get; set; } = default!;

        [Required]
        [EnumDataType(typeof(Licence))]
        public Licence Licence { get; set; }
    }
}
