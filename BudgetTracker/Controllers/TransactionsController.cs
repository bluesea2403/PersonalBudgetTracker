using X.PagedList;
using BudgetTracker.Data;
using BudgetTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using System.Text;
[Authorize]
public class TransactionsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICompositeViewEngine _viewEngine;
    public TransactionsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ICompositeViewEngine viewEngine)
    {
        _context = context;
        _userManager = userManager;
        _viewEngine = viewEngine;
        _viewEngine = viewEngine;
    }


    // GET: Transactions
    public async Task<IActionResult> Index(string sortOrder, int? categoryId, string searchString, DateTime? startDate, DateTime? endDate, int? page)
    {
        var currentUser = await _userManager.GetUserAsync(User);

        // Lưu lại các tham số để dùng trong View
        ViewData["CurrentSort"] = sortOrder;
        ViewData["DateSortParm"] = String.IsNullOrEmpty(sortOrder) ? "date_desc" : "";
        ViewData["AmountSortParm"] = sortOrder == "Amount" ? "amount_desc" : "Amount";
        ViewData["CurrentCategory"] = categoryId;
        ViewData["CurrentSearch"] = searchString;
        ViewData["CurrentStartDate"] = startDate?.ToString("yyyy-MM-dd");
        ViewData["CurrentEndDate"] = endDate?.ToString("yyyy-MM-dd");

        IQueryable<Transaction> transactionsQuery = _context.Transactions
            .Where(t => t.UserId == currentUser.Id)
            .Include(t => t.Category);

        // Áp dụng các bộ lọc
        if (!String.IsNullOrEmpty(searchString))
        {
            transactionsQuery = transactionsQuery.Where(t => t.Description.Contains(searchString));
        }
        if (categoryId.HasValue)
        {
            transactionsQuery = transactionsQuery.Where(t => t.CategoryId == categoryId.Value);
        }
        if (startDate.HasValue)
        {
            transactionsQuery = transactionsQuery.Where(t => t.Date >= startDate.Value);
        }
        if (endDate.HasValue)
        {
            transactionsQuery = transactionsQuery.Where(t => t.Date <= endDate.Value);
        }

        // Áp dụng sắp xếp
        switch (sortOrder)
        {
            case "date_desc":
                transactionsQuery = transactionsQuery.OrderByDescending(t => t.Date);
                break;
            case "Amount":
                transactionsQuery = transactionsQuery.OrderBy(t => t.Amount);
                break;
            case "amount_desc":
                transactionsQuery = transactionsQuery.OrderByDescending(t => t.Amount);
                break;
            default: // Mặc định sắp xếp theo ngày mới nhất
                transactionsQuery = transactionsQuery.OrderBy(t => t.Date);
                break;
        }

        ViewData["CategoryIdFilter"] = new SelectList(await _context.Categories.Where(c => c.UserId == currentUser.Id || c.UserId == null).ToListAsync(), "Id", "Name", categoryId);

        int pageSize = 10;
        int pageNumber = page ?? 1;
        var pagedList = await transactionsQuery.AsNoTracking().ToPagedListAsync(pageNumber, pageSize);
        return View(pagedList);
    }
    // GET: Transactions/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var currentUser = await _userManager.GetUserAsync(User);
        var transaction = await _context.Transactions
            .Include(t => t.Category)
            .FirstOrDefaultAsync(m => m.Id == id && m.UserId == currentUser.Id);
        if (transaction == null) return NotFound();
        return View(transaction);
    }

    // GET: Transactions/Create
    public async Task<IActionResult> Create(decimal? amount, string description)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        ViewData["CategoryId"] = new SelectList(
     await _context.Categories.Where(c => c.UserId == currentUser.Id || c.UserId == null).ToListAsync(),
     "Id", "Name");

        var transaction = new Transaction
        {
            Date = DateTime.Now,
            Amount = amount ?? 0,
            Description = description ?? ""
        };

        return PartialView("_TransactionForm", transaction);
    }

    // POST: Transactions/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Amount,Description,Date,CategoryId")] Transaction transaction)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        transaction.UserId = currentUser.Id;

        ModelState.Remove("User");
        ModelState.Remove("UserId");
        ModelState.Remove("Category");

        if (ModelState.IsValid)
        {
            _context.Add(transaction);
            await _context.SaveChangesAsync();
            return Json(new { isValid = true, html = "" });
        }
        ViewData["CategoryId"] = new SelectList(_context.Categories.Where(c => c.UserId == currentUser.Id), "Id", "Name", transaction.CategoryId);
        // Dòng code mới
        return Json(new { isValid = false, html = await RenderRazorViewToString("_TransactionForm", transaction) });
    }

    // GET: Transactions/Edit/5  <-- PHẦN THÊM VÀO
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var currentUser = await _userManager.GetUserAsync(User);
        var transaction = await _context.Transactions.FirstOrDefaultAsync(t => t.Id == id && t.UserId == currentUser.Id);
        if (transaction == null) return NotFound();

        ViewData["CategoryId"] = new SelectList(await _context.Categories.Where(c => c.UserId == currentUser.Id || c.UserId == null).ToListAsync(), "Id", "Name", transaction.CategoryId); ViewBag.Action = "Edit";
        return PartialView("_TransactionForm", transaction);
    }

    // POST: Transactions/Edit/5  <-- PHẦN THÊM VÀO
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Amount,Description,Date,CategoryId")] Transaction transactionFromForm)
    {
        if (id != transactionFromForm.Id)
        {
            return NotFound();
        }

        var currentUser = await _userManager.GetUserAsync(User);

        // --- LOGIC NÂNG CẤP ĐỂ ĐỒNG BỘ HÓA ---

        // 1. Lấy giao dịch GỐC từ DB, bao gồm cả thông tin đóng góp
        var transactionInDb = await _context.Transactions
            .AsNoTracking() // Rất quan trọng: dùng AsNoTracking để tránh lỗi theo dõi của EF Core
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == currentUser.Id);

        if (transactionInDb == null)
        {
            return NotFound();
        }

        // Lấy lại GoalContributionId từ bản ghi gốc trong DB
        transactionFromForm.GoalContributionId = transactionInDb.GoalContributionId;

        // 2. Kiểm tra nếu đây là giao dịch đóng góp cho mục tiêu
        if (transactionFromForm.GoalContributionId != null)
        {
            // Tìm mục tiêu cha
            var contributionInDb = await _context.GoalContributions.FirstOrDefaultAsync(gc => gc.Id == transactionFromForm.GoalContributionId);
            var goal = await _context.FinancialGoals.FindAsync(contributionInDb.FinancialGoalId);

            if (goal != null)
            {
                // Tính toán sự chênh lệch giữa số tiền mới và cũ
                var amountDifference = transactionFromForm.Amount - transactionInDb.Amount;
                // Cập nhật lại mục tiêu với sự chênh lệch đó
                goal.CurrentAmount += amountDifference;
                _context.Update(goal);

                // Cập nhật lại số tiền trong bản ghi đóng góp
                contributionInDb.Amount = transactionFromForm.Amount;
                _context.Update(contributionInDb);
            }
        }

        // --- KẾT THÚC LOGIC NÂNG CẤP ---

        // Gán lại UserId để bảo mật
        transactionFromForm.UserId = currentUser.Id;

        ModelState.Remove("User");
        ModelState.Remove("Category");
        ModelState.Remove("GoalContribution");

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(transactionFromForm);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Transactions.Any(e => e.Id == transactionFromForm.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            // Trả về kết quả thành công cho AJAX để tải lại trang
            string html = await RenderRazorViewToString("_ViewAll", await _context.Transactions.Where(t => t.UserId == currentUser.Id).Include(t => t.Category).ToListAsync());
            return Json(new { isValid = true, html = html });
        }

        // Trả về form với lỗi nếu không hợp lệ
        ViewData["CategoryId"] = new SelectList(await _context.Categories.Where(c => c.UserId == currentUser.Id || c.UserId == null).ToListAsync(), "Id", "Name", transactionFromForm.CategoryId);
        string errorHtml = await RenderRazorViewToString("_TransactionForm", transactionFromForm);
        return Json(new { isValid = false, html = errorHtml });
    }

    // GET: Transactions/Delete/5  <-- PHẦN THÊM VÀO
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var currentUser = await _userManager.GetUserAsync(User);
        var transaction = await _context.Transactions
            .Include(t => t.Category)
            .FirstOrDefaultAsync(m => m.Id == id && m.UserId == currentUser.Id);
        if (transaction == null) return NotFound();

        return View(transaction);
    }

    // POST: Transactions/Delete/5  <-- PHẦN THÊM VÀO
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var currentUser = await _userManager.GetUserAsync(User);

        // Lấy giao dịch VÀ thông tin đóng góp liên quan (nếu có)
        var transaction = await _context.Transactions
            .Include(t => t.GoalContribution)
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == currentUser.Id);

        if (transaction != null)
        {
            // KIỂM TRA NẾU ĐÂY LÀ GIAO DỊCH ĐÓNG GÓP CHO MỤC TIÊU
            if (transaction.GoalContribution != null)
            {
                // Tìm đến mục tiêu cha
                var goal = await _context.FinancialGoals.FindAsync(transaction.GoalContribution.FinancialGoalId);
                if (goal != null)
                {
                    // Trừ lại số tiền đã đóng góp khỏi mục tiêu
                    goal.CurrentAmount -= transaction.Amount;
                    _context.Update(goal);
                }
                // Xóa cả bản ghi đóng góp
                _context.GoalContributions.Remove(transaction.GoalContribution);
            }

            // Xóa giao dịch như bình thường
            _context.Transactions.Remove(transaction);

            // Lưu tất cả các thay đổi vào DB cùng một lúc
            await _context.SaveChangesAsync();

            // Trả về kết quả thành công cho AJAX
            return Json(new { success = true, message = "Xóa giao dịch thành công." });
        }

        return Json(new { success = false, message = "Không tìm thấy giao dịch." });
    }

    private bool TransactionExists(int id)
    {
        return _context.Transactions.Any(e => e.Id == id);
    }
    // Dán phương thức này vào cuối class TransactionsController
    [NonAction]
    public async Task<string> RenderRazorViewToString(string viewName, object model)
    {
        ViewData.Model = model;
        using (var sw = new StringWriter())
        {
            var viewResult = _viewEngine.FindView(ControllerContext, viewName, false);
            if (viewResult.View == null)
            {
                throw new ArgumentNullException($"{viewName} does not match any available view");
            }
            var viewContext = new ViewContext(
                ControllerContext,
                viewResult.View,
                ViewData,
                TempData,
                sw,
                new HtmlHelperOptions()
            );
            await viewResult.View.RenderAsync(viewContext);
            return sw.ToString();
        }
    }
    public async Task<IActionResult> ExportToCsv(int? categoryId, string searchString, DateTime? startDate, DateTime? endDate)
    {
        var currentUser = await _userManager.GetUserAsync(User);

        // Bắt đầu câu truy vấn, giống hệt action Index
        IQueryable<Transaction> transactionsQuery = _context.Transactions
                                                            .Where(t => t.UserId == currentUser.Id);

        // Áp dụng các bộ lọc y hệt action Index
        if (!String.IsNullOrEmpty(searchString))
        {
            transactionsQuery = transactionsQuery.Where(t => t.Description.Contains(searchString));
        }
        if (categoryId.HasValue)
        {
            transactionsQuery = transactionsQuery.Where(t => t.CategoryId == categoryId.Value);
        }
        if (startDate.HasValue)
        {
            transactionsQuery = transactionsQuery.Where(t => t.Date >= startDate.Value);
        }
        if (endDate.HasValue)
        {
            transactionsQuery = transactionsQuery.Where(t => t.Date <= endDate.Value);
        }

        // Lấy dữ liệu đã được lọc
        var transactions = await transactionsQuery
                                    .Include(t => t.Category)
                                    .OrderByDescending(t => t.Date)
                                    .ToListAsync();

        // Phần tạo file CSV giữ nguyên
        var builder = new StringBuilder();
        builder.AppendLine("Ngày,Loại,Danh mục,Mô tả,Số tiền");
        foreach (var transaction in transactions)
        {
            builder.AppendLine($"{transaction.Date:dd/MM/yyyy},{transaction.Category.Type},\"{transaction.Category.Name}\",\"{transaction.Description}\",{transaction.Amount}");
        }

        return File(Encoding.UTF8.GetBytes(builder.ToString()), "text/csv", $"GiaoDich_{DateTime.Now:dd-MM-yyyy}.csv");
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteMultiple([FromBody] List<int> ids)
    {
        if (ids == null || !ids.Any())
        {
            return Json(new { success = false, message = "Vui lòng chọn ít nhất một giao dịch để xóa." });
        }

        var currentUser = await _userManager.GetUserAsync(User);

        // Tìm tất cả các giao dịch hợp lệ (thuộc về người dùng và có trong danh sách ID)
        var transactionsToDelete = await _context.Transactions
            .Where(t => t.UserId == currentUser.Id && ids.Contains(t.Id))
            .ToListAsync();

        if (transactionsToDelete.Any())
        {
            _context.Transactions.RemoveRange(transactionsToDelete);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = $"Đã xóa thành công {transactionsToDelete.Count} giao dịch." });
        }

        return Json(new { success = false, message = "Không tìm thấy giao dịch nào để xóa." });
    }
}