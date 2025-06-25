using BudgetTracker.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using BudgetTracker.Models;

namespace BudgetTracker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatbotController : ControllerBase
    {
        private readonly IChatbotService _chatbotService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ChatbotController(IChatbotService chatbotService, UserManager<ApplicationUser> userManager)
        {
            _chatbotService = chatbotService;
            _userManager = userManager;
        }

        [HttpPost("SendMessage")]
        public async Task<IActionResult> SendMessage([FromBody] ChatMessage message)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var response = await _chatbotService.GetResponse(message.Text, user.Id);
            return Ok(new { reply = response });
        }
    }

    // Model đơn giản để nhận message từ client
    public class ChatMessage
    {
        public string Text { get; set; }
    }
}