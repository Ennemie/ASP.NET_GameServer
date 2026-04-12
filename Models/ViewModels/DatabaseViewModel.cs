namespace Minecraft.Models.ViewModels
{
    public class DatabaseViewModel
    {
        public List<Player> Players { get; set; }
        public List<Item> Items { get; set; }
        public List<Vehicle> Vehicles { get; set; }
        public List<GameMode> GameModes { get; set; }
        public List<Quest> Quests { get; set; }
        public List<Purchase> Purchases { get; set; }
        public List<MonsterKill> MonsterKills { get; set; }
        public List<PlayerQuest> PlayerQuests { get; set; }
        public List<Monster> Monsters { get; set; }
    }
}
