namespace Minecraft.Models.CreateRequest
{
    public class CreateItemRequest
    {
        public int ItemID { get; set; }
        public string ItemName { get; set; }
        public string ItemType { get; set; }
        public string ImageURL { get; set; }
        public int Price { get; set; }
    }
}
