using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Minecraft.Models
{
public class Purchase
    {
        [Key]
        public int PurchaseID { get; set; }

        public DateTime PurchaseDate { get; set; }

        // FK Player
        [ForeignKey("PlayerID")]
        public int PlayerID { get; set; }
        public Player Player { get; set; }

        // FK Item
        [ForeignKey("ItemID")]
        public int? ItemID { get; set; }
        public Item Item { get; set; }

        // FK Vehicle
        [ForeignKey("VehicleID")]
        public int? VehicleID { get; set; }
        public Vehicle Vehicle { get; set; }

        public int Amount { get; set; }
    }
}
