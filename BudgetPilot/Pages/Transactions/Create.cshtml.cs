using System;
using System.Collections.Generic;
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
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public CreateModel(ApplicationDbContext db) => _db = db;

        [BindProperty]
        public Transaction Transaction { get; set; } = new();

        public IList<Account> Accounts { get; private set; } = default!;
        public IList<Category>  Categories { get; private set; } = default!;

        public async Task OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Accounts = await _db.Accounts
                .Where(a => a.OwnerId == userId)
                .ToListAsync();
            Categories = await _db.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();
            
            Transaction.Date = DateTime.Today;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync();
                return Page();
            }

            Transaction.CreatedAt = DateTime.UtcNow;
            _db.Transactions.Add(Transaction);
            await _db.SaveChangesAsync();
            return RedirectToPage("Index");
        }
    }
}