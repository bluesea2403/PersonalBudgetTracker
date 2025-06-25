using BudgetTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace BudgetTracker.Areas.Admin.Controllers
{
    [Route("api/Admin/[controller]")] // Thêm "Admin" vào route
    [ApiController]
    [Authorize(Roles = SD.Role_Admin)]
    public class DashboardApiController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardApiController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet("UserRoleDistribution")]
        public async Task<IActionResult> GetUserRoleDistribution()
        {
            var adminUsersCount = (await _userManager.GetUsersInRoleAsync(SD.Role_Admin)).Count;
            var regularUsersCount = (await _userManager.GetUsersInRoleAsync(SD.Role_User)).Count;

            var labels = new[] { "Admin", "User" };
            var data = new[] { adminUsersCount, regularUsersCount };

            return Ok(new { labels, data });
        }
    }
}