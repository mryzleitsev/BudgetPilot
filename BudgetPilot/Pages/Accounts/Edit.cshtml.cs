using System;
using System.Threading.Tasks;
using BudgetPilot.Data;
using BudgetPilot.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BudgetPilot.Pages.Accounts
{
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public EditModel(ApplicationDbContext db) => _db = db;

        [BindProperty]
        public Account Account { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Account = await _db.Accounts.FindAsync(id)
                      ?? throw new InvalidOperationException("Account not found");
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            // find an existing record
            var acct = await _db.Accounts.FindAsync(Account.Id);
            if (acct == null) return NotFound();

            // update only the necessary properties
            acct.Balance   = Account.Balance;
            acct.Name      = Account.Name;
            acct.Currency  = Account.Currency;
            acct.Type      = Account.Type;
            acct.Description = Account.Description;
            acct.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return RedirectToPage("Index");
        }
    }
}