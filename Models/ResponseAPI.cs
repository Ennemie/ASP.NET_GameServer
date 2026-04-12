namespace Minecraft.Models
{
    public class ResponseAPI
    {
        public bool isSuccess { get; set; } = true;
        public string notification { get; set; }
        public string token { get; set; }
        public string loggedInName { get; set; }
        public int playerID { get; set; }
        public int walletBalance { get; set; }
        public object data { get; set; }
    }
}
