using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Minecraft.Models
{
public class MonsterKill
    {
        [Key]
        public int KillID { get; set; }

        [ForeignKey("PlayerID")]
        public int PlayerID { get; set; }
        public Player Player { get; set; }

        [ForeignKey("MonsterID")]
        public int MonsterID { get; set; }
        public Monster Monster { get; set; } 

        public int RewardExp { get; set; }

        public DateTime KillTime { get; set; }
    }
}
