using System.Collections.Generic;

namespace BudgetTracker.Models
{
    public class AdminDashboardViewModel
    {
        // Các thuộc tính đã có
        public int TotalUsers { get; set; }
        public int LockedAccounts { get; set; }
        public int ActiveAccounts { get; set; }
        public int TotalCategories { get; set; }

        // --- THÊM CÁC THUỘC TÍNH MỚI ---
        public int TotalTransactions { get; set; }
        public int TotalBudgets { get; set; }
        public List<ApplicationUser> RecentUsers { get; set; }
    }
}