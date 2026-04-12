using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Minecraft.Models
{
    public class Player
    {
        [Key]
        public int PlayerID { get; set; }

        // FK GameMode
        [ForeignKey("ModeID")]
        public int ModeID { get; set; }
        public GameMode GameMode { get; set; }

        [Required]
        [MaxLength(150)]
        public string Email { get; set; }

        [Required]
        [MaxLength(150)]
        public string Password { get; set; }

        [Required]
        [MaxLength(100)]
        public string CharacterName { get; set; }

        public int ExperiencePoints { get; set; }
        public int WalletBalance { get; set; }


        // 1 - n
        public List<Purchase> Purchases { get; set; }
        public List<PlayerQuest> PlayerQuests { get; set; }
        public List<MonsterKill> MonsterKills { get; set; }
    }
}
