using System.ComponentModel.DataAnnotations;

namespace BudgetTracker.Models
{
    public class CategoryGroup
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Tên Nhóm")]
        public string Name { get; set; }

        [Display(Name = "Icon")]
        public string? Icon { get; set; }
    }
}