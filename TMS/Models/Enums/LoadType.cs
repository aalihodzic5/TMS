using System.ComponentModel.DataAnnotations;

namespace TMS.Models.Enums
{
    public enum LoadType
    {
        [Display(Name = "FULL")]
        FULL,
        [Display(Name = "PARTIAL")]
        PARTIAL
    }
}
