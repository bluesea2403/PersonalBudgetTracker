using System.Threading.Tasks;

namespace BudgetTracker.Services
{
    public interface IRecurringTransactionService
    {
        Task ProcessRecurringTransactions(string userId);
    }
}