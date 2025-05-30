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

namespace BudgetPilot.Pages.Transactions
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public IndexModel(ApplicationDbContext db) => _db = db;

        // list of accs
        public IList<Account> Accounts { get; private set; } = default!;

        // chosen acc
        public int? SelectedAccountId { get; set; }

        // Grouping by date
        public List<IGrouping<DateTime, Transaction>> GroupedTransactions { get; private set; }
            = new List<IGrouping<DateTime, Transaction>>();

        public async Task OnGetAsync(int? accountId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            Accounts = await _db.Accounts
                .Where(a => a.OwnerId == userId)
                .OrderBy(a => a.Name)
                .AsNoTracking()
                .ToListAsync();

            var q = _db.Transactions
                .Include(t => t.Account)
                .Include(t => t.Category)
                .Where(t => t.Account.OwnerId == userId);

            if (accountId.HasValue)
            {
                q = q.Where(t => t.AccountId == accountId.Value);
                SelectedAccountId = accountId;
            }

            var all = await q
                .OrderByDescending(t => t.Date)
                .AsNoTracking()
                .ToListAsync();

            GroupedTransactions = all
                .GroupBy(t => t.Date.Date)
                .OrderByDescending(g => g.Key)
                .ToList();
        }
    }
}
