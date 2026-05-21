using System.ComponentModel.DataAnnotations;

namespace TMS.Models.Enums
{
    public enum DriverStatus
    {
        [Display (Name =  "AVAILABLE")]
        AVAILABLE,
        [Display (Name = "ON THE ROAD")]
        ON_THE_ROUTE
    }
}
