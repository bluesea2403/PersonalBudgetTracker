using BudgetTracker.Data;
using BudgetTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

[Authorize] // Yêu cầu người dùng phải đăng nhập mới vào được trang chủ này
public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        // --- PHẦN CODE CŨ BẠN ĐÃ CÓ ---
        var currentUser = await _userManager.GetUserAsync(User);
        var startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        var transactions = await _context.Transactions
                                         .Where(t => t.UserId == currentUser.Id && t.Date >= startDate && t.Date <= endDate)
                                         .Include(t => t.Category)
                                         .ToListAsync();
        // --- LOGIC MỚI: TÍNH TOÁN TỔNG TIẾN ĐỘ TẤT CẢ MỤC TIÊU ---
        var allGoals = await _context.FinancialGoals
                                     .Where(g => g.UserId == currentUser.Id)
                                     .ToListAsync();

        decimal allGoalsTargetAmount = allGoals.Sum(g => g.TargetAmount);
        decimal allGoalsCurrentAmount = allGoals.Sum(g => g.CurrentAmount);
        // --- LOGIC MỚI: LẤY CÁC GIAO DỊCH ĐỊNH KỲ SẮP TỚI ---
        var today = DateTime.Today;
        var upcomingTransactionsList = new List<UpcomingTransactionViewModel>();
        // Lấy tất cả các mẫu đang hoạt động của người dùng
        var activeRecurringTemplates = await _context.RecurringTransactions
            .Where(r => r.UserId == currentUser.Id && r.IsActive)
            .Include(r => r.Category)
            .ToListAsync();


        decimal totalIncome = transactions.Where(t => t.Category.Type == StaticDetails.Income).Sum(t => t.Amount);
        decimal totalExpense = transactions.Where(t => t.Category.Type == StaticDetails.Expense).Sum(t => t.Amount);
        decimal balance = totalIncome - totalExpense;
        // --- LOGIC MỚI: TẠO LỜI NHẮN ĐỘNG VIÊN ---
        string motivationalMessage = "";
        if (totalIncome > totalExpense)
        {
            motivationalMessage = $"WOW! Bạn đã thu nhiều hơn chi {balance.ToString("N0")} đ trong tháng này. Tiếp tục phát huy!";
        }
        else if (totalExpense > totalIncome)
        {
            motivationalMessage = $"Tháng này bạn đã chi nhiều hơn thu {Math.Abs(balance).ToString("N0")} đ. Hãy kiểm soát lại chi tiêu nhé!";
        }
        else if (totalIncome != 0)
        {
            motivationalMessage = "Thu chi cân bằng, bạn đang quản lý tài chính rất tốt!";
        }
        // ==========================================================
        // --- PHẦN CODE MỚI ĐỂ TÍNH TOÁN NGÂN SÁCH ---
        // ==========================================================

        // 1. Lấy tất cả ngân sách người dùng đã đặt cho tháng này
        var budgets = await _context.Budgets
                                    .Where(b => b.UserId == currentUser.Id && b.Year == startDate.Year && b.Month == startDate.Month)
                                    .Include(b => b.Category)
                                    .ToListAsync();

        // 2. Chuẩn bị danh sách để chứa kết quả tiến độ
        var budgetProgresses = new List<BudgetProgressViewModel>();

        // --- LOGIC MỚI: TÍNH TOÁN TOP 3 DANH MỤC CHI TIÊU ---
        var topSpendingCategories = transactions
            .Where(t => t.Category.Type == StaticDetails.Expense) // Chỉ xét các khoản chi
            .GroupBy(t => new { t.CategoryId, t.Category.Name, t.Category.Icon }) // Nhóm theo danh mục
            .Select(g => new TopSpendingCategory
            {
                CategoryName = g.Key.Name,
                Icon = g.Key.Icon,
                TotalSpent = g.Sum(t => t.Amount) // Tính tổng chi cho mỗi nhóm
            })
            .OrderByDescending(c => c.TotalSpent) // Sắp xếp giảm dần
            .Take(4) // Lấy 4 mục đầu tiên
            .ToList();
        // 3. Lặp qua từng ngân sách để tính toán
        foreach (var budget in budgets)
        {
            var amountSpent = transactions
                .Where(t => t.CategoryId == budget.CategoryId)
                .Sum(t => t.Amount);

            int progressPercentage = (budget.Amount > 0) ? (int)((amountSpent / budget.Amount) * 100) : 0;

            // --- LOGIC MỚI: XÁC ĐỊNH TRẠNG THÁI VÀ MÀU SẮC ---
            string statusIcon = "";
            if (progressPercentage >= 100)
            {
                statusIcon = "❌"; // Vượt ngân sách
            }
            else if (progressPercentage >= 80)
            {
                statusIcon = "⚠️"; // Sắp vượt ngân sách
            }

            budgetProgresses.Add(new BudgetProgressViewModel
            {
                CategoryName = budget.Category.Name,
                BudgetAmount = budget.Amount,
                AmountSpent = amountSpent,
                ProgressPercentage = (progressPercentage > 100) ? 100 : progressPercentage, // Giới hạn ở 100% để thanh không bị tràn
                Status = statusIcon // Gán giá trị trạng thái mới
            });
        }
        foreach (var template in activeRecurringTemplates)
        {
            // Tính ngày đến hạn tiếp theo
            var nextDueDate = new DateTime(today.Year, today.Month, template.DayOfMonth);
            if (nextDueDate < today) // Nếu ngày đó đã qua trong tháng này
            {
                nextDueDate = nextDueDate.AddMonths(1); // Thì ngày đến hạn là của tháng sau
            }

            // Kiểm tra xem ngày đến hạn có nằm trong 7 ngày tới không
            if (nextDueDate <= today.AddDays(7))
            {
                upcomingTransactionsList.Add(new UpcomingTransactionViewModel
                {
                    Description = template.Description,
                    Amount = template.Amount,
                    NextDueDate = nextDueDate,
                    CategoryIcon = template.Category.Icon,
                    CategoryType = template.Category.Type
                });
            }
        }

        // --- CẬP NHẬT LẠI VIEWMODEL CUỐI CÙNG ---
        var dashboardViewModel = new DashboardViewModel
        {
            TotalIncome = totalIncome,
            TotalExpense = totalExpense,
            Balance = totalIncome - totalExpense,
            BudgetProgresses = budgetProgresses.OrderBy(p => p.CategoryName).ToList(),
            TopCategories = topSpendingCategories,
            MotivationalMessage = motivationalMessage,
            UpcomingTransactions = upcomingTransactionsList.OrderBy(t => t.NextDueDate).ToList(),
            AllGoalsTargetAmount = allGoalsTargetAmount,
            AllGoalsCurrentAmount = allGoalsCurrentAmount
        };

        return View(dashboardViewModel);
    }
    [AllowAnonymous]  //Công khai
    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [AllowAnonymous]
    public IActionResult About()
    {
        return View();
    }

    [AllowAnonymous]
    public IActionResult Contact()
    {
        return View();
    }
    [AllowAnonymous]
    public IActionResult TermsOfService()
    {
        return View();
    }
    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}