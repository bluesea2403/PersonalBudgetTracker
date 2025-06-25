using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BudgetTracker.Models
{
    public class GoalContribution
    {
        public int Id { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; }

        public DateTime ContributionDate { get; set; }

        public int FinancialGoalId { get; set; }
        public FinancialGoal FinancialGoal { get; set; }
    }
}