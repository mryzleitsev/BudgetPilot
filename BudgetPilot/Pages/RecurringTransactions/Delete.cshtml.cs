using System;
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
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public DeleteModel(ApplicationDbContext db) => _db = db;

        [BindProperty]
        public RecurringTransaction Recurring { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            Recurring = await _db.RecurringTransactions
                            .Include(r => r.Account)
                            .Include(r => r.Category)
                            .FirstOrDefaultAsync(r =>
                                r.Id == id &&
                                r.Account.OwnerId == userId
                            )
                        ?? throw new InvalidOperationException("Recurring transaction not found or access denied");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var toDelete = await _db.RecurringTransactions
                .Include(r => r.Account)
                .FirstOrDefaultAsync(r =>
                    r.Id == id &&
                    r.Account.OwnerId == userId
                );

            if (toDelete == null)
                return NotFound();

            _db.RecurringTransactions.Remove(toDelete);
            await _db.SaveChangesAsync();

            return RedirectToPage("Index");
        }
    }
}