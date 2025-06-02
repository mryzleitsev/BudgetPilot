using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BudgetPilot.Data;
using BudgetPilot.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BudgetPilot.Pages.RecurringTransactions
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public CreateModel(ApplicationDbContext db) => _db = db;

        [BindProperty]
        public RecurringTransaction Recurring { get; set; } = new();

        [BindProperty]
        public bool IsIncome { get; set; } = false;

        public IList<Account>   Accounts   { get; private set; } = new List<Account>();
        public IList<Category>  Categories { get; private set; } = new List<Category>();

        public async Task OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            Accounts = await _db.Accounts
                .Where(a => a.OwnerId == userId)
                .OrderBy(a => a.Name)
                .ToListAsync();

            Categories = await _db.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();

            Recurring.NextRunDate = DateTime.Today;
            Recurring.IsActive    = true;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!ModelState.IsValid)
            {
                await OnGetAsync();
                return Page();
            }

            Recurring.CreatedAt = DateTime.UtcNow;

            if (IsIncome)
                Recurring.Amount = Math.Abs(Recurring.Amount);
            else
                Recurring.Amount = -Math.Abs(Recurring.Amount);

            var account = await _db.Accounts.FindAsync(Recurring.AccountId);
            if (account == null || account.OwnerId != userId)
            {
                ModelState.AddModelError(string.Empty, "Selected account not found or access denied.");
                await OnGetAsync();
                return Page();
            }

            _db.RecurringTransactions.Add(Recurring);
            await _db.SaveChangesAsync();

            return RedirectToPage("Index");
        }
    }
}
