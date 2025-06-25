using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BudgetTracker.Models
{
    public class Budget
    {
        public int Id { get; set; }

        [Required]
        public int Month { get; set; } // Ví dụ: 6 cho tháng Sáu

        [Required]
        public int Year { get; set; } // Ví dụ: 2025
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18, 2)")]
        [Required(ErrorMessage = "Vui lòng nhập số tiền.")]
        public decimal Amount { get; set; } // Hạn mức chi tiêu

        // Foreign Key tới Category
        public int CategoryId { get; set; }
        public Category Category { get; set; }  

        // Foreign Key tới User
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
    }
}