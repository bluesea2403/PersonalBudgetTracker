using BudgetTracker.Data;
using BudgetTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace BudgetTracker.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class DashboardController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context; // Inject DbContext

        public DashboardController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context; // Gán giá trị
        }

        public async Task<IActionResult> Index()
        {
            // Các tính toán đã có
            var totalUsers = await _userManager.Users.CountAsync();
            var lockedAccounts = await _userManager.Users.CountAsync(u => u.LockoutEnd != null && u.LockoutEnd > DateTime.Now);
            var activeAccounts = totalUsers - lockedAccounts;
            var totalCategories = await _context.Categories.CountAsync();

            // --- TÍNH TOÁN CÁC SỐ LIỆU MỚI ---
            var totalTransactions = await _context.Transactions.CountAsync();
            var totalBudgets = await _context.Budgets.CountAsync();
            var recentUsers = await _context.Users
                                            .OrderByDescending(u => u.Id) // Cách đơn giản để lấy user mới nhất
                                            .Take(5) // Lấy 5 người dùng mới nhất
                                            .ToListAsync();

            // Tạo ViewModel và gán tất cả giá trị
            var viewModel = new AdminDashboardViewModel
            {
                TotalUsers = totalUsers,
                TotalCategories = totalCategories,
                LockedAccounts = lockedAccounts,
                ActiveAccounts = activeAccounts,
                TotalTransactions = totalTransactions,
                TotalBudgets = totalBudgets,
                RecentUsers = recentUsers
            };

            return View(viewModel);
        }
    }
}