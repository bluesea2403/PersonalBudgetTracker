using Microsoft.AspNetCore.Mvc;
using BudgetTracker.Data;
using BudgetTracker.Models;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Linq;
using System;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class DashboardApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public DashboardApiController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet("ExpenseSummaryForChart")]
    public async Task<IActionResult> GetExpenseSummaryForChart()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        var startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        var expenseTransactions = await _context.Transactions
            .Where(t => t.UserId == currentUser.Id &&
                        t.Date >= startDate && t.Date <= endDate &&
                        t.Category.Type == StaticDetails.Expense)
            .Include(t => t.Category)
            .ToListAsync();

        // Tính toán tổng chi tiêu để làm mẫu số
        var totalExpense = expenseTransactions.Sum(t => t.Amount);

        // Nhóm và tính toán
        var summary = expenseTransactions
            .GroupBy(t => t.Category.Name)
            .Select(g => new
            {
                Category = g.Key,
                Amount = g.Sum(t => t.Amount)
            })
            .OrderByDescending(x => x.Amount)
            .ToList();

        // Tạo các mảng dữ liệu để trả về
        var labels = summary.Select(s => s.Category).ToArray();
        var data = summary.Select(s => s.Amount).ToArray();

        // --- LOGIC MỚI: TÍNH TOÁN PHẦN TRĂM ---
        var percentages = summary
            .Select(s => (totalExpense > 0) ? Math.Round((s.Amount / totalExpense) * 100, 2) : 0)
            .ToArray();

        // Trả về đối tượng JSON có thêm cả mảng percentages
        return Ok(new { labels, data, percentages });
    }
}