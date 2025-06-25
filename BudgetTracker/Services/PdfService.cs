using BudgetTracker.Models;
using BudgetTracker.Services;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Path = System.IO.Path;

namespace BudgetTracker.Services
{
    public class PdfService : IPdfService
    {   
        private readonly string _fontRegularPath;
        private readonly string _fontBoldPath;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PdfService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
            var fontsDir = Path.Combine(webHostEnvironment.WebRootPath, "fonts");
            _fontRegularPath = Path.Combine(fontsDir, "Roboto-Regular.ttf");
            _fontBoldPath = Path.Combine(fontsDir, "Roboto-Bold.ttf");
        }

        public byte[] CreateReportPdf(ReportViewModel reportData, string chartImageBase64)
        {
            using (var memoryStream = new MemoryStream())
            {
                var writer = new PdfWriter(memoryStream);
                var pdf = new PdfDocument(writer);
                var document = new Document(pdf, PageSize.A4);
                document.SetMargins(30, 30, 30, 30);

                var fontRegular = PdfFontFactory.CreateFont(_fontRegularPath, iText.IO.Font.PdfEncodings.IDENTITY_H, PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED);
                var fontBold = PdfFontFactory.CreateFont(_fontBoldPath, iText.IO.Font.PdfEncodings.IDENTITY_H, PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED);

                // --- Tiêu đề chính ---
                var title = new Paragraph("BÁO CÁO TÀI CHÍNH")
                    .SetFont(fontBold).SetFontSize(20).SetTextAlignment(TextAlignment.CENTER);
                document.Add(title);

                var dateRange = new Paragraph($"Từ ngày {reportData.StartDate:dd/MM/yyyy} đến ngày {reportData.EndDate:dd/MM/yyyy}")
                    .SetFont(fontRegular).SetFontSize(12).SetTextAlignment(TextAlignment.CENTER);
                document.Add(dateRange);

                document.Add(new Paragraph().SetMarginBottom(20));

                if (!string.IsNullOrEmpty(chartImageBase64) && chartImageBase64.Contains(','))
                {
                    var imageBytes = Convert.FromBase64String(chartImageBase64.Split(',')[1]);
                    var imageData = ImageDataFactory.Create(imageBytes);
                    var image = new iText.Layout.Element.Image(imageData).SetWidth(UnitValue.CreatePercentValue(100));
                    document.Add(image);
                    document.Add(new Paragraph().SetMarginBottom(20));
                }

                // --- Tóm tắt chi tiêu ---
                document.Add(new Paragraph("Tóm tắt chi tiêu").SetFont(fontBold).SetFontSize(14));

                var summaryTable = new Table(new float[] { 3, 2 }).UseAllAvailableWidth();
                summaryTable.AddHeaderCell(new Cell().Add(new Paragraph("Danh mục").SetFont(fontBold)));
                summaryTable.AddHeaderCell(new Cell().Add(new Paragraph("Tổng chi").SetFont(fontBold)).SetTextAlignment(TextAlignment.RIGHT));

                foreach (var summary in reportData.CategorySummaries)
                {
                    summaryTable.AddCell(new Paragraph(summary.CategoryName).SetFont(fontRegular));
                    summaryTable.AddCell(new Paragraph(summary.TotalAmount.ToString("N0") + " đ").SetFont(fontRegular).SetTextAlignment(TextAlignment.RIGHT));
                }

                document.Add(summaryTable);
                document.Add(new Paragraph().SetMarginBottom(20));

                // --- Chi tiết giao dịch ---
                document.Add(new Paragraph("Chi tiết giao dịch").SetFont(fontBold).SetFontSize(14));

                var detailTable = new Table(new float[] { 2, 3, 4, 2 }).UseAllAvailableWidth();
                detailTable.AddHeaderCell(new Cell().Add(new Paragraph("Ngày").SetFont(fontBold)));
                detailTable.AddHeaderCell(new Cell().Add(new Paragraph("Danh mục").SetFont(fontBold)));
                detailTable.AddHeaderCell(new Cell().Add(new Paragraph("Mô tả").SetFont(fontBold)));
                detailTable.AddHeaderCell(new Cell().Add(new Paragraph("Số tiền").SetFont(fontBold)).SetTextAlignment(TextAlignment.RIGHT));

                foreach (var transaction in reportData.Transactions)
                {
                    detailTable.AddCell(new Paragraph(transaction.Date.ToString("dd/MM/yyyy")).SetFont(fontRegular));
                    detailTable.AddCell(new Paragraph(transaction.Category.Name).SetFont(fontRegular));
                    detailTable.AddCell(new Paragraph(transaction.Description ?? "").SetFont(fontRegular));

                    var amountPara = new Paragraph(transaction.Amount.ToString("N0") + " đ")
                        .SetFont(fontRegular)
                        .SetTextAlignment(TextAlignment.RIGHT);

                    if (transaction.Category.Type == StaticDetails.Expense)
                    {
                        amountPara.SetFontColor(ColorConstants.RED);
                    }

                    detailTable.AddCell(amountPara);
                }

                document.Add(detailTable);
                document.Close();
                return memoryStream.ToArray();
            }
        }
    }
}
