using System.ComponentModel.DataAnnotations;

namespace BudgetTracker.Models
{
    public class AddContributionViewModel
    {
        [Required]
        public int GoalId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số tiền.")]
        [Range(1, double.MaxValue, ErrorMessage = "Số tiền phải lớn hơn 0.")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn một danh mục chi tiêu.")]
        [Display(Name = "Ghi nhận vào Danh mục Chi tiêu")]
        public int CategoryId { get; set; }
    }
}