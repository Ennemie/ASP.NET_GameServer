using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Minecraft.Data;
using Minecraft.Models;

namespace Minecraft.Controllers
{
    public class ItemManagerController : Controller
    {
        private readonly ILogger<ItemManagerController> _logger;
        private readonly ApplicationDbContext _context;

        public ItemManagerController(ILogger<ItemManagerController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // 1. INDEX
        public async Task<IActionResult> Index()
        {
            var items = await _context.Items.ToListAsync();
            return View(items);
        }

        // 2. CREATE (GET)
        public IActionResult Create()
        {
            return View();
        }

        // 3. CREATE (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ItemName,ItemType,ImageURL,Price")] Item item)
        {
            // --- FIX QUAN TRỌNG: Bỏ qua kiểm tra danh sách liên kết ---
            ModelState.Remove("Purchases");

            // Trim dữ liệu đầu vào
            item.ItemName = item.ItemName?.Trim();
            item.ItemType = item.ItemType?.Trim();
            item.ImageURL = item.ImageURL?.Trim();

            // Kiểm tra thủ công
            if (string.IsNullOrWhiteSpace(item.ItemName))
                ModelState.AddModelError("ItemName", "Tên vật phẩm không được để trống!");

            if (item.Price < 0)
                ModelState.AddModelError("Price", "Giá không được âm!");

            // Kiểm tra trùng tên
            if (await _context.Items.AnyAsync(i => i.ItemName == item.ItemName))
            {
                ModelState.AddModelError("ItemName", "Tên vật phẩm này đã tồn tại!");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(item);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"✅ Tạo thành công vật phẩm: {item.ItemName}!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Lỗi khi tạo vật phẩm: {ex.Message}");
                    ModelState.AddModelError("", "Lỗi lưu dữ liệu: " + ex.Message);
                }
            }

            return View(item);
        }

        // 4. EDIT (GET)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var item = await _context.Items.FindAsync(id);
            if (item == null) return NotFound();

            return View(item);
        }

        // 5. EDIT (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ItemID,ItemName,ItemType,ImageURL,Price")] Item item)
        {
            if (id != item.ItemID) return NotFound();

            // --- FIX QUAN TRỌNG: Bỏ qua kiểm tra danh sách liên kết ---
            ModelState.Remove("Purchases");

            // Trim dữ liệu
            item.ItemName = item.ItemName?.Trim();
            item.ItemType = item.ItemType?.Trim();
            item.ImageURL = item.ImageURL?.Trim();

            if (ModelState.IsValid)
            {
                // Kiểm tra trùng tên (trừ chính nó ra)
                if (await _context.Items.AnyAsync(i => i.ItemName == item.ItemName && i.ItemID != id))
                {
                    ModelState.AddModelError("ItemName", "Tên vật phẩm này đã được sử dụng!");
                    return View(item);
                }

                try
                {
                    _context.Update(item);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "✅ Cập nhật vật phẩm thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ItemExists(item.ItemID)) return NotFound();
                    else throw;
                }
            }
            return View(item);
        }

        // 6. DETAILS
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var item = await _context.Items
                .Include(i => i.Purchases)
                .FirstOrDefaultAsync(m => m.ItemID == id);

            if (item == null) return NotFound();

            return View(item);
        }

        // 7. DELETE (GET)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var item = await _context.Items
                .FirstOrDefaultAsync(m => m.ItemID == id);

            if (item == null) return NotFound();

            return View(item);
        }

        // 8. DELETE (POST)
        // Lưu ý: Tên tham số là ItemID để khớp với input hidden trong View Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int ItemID)
        {
            var item = await _context.Items.FindAsync(ItemID);
            if (item != null)
            {
                // Xóa dữ liệu liên quan (Lịch sử mua hàng của item này)
                var purchases = _context.Purchases.Where(p => p.ItemID == ItemID);
                _context.Purchases.RemoveRange(purchases);

                // Xóa item
                _context.Items.Remove(item);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "🗑️ Đã xóa vật phẩm thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "❌ Không tìm thấy vật phẩm để xóa!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ItemExists(int id)
        {
            return _context.Items.Any(e => e.ItemID == id);
        }
    }
}