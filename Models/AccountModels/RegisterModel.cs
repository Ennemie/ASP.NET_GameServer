namespace Minecraft.Models.AccountModels
{
    public class RegisterModel
    {
        public string CharacterName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public int GameModeID { get; set; }
    }
}
