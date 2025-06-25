using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BudgetTracker.Data;
using BudgetTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace BudgetTracker.Controllers
{
    [Authorize]
    public class RecurringTransactionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public RecurringTransactionsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: RecurringTransactions
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var recurringTransactions = await _context.RecurringTransactions
                .Include(r => r.Category)
                .Where(r => r.UserId == currentUser.Id)
                .OrderBy(r => r.DayOfMonth)
                .ToListAsync();
            return View(recurringTransactions);
        }

        // GET: RecurringTransactions/Create
        public async Task<IActionResult> Create()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            ViewData["CategoryId"] = new SelectList(await _context.Categories.Where(c => c.UserId == currentUser.Id || c.UserId == null).ToListAsync(), "Id", "Name");
            return View();
        }

        // POST: RecurringTransactions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Description,Amount,DayOfMonth,CategoryId")] RecurringTransaction recurringTransaction)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            recurringTransaction.UserId = currentUser.Id;

            ModelState.Remove("User");
            ModelState.Remove("UserId");
            ModelState.Remove("Category");

            if (ModelState.IsValid)
            {
                _context.Add(recurringTransaction);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoryId"] = new SelectList(
                await _context.Categories.Where(c => c.UserId == currentUser.Id).ToListAsync(),
                "Id", "Name",
                recurringTransaction.CategoryId);
            return View(recurringTransaction);
        }

        // GET: RecurringTransactions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            var recurringTransaction = await _context.RecurringTransactions
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == currentUser.Id);

            if (recurringTransaction == null) return NotFound();

            ViewData["CategoryId"] = new SelectList(await _context.Categories.Where(c => c.UserId == currentUser.Id || c.UserId == null).ToListAsync(), "Id", "Name", recurringTransaction.CategoryId);
            return View(recurringTransaction);
        }

        // POST: RecurringTransactions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Description,Amount,DayOfMonth,CategoryId")] RecurringTransaction recurringTransaction)
        {
            if (id != recurringTransaction.Id) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            var originalTransaction = await _context.RecurringTransactions
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == currentUser.Id);

            if (originalTransaction == null) return NotFound();

            recurringTransaction.UserId = originalTransaction.UserId;
            recurringTransaction.LastProcessedDate = originalTransaction.LastProcessedDate;

            ModelState.Remove("User");
            ModelState.Remove("UserId");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(recurringTransaction);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RecurringTransactionExists(recurringTransaction.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(
                await _context.Categories.Where(c => c.UserId == currentUser.Id).ToListAsync(),
                "Id", "Name",
                recurringTransaction.CategoryId);
            return View(recurringTransaction);
        }

        // GET: RecurringTransactions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            var recurringTransaction = await _context.RecurringTransactions
                .Include(r => r.Category)
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == currentUser.Id);
            if (recurringTransaction == null) return NotFound();

            return View(recurringTransaction);
        }

        // POST: RecurringTransactions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var recurringTransaction = await _context.RecurringTransactions
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == currentUser.Id);

            if (recurringTransaction != null)
            {
                _context.RecurringTransactions.Remove(recurringTransaction);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var recurringTransaction = await _context.RecurringTransactions
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == currentUser.Id);

            if (recurringTransaction != null)
            {
                recurringTransaction.IsActive = !recurringTransaction.IsActive; // Đảo ngược trạng thái
                _context.Update(recurringTransaction);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool RecurringTransactionExists(int id)
        {
            return _context.RecurringTransactions.Any(e => e.Id == id);
        }
    }
}