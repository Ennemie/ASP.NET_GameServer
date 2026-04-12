using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Minecraft.Data;
using Minecraft.Models;

namespace Minecraft.Controllers
{
    public class VehicleManagerController : Controller
    {
        private readonly ILogger<VehicleManagerController> _logger;
        private readonly ApplicationDbContext _context;

        public VehicleManagerController(ILogger<VehicleManagerController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // 1. INDEX - Danh sách phương tiện
        public async Task<IActionResult> Index()
        {
            try
            {
                var vehicles = await _context.Vehicles.ToListAsync();
                return View(vehicles);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi tải danh sách phương tiện: {ex.Message}");
                return View(new List<Vehicle>());
            }
        }

        // 2. CREATE (GET) - Hiển thị form thêm mới
        public IActionResult Create()
        {
            return View();
        }

        // 3. CREATE (POST) - Xử lý thêm mới
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("VehicleName,VehicleType,ImageURL,Price")] Vehicle vehicle)
        {
            // --- FIX QUAN TRỌNG: Bỏ qua kiểm tra danh sách liên kết ---
            ModelState.Remove("Purchases");

            // Trim khoảng trắng thừa
            vehicle.VehicleName = vehicle.VehicleName?.Trim();
            vehicle.VehicleType = vehicle.VehicleType?.Trim();
            vehicle.ImageURL = vehicle.ImageURL?.Trim();

            // Validate thủ công các trường hợp đặc biệt
            if (string.IsNullOrWhiteSpace(vehicle.VehicleName))
                ModelState.AddModelError("VehicleName", "Tên phương tiện không được để trống!");

            if (vehicle.Price < 0)
                ModelState.AddModelError("Price", "Giá không được âm!");

            // Kiểm tra trùng tên phương tiện
            if (await _context.Vehicles.AnyAsync(v => v.VehicleName == vehicle.VehicleName))
            {
                ModelState.AddModelError("VehicleName", "Tên phương tiện này đã tồn tại!");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(vehicle);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"Tạo phương tiện thành công: {vehicle.VehicleName}");
                    TempData["SuccessMessage"] = $"✅ Đã thêm phương tiện: {vehicle.VehicleName}!";

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Lỗi khi lưu phương tiện: {ex.Message}");
                    ModelState.AddModelError("", "Lỗi hệ thống khi lưu dữ liệu.");
                }
            }

            return View(vehicle);
        }

        // 4. EDIT (GET) - Hiển thị form sửa
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null) return NotFound();

            return View(vehicle);
        }

        // 5. EDIT (POST) - Xử lý cập nhật
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("VehicleID,VehicleName,VehicleType,ImageURL,Price")] Vehicle vehicle)
        {
            if (id != vehicle.VehicleID) return NotFound();

            // --- FIX QUAN TRỌNG: Bỏ qua kiểm tra danh sách liên kết ---
            ModelState.Remove("Purchases");

            // Trim dữ liệu
            vehicle.VehicleName = vehicle.VehicleName?.Trim();
            vehicle.VehicleType = vehicle.VehicleType?.Trim();
            vehicle.ImageURL = vehicle.ImageURL?.Trim();

            if (ModelState.IsValid)
            {
                // Kiểm tra trùng tên (ngoại trừ chính nó)
                var exists = await _context.Vehicles.AnyAsync(v => v.VehicleName == vehicle.VehicleName && v.VehicleID != id);
                if (exists)
                {
                    ModelState.AddModelError("VehicleName", "Tên phương tiện này đã được sử dụng!");
                    return View(vehicle);
                }

                try
                {
                    _context.Update(vehicle);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "✅ Cập nhật phương tiện thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VehicleExists(vehicle.VehicleID)) return NotFound();
                    else throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Lỗi cập nhật: {ex.Message}");
                    ModelState.AddModelError("", "Lỗi khi cập nhật dữ liệu.");
                }
            }
            return View(vehicle);
        }

        // 6. DETAILS
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var vehicle = await _context.Vehicles
                .Include(v => v.Purchases)
                .FirstOrDefaultAsync(m => m.VehicleID == id);

            if (vehicle == null) return NotFound();

            return View(vehicle);
        }

        // 7. DELETE (GET) - Trang xác nhận xóa
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var vehicle = await _context.Vehicles
                .FirstOrDefaultAsync(m => m.VehicleID == id);

            if (vehicle == null) return NotFound();

            return View(vehicle);
        }

        // 8. DELETE (POST) - Thực hiện xóa
        // Lưu ý: Tên tham số là VehicleID để khớp với input hidden trong View Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int VehicleID)
        {
            var vehicle = await _context.Vehicles.FindAsync(VehicleID);
            if (vehicle != null)
            {
                try
                {
                    // Xóa dữ liệu liên quan (Lịch sử mua hàng của phương tiện này)
                    // Bước này cần thiết nếu Database không thiết lập Cascade Delete
                    var purchases = _context.Purchases.Where(p => p.VehicleID == VehicleID);
                    _context.Purchases.RemoveRange(purchases);

                    // Xóa phương tiện
                    _context.Vehicles.Remove(vehicle);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "🗑️ Đã xóa phương tiện thành công!";
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Lỗi khi xóa: {ex.Message}");
                    TempData["ErrorMessage"] = "❌ Lỗi khi xóa dữ liệu (có thể do ràng buộc khóa ngoại).";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "❌ Không tìm thấy phương tiện để xóa!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool VehicleExists(int id)
        {
            return _context.Vehicles.Any(e => e.VehicleID == id);
        }
    }
}