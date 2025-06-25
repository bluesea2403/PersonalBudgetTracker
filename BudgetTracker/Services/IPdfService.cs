using BudgetTracker.Models;
using System.Threading.Tasks;

namespace BudgetTracker.Services
{
    public interface IPdfService
    {
        byte[] CreateReportPdf(ReportViewModel reportData, string chartImageBase64);
    }
}