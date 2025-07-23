using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace TMS.Models
{
    public class Truck
    {
        [Key]
        public int Id { get; set; }

        public string? UserID { get; set; } // ID korisnika koji je dodao kamion
        public User? User { get; set; }
        public string brand { get; set; }

        public string model { get; set; }

        public string licensePlate { get; set; }

        public string specification { get; set; }

        public DateTime nextServiceDate { get; set; }

        public DateTime registration { get; set; }


        public Truck() { }


    }
}
