namespace Minecraft.Models.CreateRequest
{
    public class CreateMonsterRequest
    {
        public int MonsterID { get; set; }
        public string MonsterName { get; set; }
        public int Health { get; set; }
        public int AttackDamage { get; set; }
    }
}
