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
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public EditModel(ApplicationDbContext db) => _db = db;

        [BindProperty]
        public Transaction Transaction { get; set; } = default!;
        public IList<Category> Categories { get; private set; } = default!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            Transaction = await _db.Transactions
                              .Include(t => t.Account)
                              .Where(t => t.Account.OwnerId == userId)
                              .FirstOrDefaultAsync(t => t.Id == id)
                          ?? throw new InvalidOperationException("Transaction not found or access denied");
            
            Categories = await _db.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                Categories = await _db.Categories
                    .OrderBy(c => c.Name)
                    .ToListAsync();
                return Page();
            }
            
            _db.Attach(Transaction).State = EntityState.Modified;
            Transaction.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return RedirectToPage("Index");
        }
    }
}