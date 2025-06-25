using System;
using System.Collections.Generic;

namespace BudgetTracker.Models
{
    // Model này dùng để chứa dữ liệu tóm tắt theo danh mục
    public class CategorySummary
    {
        public string CategoryName { get; set; }
        public decimal TotalAmount { get; set; }
    }

    // Model chính để gửi tất cả dữ liệu cần thiết sang View
    public class ReportViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<Transaction> Transactions { get; set; }
        public List<CategorySummary> CategorySummaries { get; set; }
        public List<CategorySummary> IncomeSummaries { get; set; } // Dùng cho tóm tắt thu nhập

        public List<string> TrendLabels { get; set; } // Chứa các nhãn tháng, ví dụ: "1/2025", "2/2025"
        public List<decimal> TrendIncomeData { get; set; } // Chứa dữ liệu tổng thu của mỗi tháng
        public List<decimal> TrendExpenseData { get; set; } // Chứa dữ liệu tổng chi của mỗi tháng
    }
}