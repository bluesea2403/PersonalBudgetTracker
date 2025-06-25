using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace BudgetTracker.Models
{
    public class RecurringTransaction
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Mô tả là bắt buộc.")]
        public string Description { get; set; }
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18, 2)")]
        [Required]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày trong tháng.")]
        [Range(1, 28, ErrorMessage = "Để đơn giản, vui lòng chọn ngày từ 1 đến 28.")]
        public int DayOfMonth { get; set; } // Ngày sẽ lặp lại, ví dụ: 25

        // Dùng để theo dõi lần cuối giao dịch này được tạo
        public DateTime? LastProcessedDate { get; set; }

        public bool IsActive { get; set; } = true; // Mặc định là true (hoạt động)

        public int CategoryId { get; set; }
        public Category Category { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
    }
}