using System;
using System.Security.Claims;
using System.Threading.Tasks;
using BudgetPilot.Data;
using BudgetPilot.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BudgetPilot.Pages.Accounts
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public CreateModel(ApplicationDbContext db) => _db = db;

        [BindProperty]
        public Account Account { get; set; } = new();

        public IActionResult OnGet() => Page();

        public async Task<IActionResult> OnPostAsync()
        {
            Account.OwnerId   = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Account.CreatedAt = DateTime.UtcNow;

            ModelState.Remove(nameof(Account.OwnerId));
            ModelState.Remove($"Account.{nameof(Account.OwnerId)}");

            if (!ModelState.IsValid)
                return Page();

            _db.Accounts.Add(Account);
            await _db.SaveChangesAsync();
            return RedirectToPage("Index");
        }

    }
}