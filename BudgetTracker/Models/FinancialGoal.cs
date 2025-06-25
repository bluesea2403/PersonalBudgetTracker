using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace BudgetTracker.Models
{
    public class FinancialGoal
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên mục tiêu.")]
        [Display(Name = "Tên mục tiêu")]
        public string Name { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        [Required(ErrorMessage = "Vui lòng nhập số tiền mục tiêu.")]
        [Display(Name = "Số tiền mục tiêu")]
        public decimal TargetAmount { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        [Display(Name = "Số tiền đã tiết kiệm")]
        public decimal CurrentAmount { get; set; } = 0;

        [DataType(DataType.Date)]
        [Display(Name = "Ngày hết hạn (tùy chọn)")]
        public DateTime? Deadline { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public List<GoalContribution> Contributions { get; set; }
    }
}