using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Tesseract;
using BudgetTracker.Models;
using System.Globalization;

namespace BudgetTracker.Services
{
    public class OcrService : IOcrService
    {
        private readonly string _tessDataPath;

        public OcrService()
        {
            _tessDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Tesseract-OCR", "tessdata");
        }

        public async Task<Transaction> ExtractTransactionDataFromInvoice(IFormFile invoiceImage)
        {
            if (invoiceImage == null || invoiceImage.Length == 0) return null;

            var tempImagePath = Path.GetTempFileName();
            await using (var stream = new FileStream(tempImagePath, FileMode.Create))
            {
                await invoiceImage.CopyToAsync(stream);
            }

            try
            {
                using (var engine = new TesseractEngine(_tessDataPath, "vie", EngineMode.Default))
                {
                    using (var img = Pix.LoadFromFile(tempImagePath))
                    {
                        using (var page = engine.Process(img))
                        {
                            var text = page.GetText();
                            return ParseTextToTransaction(text);
                        }
                    }
                }
            }
            finally
            {
                File.Delete(tempImagePath);
            }
        }

        private Transaction ParseTextToTransaction(string text)
        {
            var lines = text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            var transaction = new Transaction
            {
                Amount = ParseAmount(lines),
                Date = ParseDate(lines),
                Description = ParseDescription(lines)
            };
            return transaction;
        }

        private string ParseDescription(string[] lines)
        {
            // Cố gắng tìm dòng tiêu đề in hoa, không phải là "CHI TIẾT GIAO DỊCH"
            var titleLine = lines.FirstOrDefault(l =>
                l.Trim().Length > 5 &&
                !l.ToLower().Contains("chi tiết giao dịch") &&
                l.Equals(l.ToUpper(), StringComparison.Ordinal));

            return titleLine?.Trim() ?? lines.FirstOrDefault() ?? "Giao dịch từ hóa đơn";
        }

        private DateTime ParseDate(string[] lines)
        {
            var dateRegex = new Regex(@"(\d{1,2}\/\d{1,2}\/\d{4})");
            foreach (var line in lines)
            {
                var match = dateRegex.Match(line);
                if (match.Success)
                {
                    if (DateTime.TryParseExact(match.Value, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                    {
                        return date;
                    }
                }
            }
            return DateTime.Now;
        }

        private decimal ParseAmount(string[] lines)
        {
            decimal finalAmount = 0;
            var amountRegex = new Regex(@"([\d.,]+)");

            // Ưu tiên 1: Tìm các dòng chứa số tiền có dấu trừ hoặc chữ "Thành công"
            var priorityLines = lines.Where(l => l.Contains("-") || l.ToLower().Contains("thành công") || l.ToLower().Contains("paid")).ToList();
            if (priorityLines.Any())
            {
                foreach (var line in priorityLines)
                {
                    var matches = amountRegex.Matches(line);
                    foreach (Match match in matches)
                    {
                        string amountStr = Regex.Replace(match.Value, @"[.,đ\s]", "").Trim();
                        if (decimal.TryParse(amountStr, out decimal currentAmount))
                        {
                            if (currentAmount > finalAmount) finalAmount = currentAmount;
                        }
                    }
                }
                if (finalAmount > 0) return finalAmount;
            }

            // Ưu tiên 2: Nếu không có, tìm số lớn nhất trong 5 dòng cuối cùng
            var lastLines = lines.Reverse().Take(5);
            foreach (var line in lastLines)
            {
                var matches = amountRegex.Matches(line);
                foreach (Match match in matches)
                {
                    string amountStr = Regex.Replace(match.Value, @"[.,đ\s]", "").Trim();
                    if (decimal.TryParse(amountStr, out decimal currentAmount))
                    {
                        if (currentAmount > finalAmount) finalAmount = currentAmount;
                    }
                }
            }
            return finalAmount;
        }
    }
}