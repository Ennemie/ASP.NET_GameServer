using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Minecraft.Models
{
public class PlayerQuest
    {
        [Key]
        public int PlayerQuestID { get; set; }

        [ForeignKey("PlayerID")]
        public int PlayerID { get; set; }
        public Player Player { get; set; }

        [ForeignKey("QuestID")]
        public int QuestID { get; set; }
        public Quest Quest { get; set; }

        [MaxLength(50)]
        public string Status { get; set; }

    }
}
