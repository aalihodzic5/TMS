using System.ComponentModel.DataAnnotations;

namespace TMS.Models.Enums
{
    public enum Licence
    {
        [Display(Name = "ADR")]
        ADR,
        [Display(Name = "ATP")]
        ATP,
        [Display(Name = "SPECIAL_TRANSPORT")]
        SPECIAL_TRANSPORT,
        [Display(Name = "TANKER")]
        TANKER
    }
}
