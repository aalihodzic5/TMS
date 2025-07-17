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
        public Truck Truck { get; set; } = default!;


        public string brand { get; set; } = default!;
        [EnumDataType(typeof(TrailerTypes))]
        public TrailerTypes trailerType { get; set; }
        public string licensePlate { get; set; } = default!;
        public DateTime registration { get; set; }
        public DateTime nextServiceDate { get; set; }
        public string specification { get; set; } = default!;


        public Trailer() { }

    }
}
