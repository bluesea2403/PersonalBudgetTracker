using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using BudgetTracker.Models;

namespace BudgetTracker.Services
{
    public interface IOcrService
    {
        Task<Transaction> ExtractTransactionDataFromInvoice(IFormFile invoiceImage);
    }
}