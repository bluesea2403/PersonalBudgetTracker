using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BudgetTracker.Data;
using BudgetTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

[Authorize]
public class BudgetsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public BudgetsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: Budgets - Sửa lại để hiển thị ngân sách của năm hiện tại
    public async Task<IActionResult> Index()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        var currentYear = DateTime.Now.Year;

        var budgets = _context.Budgets
            .Include(b => b.Category)
            .Where(b => b.UserId == currentUser.Id && b.Year == currentYear) // <-- Chỉ lọc theo NĂM
            .OrderBy(b => b.Month); // <-- Sắp xếp theo tháng để dễ nhìn

        ViewData["CurrentYear"] = currentYear; // Gửi năm hiện tại sang View để hiển thị
        return View(await budgets.ToListAsync());
    }

    // GET: Budgets/Create
    public async Task<IActionResult> Create()
    {
        var currentUser = await _userManager.GetUserAsync(User);

        // SỬA LẠI CÂU TRUY VẤN Ở ĐÂY
        // Nó sẽ lấy các danh mục có UserId là của bạn HOẶC có UserId là null (danh mục chung)
        ViewData["CategoryId"] = new SelectList(
            await _context.Categories
                          .Where(c => (c.UserId == currentUser.Id || c.UserId == null) && c.Type == StaticDetails.Expense)
                          .ToListAsync(),
            "Id",
            "Name");

        // Phần tạo danh sách Tháng và Năm giữ nguyên
        ViewBag.MonthList = new SelectList(Enumerable.Range(1, 12).Select(i => new { Value = i, Text = $"Tháng {i}" }), "Value", "Text", DateTime.Now.Month);
        int currentYear = DateTime.Now.Year;
        ViewBag.YearList = new SelectList(Enumerable.Range(currentYear - 2, 5).Select(i => new { Value = i, Text = i.ToString() }), "Value", "Text", currentYear);

        return View();
    }

    // POST: Budgets/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Amount,CategoryId,Month,Year")] Budget budget)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        budget.UserId = currentUser.Id;
        ModelState.Remove("User");
        ModelState.Remove("Category");
        ModelState.Remove("UserId");
        // Giờ chúng ta cần UserId, Month, Year từ form nên không Remove chúng nữa

        if (ModelState.IsValid)
        {
            // Kiểm tra xem đã có ngân sách cho danh mục này trong tháng/năm đó chưa
            bool budgetExists = await _context.Budgets.AnyAsync(b =>
                b.UserId == currentUser.Id &&
                b.CategoryId == budget.CategoryId &&
                b.Month == budget.Month &&
                b.Year == budget.Year);

            if (budgetExists)
            {
                ModelState.AddModelError("CategoryId", "Bạn đã đặt ngân sách cho danh mục này trong tháng được chọn.");
            }
            else
            {
                _context.Add(budget);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
        }
        var existingBudgetCategoryIds = await _context.Budgets
            .Where(b => b.UserId == currentUser.Id && b.Month == budget.Month && b.Year == budget.Year)
            .Select(b => b.CategoryId)
            .ToListAsync();

        // Nếu có lỗi, tải lại danh sách và trả về View
        var availableCategories = await _context.Categories
            .Where(c => (c.UserId == currentUser.Id || c.UserId == null) && c.Type == StaticDetails.Expense && !existingBudgetCategoryIds.Contains(c.Id))
            .ToListAsync(); ViewBag.MonthList = new SelectList(Enumerable.Range(1, 12).Select(i => new { Value = i, Text = $"Tháng {i}" }), "Value", "Text", budget.Month);
        int currentYear = DateTime.Now.Year;
        ViewBag.YearList = new SelectList(Enumerable.Range(currentYear - 2, 5).Select(i => new { Value = i, Text = i.ToString() }), "Value", "Text", budget.Year);
        return View(budget);
    }

    // GET: Budgets/Edit/5
    public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var budget = await _context.Budgets.FindAsync(id);
            if (budget == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", budget.CategoryId);
            return View(budget);
        }

    // POST: Budgets/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Month,Year,Amount,CategoryId")] Budget budget)
    {
        if (id != budget.Id)
        {
            return NotFound();
        }

        // Lấy thông tin người dùng hiện tại để gán lại UserId, đảm bảo người dùng không thể sửa ngân sách của người khác
        var currentUser = await _userManager.GetUserAsync(User);

        // Tìm ngân sách gốc trong DB để kiểm tra quyền sở hữu
        var budgetInDb = await _context.Budgets.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id && b.UserId == currentUser.Id);
        if (budgetInDb == null)
        {
            return NotFound(); // Không tìm thấy hoặc không có quyền
        }

        // Gán lại UserId từ đối tượng gốc trong DB để đảm bảo an toàn
        budget.UserId = budgetInDb.UserId;

        // Bỏ qua kiểm tra lỗi cho các thuộc tính không được bind từ form
        ModelState.Remove("User");
        ModelState.Remove("Category");
        ModelState.Remove("UserId");

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(budget);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BudgetExists(budget.Id))
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

        // Nếu không hợp lệ, tải lại dropdown và trả về view
        ViewData["CategoryId"] = new SelectList(await _context.Categories.Where(c => c.UserId == currentUser.Id).ToListAsync(), "Id", "Name", budget.CategoryId);
        return View(budget);
    }

    // GET: Budgets/Delete/5
    public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var budget = await _context.Budgets
                .Include(b => b.Category)
                .Include(b => b.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (budget == null)
            {
                return NotFound();
            }

            return View(budget);
        }

        // POST: Budgets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var budget = await _context.Budgets.FindAsync(id);
            if (budget != null)
            {
                _context.Budgets.Remove(budget);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BudgetExists(int id)
        {
            return _context.Budgets.Any(e => e.Id == id);
        }
    }
