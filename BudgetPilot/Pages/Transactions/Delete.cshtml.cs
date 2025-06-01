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
                              .Include(t => t.Category)
                              .FirstOrDefaultAsync(t =>
                                  t.Id == id &&
                                  t.Account.OwnerId == userId
                              )
                          ?? throw new InvalidOperationException("Transaction not found or access denied");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Подгружаем саму транзакцию вместе с её Account
            var toDelete = await _db.Transactions
                .Include(t => t.Account)
                .FirstOrDefaultAsync(t =>
                    t.Id == id &&
                    t.Account.OwnerId == userId
                );

            if (toDelete == null)
            {
                return NotFound();
            }

            // «Откатываем» изменение баланса:
            toDelete.Account!.Balance -= toDelete.Amount;

            // Удаляем транзакцию
            _db.Transactions.Remove(toDelete);
            await _db.SaveChangesAsync();

            return RedirectToPage("Index");
        }
    }
}
