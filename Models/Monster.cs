using System.ComponentModel.DataAnnotations;

namespace Minecraft.Models
{
    public class Monster
    {
        [Key]
        public int MonsterID { get; set; }

        [MaxLength(100)]
        public string MonsterName { get; set; }
        public int Health { get; set; }
        public int AttackDamage { get; set; }
        public List<MonsterKill> MonsterKills { get; set; }
    }
}
