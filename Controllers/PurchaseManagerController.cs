using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Minecraft.Data;
using Minecraft.Models;

namespace Minecraft.Controllers
{
    public class PurchaseManagerController : Controller
    {
        private readonly ILogger<PurchaseManagerController> _logger;
        private readonly ApplicationDbContext _context;

        public PurchaseManagerController(ILogger<PurchaseManagerController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // 1. INDEX
        public async Task<IActionResult> Index()
        {
            var purchases = await _context.Purchases
                .Include(p => p.Player)
                .Include(p => p.Item)
                .Include(p => p.Vehicle)
                .OrderByDescending(p => p.PurchaseDate)
                .ToListAsync();
            return View(purchases);
        }

        // 2. CREATE (GET)
        public async Task<IActionResult> Create()
        {
            await LoadViewData();
            return View();
        }

        // 3. CREATE (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PlayerID,ItemID,VehicleID,Amount,PurchaseDate")] Purchase purchase)
        {
            // --- FIX QUAN TRỌNG: Bỏ qua kiểm tra các object liên kết ---
            ModelState.Remove("Player");
            ModelState.Remove("Item");
            ModelState.Remove("Vehicle");

            // Logic kiểm tra: Phải chọn ít nhất Item HOẶC Vehicle
            if (purchase.ItemID == null && purchase.VehicleID == null)
            {
                ModelState.AddModelError("", "Vui lòng chọn ít nhất một Vật phẩm hoặc Phương tiện!");
            }

            if (purchase.PlayerID <= 0)
            {
                ModelState.AddModelError("PlayerID", "Vui lòng chọn Người chơi!");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Nếu ngày mua chưa chọn, lấy ngày hiện tại
                    if (purchase.PurchaseDate == default)
                    {
                        purchase.PurchaseDate = DateTime.Now;
                    }

                    _context.Add(purchase);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "✅ Tạo giao dịch thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Lỗi tạo giao dịch: {ex.Message}");
                    ModelState.AddModelError("", "Lỗi lưu dữ liệu: " + ex.Message);
                }
            }

            await LoadViewData();
            return View(purchase);
        }

        // 4. EDIT (GET)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var purchase = await _context.Purchases.FindAsync(id);
            if (purchase == null) return NotFound();

            await LoadViewData();
            return View(purchase);
        }

        // 5. EDIT (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PurchaseID,PlayerID,ItemID,VehicleID,Amount,PurchaseDate")] Purchase purchase)
        {
            if (id != purchase.PurchaseID) return NotFound();

            // --- FIX QUAN TRỌNG: Bỏ qua kiểm tra object liên kết ---
            ModelState.Remove("Player");
            ModelState.Remove("Item");
            ModelState.Remove("Vehicle");

            if (purchase.ItemID == null && purchase.VehicleID == null)
            {
                ModelState.AddModelError("", "Vui lòng chọn ít nhất một Vật phẩm hoặc Phương tiện!");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(purchase);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "✅ Cập nhật giao dịch thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PurchaseExists(purchase.PurchaseID)) return NotFound();
                    else throw;
                }
            }

            await LoadViewData();
            return View(purchase);
        }

        // 6. DETAILS
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var purchase = await _context.Purchases
                .Include(p => p.Player)
                .Include(p => p.Item)
                .Include(p => p.Vehicle)
                .FirstOrDefaultAsync(m => m.PurchaseID == id);

            if (purchase == null) return NotFound();

            return View(purchase);
        }

        // 7. DELETE (GET)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var purchase = await _context.Purchases
                .Include(p => p.Player)
                .Include(p => p.Item)
                .Include(p => p.Vehicle)
                .FirstOrDefaultAsync(m => m.PurchaseID == id);

            if (purchase == null) return NotFound();

            return View(purchase);
        }

        // 8. DELETE (POST)
        // Lưu ý: Tên tham số là PurchaseID để khớp với View Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int PurchaseID)
        {
            var purchase = await _context.Purchases.FindAsync(PurchaseID);
            if (purchase != null)
            {
                _context.Purchases.Remove(purchase);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "🗑️ Đã xóa giao dịch thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "❌ Không tìm thấy giao dịch để xóa!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool PurchaseExists(int id)
        {
            return _context.Purchases.Any(e => e.PurchaseID == id);
        }

        // Hàm hỗ trợ load dữ liệu cho Dropdown
        private async Task LoadViewData()
        {
            ViewData["Players"] = await _context.Players.ToListAsync();
            ViewData["Items"] = await _context.Items.ToListAsync();
            ViewData["Vehicles"] = await _context.Vehicles.ToListAsync();
        }
    }
}