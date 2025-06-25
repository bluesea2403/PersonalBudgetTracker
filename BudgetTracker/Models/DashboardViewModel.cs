using System.Collections.Generic;
namespace BudgetTracker.Models
{
    public class TopSpendingCategory
    {
        public string CategoryName { get; set; }
        public decimal TotalSpent { get; set; }
        public string Icon { get; set; }
    }
    public class BudgetProgressViewModel
    {
        public string CategoryName { get; set; }
        public decimal AmountSpent { get; set; }
        public decimal BudgetAmount { get; set; }
        public int ProgressPercentage { get; set; }
        public string Status { get; set; }
    }
    public class DashboardViewModel
    {
        public decimal TotalIncome { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal Balance { get; set; }
        public List<BudgetProgressViewModel> BudgetProgresses { get; set; }
        public List<TopSpendingCategory> TopCategories { get; set; }
        public string MotivationalMessage { get; set; }
        public List<UpcomingTransactionViewModel> UpcomingTransactions { get; set; }
        public decimal AllGoalsTargetAmount { get; set; }
        public decimal AllGoalsCurrentAmount { get; set; }

    }
    public class UpcomingTransactionViewModel
    {
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public DateTime NextDueDate { get; set; }
        public string CategoryIcon { get; set; }
        public string CategoryType { get; set; }
    }
}