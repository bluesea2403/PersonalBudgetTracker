using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BudgetTracker.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18,2)")]
        [Required(ErrorMessage = "Số tiền là bắt buộc.")]
        public decimal Amount { get; set; }

        public string? Description { get; set; }

        [Required]
        public DateTime Date { get; set; }

        // Khóa ngoại tới ApplicationUser
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        // Khóa ngoại tới Category
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        public int? GoalContributionId { get; set; }
        public GoalContribution? GoalContribution { get; set; }
    }
}