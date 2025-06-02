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
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BudgetPilot.Pages.Transactions
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public CreateModel(ApplicationDbContext db) => _db = db;

        [BindProperty]
        public Transaction Transaction { get; set; } = new();

        // New boolean property: if true - income; if false - expense
        [BindProperty]
        public bool IsIncome { get; set; } = false;

        public IList<Account> Accounts     { get; private set; } = default!;
        public IList<Category> Categories { get; private set; } = default!;

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
            
            Transaction.Date = DateTime.Today;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (!ModelState.IsValid)
            {
                await OnGetAsync();
                return Page();
            }
            
            Transaction.CreatedAt = DateTime.UtcNow;
            
            if (IsIncome)
            {
                Transaction.Amount = Math.Abs(Transaction.Amount);
            }
            else
            {
                Transaction.Amount = -Math.Abs(Transaction.Amount);
            }

            var account = await _db.Accounts.FindAsync(Transaction.AccountId);
            if (account == null || account.OwnerId != userId)
            {
                ModelState.AddModelError(string.Empty, "Selected account not found or access denied.");
                await OnGetAsync();
                return Page();
            }

            account.Balance += Transaction.Amount;

            _db.Transactions.Add(Transaction);
            await _db.SaveChangesAsync();

            return RedirectToPage("Index");
        }
    }
}
