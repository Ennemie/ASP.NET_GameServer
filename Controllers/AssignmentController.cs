using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Minecraft.Data;
using Minecraft.Models;

namespace Minecraft.Controllers
{
    public class AssignmentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AssignmentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Action hiển thị giao diện chính
        public IActionResult Index()
        {
            return View();
        }

        // ==========================================
        // 1. Lấy thông tin tất cả các loại tài nguyên (Items)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> GetAllResources()
        {
            var items = await _context.Items
                .Select(i => new { i.ItemID, i.ItemName, i.ItemType, i.ImageURL, i.Price })
                .ToListAsync();
            var vehicles = await _context.Vehicles
                .Select(v => new { v.VehicleID, v.VehicleName, v.VehicleType, v.ImageURL, v.Price })
                .ToListAsync();
            var result = new
            {
                Items = items,
                Vehicles = vehicles
            };
            return Json(result);
        }

        // ==========================================
        // 2. Lấy người chơi theo chế độ chơi
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> GetPlayersByMode(string modeName)
        {
            if (string.IsNullOrEmpty(modeName)) return BadRequest("Vui lòng nhập tên chế độ.");

            var players = await _context.Players
                .Include(p => p.GameMode)
                .Where(p => p.GameMode.ModeName.Contains(modeName)) // Tìm kiếm gần đúng
                .Select(p => new { p.PlayerID, p.CharacterName, p.Email, Mode = p.GameMode.ModeName, p.ExperiencePoints, p.WalletBalance })
                .ToListAsync();
            return Json(players);
        }

        // ==========================================
        // 3. Lấy Items hoac Vehicle có giá > 100
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> GetHighValueItemsVehicles()
        {
            var items = await _context.Items
                .Where(i => i.Price > 100)
                .Select(i => new { i.ItemID, i.ItemName, i.ItemType, i.ImageURL, i.Price })
                .ToListAsync();
            var vehicles = await _context.Vehicles
                .Where(v => v.Price > 100)
                .Select(v => new { v.VehicleID, v.VehicleName, v.VehicleType, v.ImageURL, v.Price })
                .ToListAsync();
            var results = new
            {
                Items = items,
                Vehicles = vehicles
            };
            return Json(results);
        }

        // ==========================================
        // 4. Lấy item người chơi có thể mua
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> GetAffordableItems(int playerId)
        {
            var player = await _context.Players.FindAsync(playerId);
            if (player == null) return NotFound("Không tìm thấy người chơi.");

            var items = await _context.Items
                .Where(i => i.Price <= player.WalletBalance)
                .Select(i => new { i.ItemID, i.ItemName, i.ItemType, i.Price, PlayerXP = player.WalletBalance })
                .ToListAsync();
            return Json(items);
        }

        // ==========================================
        // 5. Lấy item chứa từ 'Diamond' và giá < 500
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> GetCheapDiamondItems()
        {
            var items = await _context.Items
                .Where(i => i.ItemName.Contains("Diamond") && i.Price < 500)
                .ToListAsync();
            return Json(items);
        }

        // ==========================================
        // 6. Lịch sử giao dịch của người chơi (sắp xếp thời gian)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> GetPlayerHistory(int playerId)
        {
            var history = await _context.Purchases
                .Where(p => p.PlayerID == playerId)
                .OrderByDescending(p => p.PurchaseDate) // Mới nhất lên đầu
                .Select(p => new {
                    p.PurchaseID,
                    ItemOrVehicle = p.ItemID.HasValue ? p.Item.ItemName : p.Vehicle.VehicleName,
                    Type = p.ItemID.HasValue ? "Vật phẩm" : "Phương tiện",
                    p.Amount,
                    Date = p.PurchaseDate.ToString("dd/MM/yyyy HH:mm")
                })
                .ToListAsync();
            return Json(history);
        }

        // ==========================================
        // 7. Thêm item mới
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> CreateItem([FromBody] Item newItem)
        {
            // --- BƯỚC QUAN TRỌNG: Bỏ qua kiểm tra danh sách Purchases ---
            ModelState.Remove("Purchases");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Items.Add(newItem);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Thêm vật phẩm thành công!", data = newItem });
                }
                catch (Exception ex)
                {
                    return BadRequest(new { success = false, message = "Lỗi lưu database: " + ex.Message });
                }
            }

            // Trả về lỗi chi tiết để bạn biết chính xác trường nào bị sai
            var errors = string.Join("; ", ModelState.Values
                                            .SelectMany(v => v.Errors)
                                            .Select(e => e.ErrorMessage));

            return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ: " + errors });
        }

        // ==========================================
        // 8. Cập nhật mật khẩu
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> UpdatePassword(int playerId, string newPassword)
        {
            var player = await _context.Players.FindAsync(playerId);
            if (player == null) return NotFound("Người chơi không tồn tại.");

            if (string.IsNullOrWhiteSpace(newPassword)) return BadRequest("Mật khẩu không được để trống.");
            if(newPassword == player.Password) 
                return BadRequest("Mật khẩu mới phải khác mật khẩu hiện tại.");

            player.Password = newPassword; // Lưu ý: Thực tế nên Hash mật khẩu
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = $"Đã cập nhật mật khẩu cho {player.CharacterName}" });
        }

        // ==========================================
        // 9. Top item được mua nhiều nhất
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> GetTopSellingItems()
        {
            var topItems = await _context.Purchases
                .Where(p => p.ItemID.HasValue)
                .GroupBy(p => p.Item)
                .Select(g => new {
                    ItemName = g.Key.ItemName,
                    TotalSold = g.Sum(p => p.Amount)
                })
                .ToListAsync();

            var topVehicles = await _context.Purchases
                .Where(p => p.VehicleID.HasValue)
                .GroupBy(p => p.Vehicle)
                .Select(g => new {
                    ItemName = g.Key.VehicleName,
                    TotalSold = g.Sum(p => p.Amount)
                })
                .ToListAsync();

            var topItemsCombined = topItems.Concat(topVehicles).OrderByDescending(x => x.TotalSold).Take(5);
            return Json(topItemsCombined);
        }

        // ==========================================
        // 10. Danh sách người chơi và số lần mua hàng
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> GetPlayerPurchaseStats()
        {
            var stats = await _context.Players
                .Select(p => new {
                    p.CharacterName,
                    p.Email,
                    PurchaseCount = p.Purchases.Count()
                })
                .OrderByDescending(x => x.PurchaseCount)
                .ToListAsync();
            return Json(stats);
        }
    }
}