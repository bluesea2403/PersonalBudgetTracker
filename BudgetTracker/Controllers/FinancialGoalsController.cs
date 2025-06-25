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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace BudgetTracker.Controllers
{
    [Authorize]
    public class FinancialGoalsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICompositeViewEngine _viewEngine;

        public FinancialGoalsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ICompositeViewEngine viewEngine)
        {
            _context = context;
            _userManager = userManager;
            _viewEngine = viewEngine;
        }

        // GET: FinancialGoals
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var financialGoals = await _context.FinancialGoals
                                               .Where(g => g.UserId == currentUser.Id)
                                               .OrderBy(g => g.Deadline)
                                               .ToListAsync();
            return View(financialGoals);
        }
        // GET: FinancialGoals/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var financialGoal = await _context.FinancialGoals
                .Include(f => f.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (financialGoal == null)
            {
                return NotFound();
            }

            return View(financialGoal);
        }

        // GET: FinancialGoals/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: FinancialGoals/Create
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,TargetAmount,Deadline")] FinancialGoal financialGoal)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            financialGoal.UserId = currentUser.Id;
            financialGoal.CurrentAmount = 0; 

            ModelState.Remove("User");
            ModelState.Remove("UserId");
            ModelState.Remove("Contributions");
            if (ModelState.IsValid)
            {
                _context.Add(financialGoal);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(financialGoal);
        }

        // GET: FinancialGoals/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var currentUser = await _userManager.GetUserAsync(User);
            var financialGoal = await _context.FinancialGoals.FirstOrDefaultAsync(g => g.Id == id && g.UserId == currentUser.Id);
            if (financialGoal == null)
            {
                return NotFound();
            }
            return View(financialGoal);
        }

        // POST: FinancialGoals/Edit/5
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,TargetAmount,Deadline")] FinancialGoal financialGoal)
        {
            if (id != financialGoal.Id)
            {
                return NotFound();
            }
            var currentUser = await _userManager.GetUserAsync(User);
            // Lấy lại dữ liệu cũ từ DB để đảm bảo không sửa đổi các trường không được phép
            var goalInDb = await _context.FinancialGoals.AsNoTracking().FirstOrDefaultAsync(g => g.Id == id && g.UserId == currentUser.Id);
            if (goalInDb == null) return NotFound();

            // Gán lại các giá trị không được sửa
            financialGoal.UserId = goalInDb.UserId;
            financialGoal.CurrentAmount = goalInDb.CurrentAmount;

            ModelState.Remove("User");
            ModelState.Remove("Contributions");
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(financialGoal);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.FinancialGoals.Any(e => e.Id == financialGoal.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(financialGoal);
        }

        // GET: FinancialGoals/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var financialGoal = await _context.FinancialGoals
                .Include(f => f.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (financialGoal == null)
            {
                return NotFound();
            }

            return View(financialGoal);
        }

        // POST: FinancialGoals/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var financialGoal = await _context.FinancialGoals.FindAsync(id);
            if (financialGoal != null)
            {
                _context.FinancialGoals.Remove(financialGoal);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        // GET: FinancialGoals/AddContribution/5
        public async Task<IActionResult> AddContribution(int? goalId)
        {
            if (goalId == null) return NotFound();

            var goal = await _context.FinancialGoals.FindAsync(goalId);
            if (goal == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);

            // Chỉ lấy các danh mục CHI TIÊU để ghi nhận khoản tiết kiệm này
            ViewData["CategoryId"] = new SelectList(
    _context.Categories.Where(c => (c.UserId == currentUser.Id || c.UserId == null) && c.Type == StaticDetails.Expense), "Id", "Name");

            var model = new AddContributionViewModel { GoalId = goalId.Value };
            return PartialView("_AddContributionForm", model);
        }

        // POST: FinancialGoals/AddContribution
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddContribution(AddContributionViewModel model)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            // Bọc trong câu lệnh if(ModelState.IsValid) để đảm bảo dữ liệu hợp lệ
            if (ModelState.IsValid)
            {
                var goal = await _context.FinancialGoals
                                         .FirstOrDefaultAsync(g => g.Id == model.GoalId && g.UserId == currentUser.Id);
                if (goal == null)
                {
                    return NotFound();
                }

                // --- LOGIC ĐÃ ĐƯỢC SẮP XẾP LẠI ---

                // 1. Tạo khoản đóng góp
                var contribution = new GoalContribution
                {
                    Amount = model.Amount,
                    ContributionDate = DateTime.Now,
                    FinancialGoalId = model.GoalId
                };

                // 2. Tạo giao dịch CHI TIÊU tương ứng
                var transaction = new Transaction
                {
                    Amount = model.Amount,
                    CategoryId = model.CategoryId,
                    Date = DateTime.Now,
                    Description = $"Tiết kiệm cho mục tiêu: {goal.Name}",
                    UserId = currentUser.Id,
                    GoalContribution = contribution // Liên kết với khoản đóng góp
                };

                // 3. Cập nhật lại số tiền đã tiết kiệm của mục tiêu
                goal.CurrentAmount += model.Amount;

                // 4. Bây giờ mới thêm và cập nhật vào context
                _context.Transactions.Add(transaction);
                _context.FinancialGoals.Update(goal);

                // 5. Lưu tất cả thay đổi vào DB một lần duy nhất
                await _context.SaveChangesAsync();

                return Json(new { isValid = true });
            }

            // Nếu ModelState không hợp lệ, trả về form với lỗi
            ViewData["CategoryId"] = new SelectList(
                await _context.Categories.Where(c => (c.UserId == currentUser.Id || c.UserId == null) && c.Type == StaticDetails.Expense).ToListAsync(),
                "Id", "Name",
                model.CategoryId);

            string html = await RenderRazorViewToString("_AddContributionForm", model);
            return Json(new { isValid = false, html = html });
        }

        private bool FinancialGoalExists(int id)
        {
            return _context.FinancialGoals.Any(e => e.Id == id);
        }
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

    }
}
