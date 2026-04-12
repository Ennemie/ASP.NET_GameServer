using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Minecraft.Data;
using Minecraft.Models;

namespace Minecraft.Controllers
{
    public class PlayerManagerController : Controller
    {
        private readonly ILogger<PlayerManagerController> _logger;
        private readonly ApplicationDbContext _context;

        public PlayerManagerController(ILogger<PlayerManagerController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // 1. INDEX
        public async Task<IActionResult> Index()
        {
            var players = await _context.Players.Include(p => p.GameMode).ToListAsync();
            return View(players);
        }

        // 2. CREATE (GET) - Hiển thị form
        public async Task<IActionResult> Create()
        {
            ViewData["GameModes"] = await _context.GameModes.ToListAsync();
            return View();
        }

        // 3. CREATE (POST) - Xử lý thêm mới
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Email,Password,CharacterName,ModeID,ExperiencePoints,WalletBalance")] Player player)
        {
            // --- FIX QUAN TRỌNG: Bỏ qua kiểm tra các bảng liên kết ---
            ModelState.Remove("GameMode");
            ModelState.Remove("Purchases");
            ModelState.Remove("MonsterKills");
            ModelState.Remove("PlayerQuests");

            // Kiểm tra thủ công
            if (player.ModeID <= 0) ModelState.AddModelError("ModeID", "Vui lòng chọn Chế Độ Chơi!");

            // Trim dữ liệu
            player.CharacterName = player.CharacterName?.Trim();
            player.Email = player.Email?.Trim();

            // Kiểm tra Email trùng
            if (await _context.Players.AnyAsync(p => p.Email == player.Email))
            {
                ModelState.AddModelError("Email", "Email này đã tồn tại!");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(player);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"✅ Tạo thành công người chơi {player.CharacterName}!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi lưu dữ liệu: " + ex.Message);
                }
            }

            // Nếu lỗi thì nạp lại GameModes để hiện lại form
            ViewData["GameModes"] = await _context.GameModes.ToListAsync();
            return View(player);
        }

        // 4. EDIT (GET) - Hiển thị form sửa
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var player = await _context.Players.FindAsync(id);
            if (player == null) return NotFound();

            ViewData["GameModes"] = await _context.GameModes.ToListAsync();
            return View(player);
        }

        // 5. EDIT (POST) - Xử lý cập nhật
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PlayerID,Email,Password,CharacterName,ModeID,ExperiencePoints,WalletBalance")] Player player)
        {
            if (id != player.PlayerID) return NotFound();

            // --- FIX QUAN TRỌNG: Bỏ qua kiểm tra các bảng liên kết ---
            ModelState.Remove("GameMode");
            ModelState.Remove("Purchases");
            ModelState.Remove("MonsterKills");
            ModelState.Remove("PlayerQuests");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(player);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "✅ Cập nhật thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PlayerExists(player.PlayerID)) return NotFound();
                    else throw;
                }
            }

            ViewData["GameModes"] = await _context.GameModes.ToListAsync();
            return View(player);
        }

        // 6. DETAILS
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var player = await _context.Players
                .Include(p => p.GameMode)
                .Include(p => p.Purchases)
                .Include(p => p.MonsterKills)
                .Include(p => p.PlayerQuests)
                .FirstOrDefaultAsync(m => m.PlayerID == id);

            if (player == null) return NotFound();

            return View(player);
        }

        // 7. DELETE (GET) - Trang xác nhận xóa
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var player = await _context.Players
                .Include(p => p.GameMode)
                .FirstOrDefaultAsync(m => m.PlayerID == id);

            if (player == null) return NotFound();

            return View(player);
        }

        // 8. DELETE (POST) - Thực hiện xóa
        // Lưu ý: Đã bỏ [ActionName("Delete")] để khớp với asp-action="DeleteConfirmed" ở View
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int PlayerID) // Đổi tên tham số trùng với asp-for="PlayerID"
        {
            var player = await _context.Players.FindAsync(PlayerID);
            if (player != null)
            {
                // Xóa dữ liệu liên quan thủ công nếu chưa set Cascade Delete trong DB
                var purchases = _context.Purchases.Where(x => x.PlayerID == PlayerID);
                _context.Purchases.RemoveRange(purchases);

                var kills = _context.MonsterKills.Where(x => x.PlayerID == PlayerID);
                _context.MonsterKills.RemoveRange(kills);

                var quests = _context.PlayerQuests.Where(x => x.PlayerID == PlayerID);
                _context.PlayerQuests.RemoveRange(quests);

                // Xóa người chơi
                _context.Players.Remove(player);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "🗑️ Đã xóa người chơi thành công!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool PlayerExists(int id)
        {
            return _context.Players.Any(e => e.PlayerID == id);
        }
    }
}