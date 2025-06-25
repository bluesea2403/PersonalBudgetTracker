// THÊM CÁC USING CẦN THIẾT Ở ĐÂY
using BudgetTracker.Data;
using BudgetTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BudgetTracker.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var adminUsers = await _userManager.GetUsersInRoleAsync(SD.Role_Admin);

            var viewModel = new UserManagementViewModel
            {
                Users = users,
                TotalUsers = users.Count,
                AdminsCount = adminUsers.Count,
                LockedAccountsCount = users.Count(u => u.LockoutEnd != null && u.LockoutEnd > DateTime.Now)
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> LockUnlock(string id)
        {
            // SỬA LẠI: Tìm người dùng trong _context.Users
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            if (user.LockoutEnd != null && user.LockoutEnd > DateTime.Now)
            {
                user.LockoutEnd = null; // Mở khóa
            }
            else
            {
                user.LockoutEnd = DateTime.Now.AddYears(100); // Khóa
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        // GET: Admin/User/RoleManagement/GUID
        public async Task<IActionResult> RoleManagement(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _roleManager.Roles.ToListAsync();
            var userRoles = await _userManager.GetRolesAsync(user);

            var model = new RoleManagementViewModel
            {
                UserId = user.Id,
                FullName = user.FullName,
                SelectedRole = userRoles.FirstOrDefault(),
                Roles = roles.Select(r => new SelectListItem
                {
                    Text = r.Name,
                    Value = r.Name,
                    Selected = userRoles.Contains(r.Name)
                }).ToList()
            };

            return View(model);
        }

        // POST: Admin/User/RoleManagement
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RoleManagement(RoleManagementViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                return NotFound();
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            // Xóa tất cả vai trò cũ
            await _userManager.RemoveFromRolesAsync(user, userRoles);
            // Chỉ thêm một vai trò mới được chọn (nếu có)
            if (!string.IsNullOrEmpty(model.SelectedRole))
            {
                await _userManager.AddToRoleAsync(user, model.SelectedRole);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}