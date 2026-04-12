namespace Minecraft.Models.CreateRequest
{
 public class CreatePurchaseRequest
    {
        public class PurchaseItemDTO
        {
            // Lưu ý: Dù client gửi playerID, server nên lấy ID từ Token để bảo mật hơn
            public int playerID { get; set; }
            public int? itemID { get; set; }
            public int? vehicleID { get; set; }
            public int quantity { get; set; }
        }

        // Hứng gói tin tổng thể
        public class PurchaseRequestDTO
        {
            public int totalCost { get; set; }
            public List<PurchaseItemDTO> items { get; set; }
        }
    }
}
