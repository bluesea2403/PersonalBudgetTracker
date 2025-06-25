using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace BudgetTracker.Models
{
    public class RoleManagementViewModel
    {
        public string UserId { get; set; }
        public string FullName { get; set; }
        public List<SelectListItem> Roles { get; set; }
        // Dùng để nhận các vai trò được chọn từ form
        public string SelectedRole { get; set; }
    }
}