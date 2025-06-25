using BudgetTracker.Data;
using BudgetTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BudgetTracker.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CategoryGroupsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoryGroupsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/CategoryGroups
        public async Task<IActionResult> Index()
        {
            return View(await _context.CategoryGroups.ToListAsync());
        }

        // GET: Admin/CategoryGroups/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categoryGroup = await _context.CategoryGroups
                .FirstOrDefaultAsync(m => m.Id == id);
            if (categoryGroup == null)
            {
                return NotFound();
            }

            return View(categoryGroup);
        }

        // GET: Admin/CategoryGroups/Create
        public IActionResult Create()
        {
            ViewBag.Icons = GetFontAwesomeIcons();
            return View();
        }

        // POST: Admin/CategoryGroups/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Icon")] CategoryGroup categoryGroup)
        {
            if (ModelState.IsValid)
            {
                _context.Add(categoryGroup);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(categoryGroup);
        }

        // GET: Admin/CategoryGroups/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categoryGroup = await _context.CategoryGroups.FindAsync(id);
            if (categoryGroup == null)
            {
                return NotFound();
            }
            ViewBag.Icons = GetFontAwesomeIcons();
            return View(categoryGroup);
        }

        // POST: Admin/CategoryGroups/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Icon")] CategoryGroup categoryGroup)
        {
            if (id != categoryGroup.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(categoryGroup);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryGroupExists(categoryGroup.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(categoryGroup);
        }

        // GET: Admin/CategoryGroups/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categoryGroup = await _context.CategoryGroups
                .FirstOrDefaultAsync(m => m.Id == id);
            if (categoryGroup == null)
            {
                return NotFound();
            }

            return View(categoryGroup);
        }

        // POST: Admin/CategoryGroups/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var categoryGroup = await _context.CategoryGroups.FindAsync(id);
            if (categoryGroup != null)
            {
                _context.CategoryGroups.Remove(categoryGroup);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryGroupExists(int id)
        {
            return _context.CategoryGroups.Any(e => e.Id == id);
        }
        private List<string> GetFontAwesomeIcons()
        {
            return new List<string>
    {
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
