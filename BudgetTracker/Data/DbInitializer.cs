using BudgetTracker.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BudgetTracker.Data
{
    public static class DbInitializer
    {
        public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
        {
            // Lấy các dịch vụ cần thiết
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Danh sách các vai trò cần tạo
            string[] roleNames = { SD.Role_Admin, SD.Role_User };

            foreach (var roleName in roleNames)
            {
                // Kiểm tra xem vai trò đã tồn tại chưa
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    // Nếu chưa, tạo mới vai trò
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }
    }
}