using System;
using System.Collections.Generic;

namespace BudgetTracker.Models
{
    public class ReportExportModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<CategorySummary> CategorySummaries { get; set; }
        public List<Transaction> Transactions { get; set; }
        public string ChartImageBase64 { get; set; }
    }
}