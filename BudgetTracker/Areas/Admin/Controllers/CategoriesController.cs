using BudgetTracker.Data;
using BudgetTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BudgetTracker.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Categories
        public async Task<IActionResult> Index()
        {
            // Chỉ hiển thị danh mục chung (UserId is null)
            var adminCategories = await _context.Categories.Where(c => c.UserId == null).ToListAsync();
            return View(adminCategories);
        }

        // GET: Admin/Categories/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.TypeList = GetTypeList();
            ViewBag.Icons = GetFontAwesomeIcons();
            ViewBag.CategoryGroups = new SelectList(await _context.CategoryGroups.OrderBy(g => g.Name).ToListAsync(), "Name", "Name");

            return View(new Category());
        }

        // POST: Admin/Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,GroupName,Type,Icon")] Category category)
        {
            category.UserId = null; // Đánh dấu là danh mục chung
            ModelState.Remove("User");
            ModelState.Remove("UserId");

            if (ModelState.IsValid)
            {
                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.TypeList = GetTypeList();
            ViewBag.Icons = GetFontAwesomeIcons();
            ViewBag.CategoryGroups = new SelectList(await _context.CategoryGroups.OrderBy(g => g.Name).ToListAsync(), "Name", "Name", category.GroupName);
            return View(category);
        }

        // --- CÁC ACTION MỚI ĐƯỢC THÊM VÀO ---

        // GET: Admin/Categories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id && c.UserId == null);
            if (category == null) return NotFound();

            ViewBag.TypeList = GetTypeList();
            ViewBag.Icons = GetFontAwesomeIcons();
            ViewBag.CategoryGroups = new SelectList(await _context.CategoryGroups.OrderBy(g => g.Name).ToListAsync(), "Name", "Name");
            return View(category);
        }

        // POST: Admin/Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Name,GroupName,Type,Icon")] Category category)
        {
            if (id != category.Id) return NotFound();

            category.UserId = null; // Đảm bảo UserId luôn là null
            ModelState.Remove("User");
            ModelState.Remove("UserId");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Categories.Any(e => e.Id == category.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.TypeList = GetTypeList();
            ViewBag.Icons = GetFontAwesomeIcons();
            ViewBag.CategoryGroups = new SelectList(await _context.CategoryGroups.OrderBy(g => g.Name).ToListAsync(), "Name", "Name", category.GroupName);

            return View(category);
        }

        // GET: Admin/Categories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.Categories.FirstOrDefaultAsync(m => m.Id == id && m.UserId == null);
            if (category == null) return NotFound();

            return View(category);
        }

        // POST: Admin/Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null && category.UserId == null)
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Categories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Chỉ tìm trong các danh mục chung (do Admin tạo)
            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == null);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }
        // Các hàm trợ giúp
        private List<SelectListItem> GetTypeList()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Text = StaticDetails.Expense, Value = StaticDetails.Expense },
                new SelectListItem { Text = StaticDetails.Income, Value = StaticDetails.Income }
            };
        }

        private List<string> GetFontAwesomeIcons()
        {
            return new List<string>
            {
                "fas fa-shopping-cart", "fas fa-bus", "fas fa-home",
                "fas fa-plane", "fas fa-file-invoice-dollar", "fas fa-gift", "fas fa-briefcase-medical",
                "fas fa-graduation-cap", "fas fa-paw", "fas fa-film", "fas fa-futbol",
                "fas fa-money-bill-wave", "fas fa-wallet", "fas fa-piggy-bank", "fas fa-chart-line",
                // --- Chi tiêu Thiết yếu ---
        "fas fa-utensils",          // Ăn uống
        "fas fa-shopping-basket",   // Đi chợ, Tạp hóa
        "fas fa-home",              // Nhà ở, Tiền nhà
        "fas fa-bolt",              // Hóa đơn (Điện)
        "fas fa-burn",              // Hóa đơn (Gas)
        "fas fa-wifi",              // Hóa đơn (Internet)
        "fas fa-phone-alt",         // Hóa đơn (Điện thoại)
        "fas fa-bus-alt",           // Di chuyển (Công cộng)
        "fas fa-gas-pump",          // Di chuyển (Xăng xe)
        "fas fa-car",               // Di chuyển (Bảo trì xe)

        // --- Cá nhân & Gia đình ---
        "fas fa-tshirt",            // Quần áo
        "fas fa-heartbeat",         // Sức khỏe
        "fas fa-pills",             // Thuốc men
        "fas fa-child",             // Con cái
        "fas fa-user-friends",      // Gia đình, Người thân
        "fas fa-paw",               // Thú cưng
        "fas fa-gift",              // Quà tặng & Từ thiện

        // --- Hưởng thụ & Giải trí ---
        "fas fa-film",              // Xem phim, Giải trí
        "fas fa-book-open",         // Sách, Đọc truyện
        "fas fa-gamepad",           // Game
        "fas fa-music",             // Âm nhạc
        "fas fa-plane-departure",   // Du lịch
        "fas fa-cocktail",          // Ăn ngoài, Cafe

        // --- Phát triển & Đầu tư ---
        "fas fa-graduation-cap",    // Giáo dục
        "fas fa-chart-line",        // Đầu tư
        "fas fa-piggy-bank",        // Tiết kiệm
        "fas fa-building",          // Bất động sản

        // --- Thu nhập ---
        "fas fa-money-bill-wave",   // Lương
        "fas fa-briefcase",         // Kinh doanh
        "fas fa-star",              // Tiền thưởng
        "fas fa-hand-holding-usd",  // Thu nhập khác
        "fas fa-wallet"             // Ví tiền chung
            };
        }
    }
}