using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;


namespace TMS.Models
{
    public class User : IdentityUser
    {
        public string Ime { get; set; } = default!;
        public string Prezime { get; set; } = default!;


        public User() { }

    }
}
