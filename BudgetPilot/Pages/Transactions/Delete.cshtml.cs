using System;
using System.Security.Claims;
using System.Threading.Tasks;
using BudgetPilot.Data;
using BudgetPilot.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BudgetPilot.Pages.Transactions
{
    [Authorize]
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public DeleteModel(ApplicationDbContext db) => _db = db;

        [BindProperty]
        public Transaction Transaction { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            Transaction = await _db.Transactions
                              .Include(t => t.Account)
                              .Where(t => t.Account.OwnerId == userId)
                              .FirstOrDefaultAsync(t => t.Id == id)
                          ?? throw new InvalidOperationException("Transaction not found or access denied");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var trx = await _db.Transactions.FindAsync(id);
            if (trx != null)
            {
                _db.Transactions.Remove(trx);
                await _db.SaveChangesAsync();
            }
            return RedirectToPage("Index");
        }
    }
}