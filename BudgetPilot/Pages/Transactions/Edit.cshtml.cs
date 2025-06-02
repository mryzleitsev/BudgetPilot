using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BudgetPilot.Data;
using BudgetPilot.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BudgetPilot.Pages.Transactions
{
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public EditModel(ApplicationDbContext db) => _db = db;

        [BindProperty]
        public Transaction Transaction { get; set; } = default!;

        [BindProperty]
        public bool IsIncome { get; set; } = false;

        public IList<Account>   Accounts   { get; private set; } = default!;
        public IList<Category>  Categories { get; private set; } = default!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            Transaction = await _db.Transactions
                              .Include(t => t.Account)
                              .FirstOrDefaultAsync(t =>
                                  t.Id == id &&
                                  t.Account.OwnerId == userId
                              )
                          ?? throw new InvalidOperationException("Transaction not found or access denied");

            IsIncome = Transaction.Amount >= 0;

            Accounts = await _db.Accounts
                .Where(a => a.OwnerId == userId)
                .OrderBy(a => a.Name)
                .ToListAsync();
            Categories = await _db.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            Accounts = await _db.Accounts
                .Where(a => a.OwnerId == userId)
                .OrderBy(a => a.Name)
                .ToListAsync();
            Categories = await _db.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var existing = await _db.Transactions
                .Include(t => t.Account)
                .FirstOrDefaultAsync(t =>
                    t.Id == Transaction.Id &&
                    t.Account.OwnerId == userId
                );

            if (existing == null)
            {
                return NotFound();
            }

            existing.Account!.Balance -= existing.Amount;

            existing.Date        = Transaction.Date;
            existing.Amount      = Transaction.Amount; 
            existing.CategoryId  = Transaction.CategoryId;
            existing.Description = Transaction.Description;

            if (IsIncome)
            {
                existing.Amount = Math.Abs(existing.Amount);
            }
            else
            {
                existing.Amount = -Math.Abs(existing.Amount);
            }

            if (existing.AccountId != Transaction.AccountId)
            {
                var newAccount = await _db.Accounts.FindAsync(Transaction.AccountId);
                if (newAccount == null || newAccount.OwnerId != userId)
                {
                    ModelState.AddModelError(string.Empty, "Selected account not found or access denied.");
                    return Page();
                }

                existing.AccountId = Transaction.AccountId;
                existing.Account   = newAccount!;
            }

            existing.Account!.Balance += existing.Amount;

            existing.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return RedirectToPage("Index");
        }
    }
}
