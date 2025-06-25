using System.Threading.Tasks;

namespace BudgetTracker.Services
{
    public interface IChatbotService
    {
        Task<string> GetResponse(string userMessage, string userId);
    }
}