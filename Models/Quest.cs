using System.ComponentModel.DataAnnotations;

namespace Minecraft.Models
{
public class Quest
    {
        [Key]
        public int QuestID { get; set; }

        [Required]
        [MaxLength(150)]
        public string QuestName { get; set; }

        public string Description { get; set; }

        public int RewardExp { get; set; }

        public List<PlayerQuest> PlayerQuests { get; set; }
    }
}
