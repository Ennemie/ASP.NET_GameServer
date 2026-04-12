using System.ComponentModel.DataAnnotations;

namespace Minecraft.Models
{
    public class Item
    {
        [Key]
        public int ItemID { get; set; }

        [Required]
        [MaxLength(100)]
        public string ItemName { get; set; }

        [MaxLength(100)]
        public string ItemType { get; set; }

        [MaxLength(255)]
        public string ImageURL { get; set; }

        public int Price { get; set; }

        public List<Purchase> Purchases { get; set; }
    }
}
