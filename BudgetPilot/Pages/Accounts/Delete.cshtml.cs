using System.Threading.Tasks;
using BudgetPilot.Data;
using BudgetPilot.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BudgetPilot.Pages.Accounts
{
    [Authorize]
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public DeleteModel(ApplicationDbContext db) => _db = db;

        [BindProperty]
        public Account Account { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Account = await _db.Accounts.FindAsync(id)
                      ?? throw new InvalidOperationException("Account not found");
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var acct = await _db.Accounts.FindAsync(id);
            if (acct != null)
            {
                _db.Accounts.Remove(acct);
                await _db.SaveChangesAsync();
            }
            return RedirectToPage("Index");
        }
    }
}