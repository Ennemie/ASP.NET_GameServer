using System.ComponentModel.DataAnnotations;

namespace Minecraft.Models
{
    public class GameMode
    {
        [Key]
        public int ModeID { get; set; }

        [Required]
        [MaxLength(100)]
        public string ModeName { get; set; }

        public string Description { get; set; }

        public List<Player> Players { get; set; }
    }
}
