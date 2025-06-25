using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BudgetTracker.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên danh mục là bắt buộc.")]
        public string Name { get; set; }

        // "Thu" hoặc "Chi"
        [Required]
        public string Type { get; set; }

        public string? Icon { get; set; }

        // Khóa ngoại tới ApplicationUser
        public string? UserId { get; set; }
        public ApplicationUser User { get; set; }
        [Display(Name = "Nhóm danh mục")]
        public string? GroupName { get; set; }
    }
}