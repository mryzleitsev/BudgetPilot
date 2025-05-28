using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using BudgetPilot.Data;
using BudgetPilot.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BudgetPilot.Pages.Accounts
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public IndexModel(ApplicationDbContext db) => _db = db;

        public IList<Account> Accounts { get; private set; } = Array.Empty<Account>();
        public decimal TotalBalance { get; private set; }

        public async Task OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Accounts = await _db.Accounts
                .Where(a => a.OwnerId == userId)
                .OrderBy(a => a.Name)
                .AsNoTracking()
                .ToListAsync();

            TotalBalance = Accounts.Sum(a => a.Balance);
        }
    }
}