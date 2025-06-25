using System.Collections.Generic;

namespace BudgetTracker.Models
{
    public class UserManagementViewModel
    {
        public List<ApplicationUser> Users { get; set; }
        public int TotalUsers { get; set; }
        public int AdminsCount { get; set; }
        public int LockedAccountsCount { get; set; }
    }
}