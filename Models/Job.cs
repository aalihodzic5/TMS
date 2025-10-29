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

        [Required]
        public string UserId { get; set; } = default!; 
        public User User { get; set; } = default!;

        public int? DriverId { get; set; }
        public Driver? Driver { get; set; } = default!;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Load Date")]
        [FutureOrTodayDate(ErrorMessage = "Load date cannot be in the past.")]
        public DateTime loadDate { get; set; }

        [Required]
        [Display(Name = "Trailer Type")]
        [EnumDataType(typeof(TrailerTypes))]
        public TrailerTypes TrailerTypes { get; set; }


        [Required]
        [Display(Name = "Load Type")]
        [EnumDataType(typeof(LoadType))]
        public LoadType LoadType { get; set; }


        [Required]
        [Display(Name = "Origin Distance (km)")]
        [Range(0, 1000, ErrorMessage = "Distance must be between 0 and 1000 km.")]
        public double? distanceOrigin { get; set; }
        
        
        [Required]
        [Display(Name = "Destination Distance (km)")]
        [Range(0, 3000, ErrorMessage = "Distance must be between 0 and 3000 km.")]
        public double distanceDestination { get; set; }

        [Required]
        [RegularExpression(@"^[A-Za-z0-9ČĆŽŠĐčćžšđ\s\.,\-]+$",
        ErrorMessage = "Location may contain only letters, numbers, spaces, dots, commas, and dashes.")]
        [StringLength(100, ErrorMessage = "Location cannot be longer than 100 characters.")]
        public string locationOrigin { get; set; } = default!;

        [Required]
        [Display(Name = "Destination Location")]
        [RegularExpression(@"^[A-Za-z0-9ČĆŽŠĐčćžšđ\s\.,\-]+$",
        ErrorMessage = "Location may contain only letters, numbers, spaces, dots, commas, and dashes.")]
        [StringLength(100, ErrorMessage = "Location cannot be longer than 100 characters.")]
        public string locationDestination { get; set; } = default!;

        [Required]
        [Display(Name = "Company Name")]
        [RegularExpression(@"^[A-Za-z0-9ČĆŽŠĐčćžšđ\s\.,'&\-]+$",
        ErrorMessage = "Company name can only contain letters, numbers, spaces, dots, commas, apostrophes, ampersand (&) and dashes.")]
        [StringLength(100, ErrorMessage = "Company name cannot be longer than 100 characters.")]
        public string companyName { get; set; } = default!;


        [Required]
        [Display(Name = "Load Weight (tons)")]
        [Range(0, 150, ErrorMessage = "Load weight must be between 0 and 150 tons.")]
        public double loadWeight { get; set; }

        [Required]
        [Display(Name = "Load Length (meters)")]
        [Range(0, 100, ErrorMessage = "Load length must be between 0 and 100 meters.")]
        public double loadLength { get; set; }

        [Required]
        [Display(Name = "Price (€)")]
        [Range(0, 1000000, ErrorMessage = "Price must be between 0 and 1,000,000 €.")]
        [Precision(18, 2)]
        public decimal price { get; set; }

        [Display(Name = "Additional Comments")]
        [StringLength(500, ErrorMessage = "Comments cannot be longer than 500 characters.")]
        public string comments { get; set; } = default!;

        [Required]
        [Display(Name = "Posting Date")]
        public DateTime postingDate { get; set; }



        public Job() { }
    }
}
