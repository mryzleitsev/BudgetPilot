using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BudgetPilot.Data;
using BudgetPilot.Models;               
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BudgetPilot.Pages.RecurringTransactions
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public IndexModel(ApplicationDbContext db) => _db = db;

        public List<RecurringTransaction> RecurringList { get; private set; } = new();

        public List<Account> Accounts { get; private set; } = new();

        public int? SelectedAccountId { get; set; }

        public async Task OnGetAsync(int? accountId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            Accounts = await _db.Accounts
                .Where(a => a.OwnerId == userId)
                .OrderBy(a => a.Name)
                .AsNoTracking()
                .ToListAsync();

            var query = _db.RecurringTransactions
                .Include(r => r.Account)
                .Include(r => r.Category)
                .Where(r => r.Account.OwnerId == userId);

            if (accountId.HasValue)
            {
                query = query.Where(r => r.AccountId == accountId.Value);
                SelectedAccountId = accountId;
            }

             RecurringList = await query
                .OrderBy(r => r.NextRunDate)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}