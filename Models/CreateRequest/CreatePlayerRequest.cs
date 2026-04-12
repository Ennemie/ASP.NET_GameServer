namespace Minecraft.Models.CreateRequest
{
    public class CreatePlayerRequest
    {
        public int PlayerID { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string CharacterName { get; set; }
        public int ExperiencePoints { get; set; }
        public int ModeID { get; set; }
    }
}
