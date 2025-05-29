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

        public IList<Transaction> Transactions { get; private set; } = default!;
        public IList<Account> Accounts             { get; private set; } = default!;
        public int? SelectedAccountId              { get; set; }

        public async Task OnGetAsync(int? accountId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // List of accounts for the current user
            Accounts = await _db.Accounts
                .Where(a => a.OwnerId == userId)
                .OrderBy(a => a.Name)
                .AsNoTracking()
                .ToListAsync();

            // Basic request for transactions
            var q = _db.Transactions
                .Include(t => t.Account)
                .Where(t => t.Account.OwnerId == userId);

            if (accountId.HasValue)
            {
                q = q.Where(t => t.AccountId == accountId.Value);
                SelectedAccountId = accountId;
            }

            Transactions = await q
                .OrderByDescending(t => t.Date)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}