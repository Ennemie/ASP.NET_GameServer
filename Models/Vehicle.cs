using System.ComponentModel.DataAnnotations;

namespace Minecraft.Models
{
    public class Vehicle
    {
        [Key]
        public int VehicleID { get; set; }

        [Required]
        [MaxLength(100)]
        public string VehicleName { get; set; }

        [MaxLength(100)]
        public string VehicleType { get; set; }

        [MaxLength(255)]
        public string ImageURL { get; set; }

        public int Price { get; set; }

        public List<Purchase> Purchases { get; set; }
    }
}
