using BudgetTracker.Data;
using BudgetTracker.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BudgetTracker.Services
{
    public class RecurringTransactionService : IRecurringTransactionService
    {
        private readonly ApplicationDbContext _context;

        public RecurringTransactionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task ProcessRecurringTransactions(string userId)
        {
            var now = DateTime.Now;
            // Lấy tất cả các mẫu giao dịch định kỳ của người dùng
            var recurringTemplates = await _context.RecurringTransactions
    .Where(r => r.UserId == userId && r.IsActive) // <-- THÊM ĐIỀU KIỆN NÀY
    .ToListAsync();

            foreach (var template in recurringTemplates)
            {
                // Kiểm tra xem giao dịch này đã được xử lý cho tháng hiện tại chưa
                bool alreadyProcessedThisMonth = template.LastProcessedDate.HasValue &&
                                                 template.LastProcessedDate.Value.Year == now.Year &&
                                                 template.LastProcessedDate.Value.Month == now.Month;

                if (!alreadyProcessedThisMonth)
                {
                    // Tạo giao dịch mới cho tháng này
                    var newTransaction = new Transaction
                    {
                        Amount = template.Amount,
                        Description = template.Description,
                        Date = new DateTime(now.Year, now.Month, template.DayOfMonth),
                        CategoryId = template.CategoryId,
                        UserId = userId
                    };

                    _context.Transactions.Add(newTransaction);

                    // Cập nhật lại ngày xử lý cuối cùng của mẫu
                    template.LastProcessedDate = now;
                    _context.RecurringTransactions.Update(template);
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}