using Microsoft.AspNetCore.Mvc;
using BudgetTracker.Data;
using BudgetTracker.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using BudgetTracker.Services; // <-- THÊM USING CHO SERVICE MỚI
[Authorize]
public class ReportController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IPdfService _pdfService;
    public ReportController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IPdfService pdfService)
    {
        _context = context;
        _userManager = userManager;
        _pdfService = pdfService;
        _pdfService = pdfService;
    }
    [HttpPost]
    public IActionResult ExportToPdf([FromBody] ReportExportModel model)
    {
        // Lấy dữ liệu và tạo lại ReportViewModel (phần này có thể được tối ưu)
        var reportData = new ReportViewModel
        {
            StartDate = model.StartDate,
            EndDate = model.EndDate,
            CategorySummaries = model.CategorySummaries,
            Transactions = model.Transactions
        };

        var pdfBytes = _pdfService.CreateReportPdf(reportData, model.ChartImageBase64);
        return File(pdfBytes, "application/pdf", $"BaoCao_{DateTime.Now:dd-MM-yyyy}.pdf");
    }

    // Action Index đã được nâng cấp để xử lý lọc theo loại giao dịch
    public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate, string transactionType = "All")
    {
        var currentUser = await _userManager.GetUserAsync(User);

        var finalStartDate = startDate ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        var finalEndDate = endDate ?? finalStartDate.AddMonths(1).AddDays(-1);

        // Lưu lại các giá trị lọc để hiển thị lại trên View
        ViewData["CurrentStartDate"] = finalStartDate.ToString("yyyy-MM-dd");
        ViewData["CurrentEndDate"] = finalEndDate.ToString("yyyy-MM-dd");
        ViewData["CurrentType"] = transactionType;

        // --- PHẦN LOGIC ĐÃ SỬA ---
        // 1. Bắt đầu câu truy vấn, chưa lấy dữ liệu vội (dùng IQueryable)
        var transactionsQuery = _context.Transactions
            .Where(t => t.UserId == currentUser.Id && t.Date >= finalStartDate && t.Date <= finalEndDate);

        // 2. Áp dụng bộ lọc loại giao dịch (nếu có)
        if (transactionType == "Thu" || transactionType == "Chi tiêu")
        {
            transactionsQuery = transactionsQuery.Where(t => t.Category.Type == transactionType);
        }

        // 3. Bây giờ mới thực thi câu truy vấn để lấy dữ liệu đã được lọc
        var transactionsInRange = await transactionsQuery
                                        .Include(t => t.Category)
                                        .OrderByDescending(t => t.Date)
                                        .ToListAsync();

        // --- CÁC PHẦN CÒN LẠI GIỮ NGUYÊN ---
        var categorySummaries = transactionsInRange
            .Where(t => t.Category.Type == StaticDetails.Expense)
            .GroupBy(t => t.Category.Name)
            .Select(group => new CategorySummary { CategoryName = group.Key, TotalAmount = group.Sum(t => t.Amount) })
            .OrderByDescending(s => s.TotalAmount)
            .ToList();
        var incomeSummaries = transactionsInRange
        .Where(t => t.Category.Type == StaticDetails.Income) // Chỉ lấy các khoản THU
        .GroupBy(t => t.Category.Name)
        .Select(group => new CategorySummary { CategoryName = group.Key, TotalAmount = group.Sum(t => t.Amount) })
        .OrderByDescending(s => s.TotalAmount)
        .ToList();
        var trendStartDate = DateTime.Now.AddMonths(-5).AddDays(-DateTime.Now.Day + 1);
        var trendEndDate = DateTime.Now;

        var trendTransactions = await _context.Transactions
            .Where(t => t.UserId == currentUser.Id && t.Date >= trendStartDate && t.Date <= trendEndDate)
            .Include(t => t.Category)
            .ToListAsync();

        var trendLabels = new List<string>();
        var trendIncomeData = new List<decimal>();
        var trendExpenseData = new List<decimal>();

        for (int i = 0; i < 6; i++)
        {
            var monthToProcess = trendStartDate.AddMonths(i);
            trendLabels.Add($"Tháng {monthToProcess.Month}/{monthToProcess.Year}");

            decimal income = trendTransactions
                .Where(t => t.Date.Year == monthToProcess.Year && t.Date.Month == monthToProcess.Month && t.Category.Type == StaticDetails.Income)
                .Sum(t => t.Amount);
            trendIncomeData.Add(income);

            decimal expense = trendTransactions
                .Where(t => t.Date.Year == monthToProcess.Year && t.Date.Month == monthToProcess.Month && t.Category.Type == StaticDetails.Expense)
                .Sum(t => t.Amount);
            trendExpenseData.Add(expense);
        }

        var viewModel = new ReportViewModel
        {
            StartDate = finalStartDate,
            EndDate = finalEndDate,
            Transactions = transactionsInRange,
            CategorySummaries = categorySummaries,
            IncomeSummaries = incomeSummaries,
            TrendLabels = trendLabels,
            TrendIncomeData = trendIncomeData,
            TrendExpenseData = trendExpenseData
        };

        return View(viewModel);
    }
}