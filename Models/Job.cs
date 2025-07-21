using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TMS.Models.Enums;

namespace TMS.Models
{
    public class Job
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Driver")]
        public int? DriverId { get; set; }
        public Driver? Driver { get; set; } = default!;

        public DateTime loadDate { get; set; }

        [EnumDataType(typeof(TrailerTypes))]
        public TrailerTypes TrailerTypes { get; set; }

        [EnumDataType(typeof(LoadType))]
        public LoadType LoadType { get; set; }
        public double distanceOrigin { get; set; }
        public double distanceDestination { get; set; }
        public string locationOrigin { get; set; } = default!;
        public string locationDestination { get; set; } = default!;
        public string companyName { get; set; } = default!;
        public double loadWeight { get; set; }
        public double loadLength { get; set; }

        [Precision(18, 2)]
        public decimal price { get; set; }
        public string comments { get; set; } = default!;
        public DateTime postingDate { get; set; }



        public Job() { }
    }
}
