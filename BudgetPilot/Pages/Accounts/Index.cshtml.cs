using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
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

        // List all wallets belonging to the current user:
        public IList<Account> Accounts { get; private set; } = null!;

        // Dictionary where Key = "USD", "EUR", etc., Value = total balance of that currency:
        public IDictionary<string, decimal> TotalByCurrency { get; private set; } = new Dictionary<string, decimal>();

        public async Task OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // 1) Load all accounts for this user:
            Accounts = await _db.Accounts
                .Where(a => a.OwnerId == userId)
                .OrderBy(a => a.Name)
                .AsNoTracking()
                .ToListAsync();

            // 2) Compute a per-currency sum:
            TotalByCurrency = await _db.Accounts
                .Where(a => a.OwnerId == userId)
                .GroupBy(a => a.Currency)
                .Select(g => new
                {
                    Currency = g.Key,
                    Sum = g.Sum(a => a.Balance)
                })
                .ToDictionaryAsync(x => x.Currency, x => x.Sum);
        }
    }
}