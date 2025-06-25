using BudgetTracker.Data;
using BudgetTracker.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace BudgetTracker.Services
{
    public class ChatbotService : IChatbotService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ChatbotService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<string> GetResponse(string userMessage, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            var userName = user?.UserName?.Split('@')[0] ?? "bạn";
            var lowerMessage = userMessage.ToLower();

            var dateRange = GetDateRange(lowerMessage);

            // === Regel 1: Hỏi về TỔNG THU NHẬP ===
            if (lowerMessage.Contains("thu nhập"))
            {
                decimal totalIncome = await _context.Transactions
                    .Where(t => t.UserId == userId &&
                                t.Category.Type == StaticDetails.Income &&
                                t.Date >= dateRange.StartDate &&
                                t.Date <= dateRange.EndDate)
                    .SumAsync(t => t.Amount);
                return $"Chào {userName}, tổng thu nhập của bạn trong {dateRange.Name} là {totalIncome.ToString("N0", new CultureInfo("vi-VN"))} đ.";
            }

            // === Regel 2: Hỏi về TỔNG CHI TIÊU (không cần danh mục) ===
            var totalExpenseKeywords = new List<string> { "tổng chi", "tổng tiền tiêu", "tổng cộng" };
            if (totalExpenseKeywords.Any(k => lowerMessage.Contains(k)))
            {
                decimal totalExpense = await _context.Transactions
                   .Where(t => t.UserId == userId &&
                               t.Category.Type == StaticDetails.Expense &&
                               t.Date >= dateRange.StartDate &&
                               t.Date <= dateRange.EndDate)
                   .SumAsync(t => t.Amount);
                return $"Chào {userName}, tổng chi tiêu của bạn trong {dateRange.Name} là {totalExpense.ToString("N0", new CultureInfo("vi-VN"))} đ.";
            }

            // === Regel 3: Hỏi về CHI TIÊU THEO DANH MỤC CỤ THỂ (logic cũ) ===
            var expenseKeywords = new List<string> { "tiêu", "chi", "xài", "hết bao nhiêu cho" };
            if (expenseKeywords.Any(keyword => lowerMessage.Contains(keyword)))
            {
                var userCategories = await _context.Categories
                    .Where(c => c.UserId == userId && c.Type == StaticDetails.Expense)
                    .ToListAsync();
                var mentionedCategory = userCategories
                    .FirstOrDefault(c => lowerMessage.Contains(c.Name.ToLower()));

                if (mentionedCategory != null)
                {
                    decimal totalSpent = await _context.Transactions
                        .Where(t => t.UserId == userId &&
                                    t.CategoryId == mentionedCategory.Id &&
                                    t.Date >= dateRange.StartDate &&
                                    t.Date <= dateRange.EndDate)
                        .SumAsync(t => t.Amount);
                    return $"Chào {userName}, trong {dateRange.Name}, bạn đã chi tiêu {totalSpent.ToString("N0", new CultureInfo("vi-VN"))} đ cho '{mentionedCategory.Name}'.";
                }
                return "Tôi có thể tra cứu chi tiêu cho một danh mục cụ thể. Bạn muốn biết về danh mục nào?";
            }

            return $"Xin lỗi {userName}, tôi chưa hiểu câu hỏi của bạn. Hãy thử hỏi về tổng thu nhập, tổng chi tiêu, hoặc chi tiêu cho một danh mục cụ thể nhé.";
        }

        // === HÀM GetDateRange ĐÃ ĐƯỢC NÂNG CẤP ===
        private (DateTime StartDate, DateTime EndDate, string Name) GetDateRange(string message)
        {
            var now = DateTime.Now;
            if (message.Contains("hôm nay"))
            {
                return (now.Date, now.Date.AddDays(1).AddTicks(-1), "hôm nay");
            }
            if (message.Contains("hôm qua"))
            {
                var yesterday = now.AddDays(-1);
                return (yesterday.Date, yesterday.Date.AddDays(1).AddTicks(-1), "hôm qua");
            }
            if (message.Contains("tuần này"))
            {
                var firstDayOfWeek = CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;
                var startOfWeek = now.Date;
                while (startOfWeek.DayOfWeek != firstDayOfWeek)
                {
                    startOfWeek = startOfWeek.AddDays(-1);
                }
                return (startOfWeek, startOfWeek.AddDays(7).AddTicks(-1), "tuần này");
            }
            if (message.Contains("tuần trước"))
            {
                var firstDayOfWeek = CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;
                var startOfLastWeek = now.Date.AddDays(-7);
                while (startOfLastWeek.DayOfWeek != firstDayOfWeek)
                {
                    startOfLastWeek = startOfLastWeek.AddDays(-1);
                }
                return (startOfLastWeek, startOfLastWeek.AddDays(7).AddTicks(-1), "tuần trước");
            }
            if (message.Contains("tháng trước"))
            {
                var lastMonth = now.AddMonths(-1);
                var startDate = new DateTime(lastMonth.Year, lastMonth.Month, 1);
                return (startDate, startDate.AddMonths(1).AddDays(-1), "tháng trước");
            }

            // Mặc định luôn là tháng này
            var defaultStartDate = new DateTime(now.Year, now.Month, 1);
            return (defaultStartDate, defaultStartDate.AddMonths(1).AddDays(-1), "tháng này");
        }
    }
}