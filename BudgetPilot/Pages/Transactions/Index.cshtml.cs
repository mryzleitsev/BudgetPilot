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

        public IList<Transaction> Transactions { get; private set; } = default!;
        public IList<Account>     Accounts     { get; private set; } = default!;
        public IList<Category>    Categories   { get; private set; } = default!;
        public int? SelectedAccountId            { get; set; }

        public async Task OnGetAsync(int? accountId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Подгружаем кошельки текущего пользователя
            Accounts = await _db.Accounts
                .Where(a => a.OwnerId == userId)
                .OrderBy(a => a.Name)
                .AsNoTracking()
                .ToListAsync();

            // Подгружаем категории (для отображения имени категории)
            Categories = await _db.Categories
                .OrderBy(c => c.Name)
                .AsNoTracking()
                .ToListAsync();

            // Базовый запрос: все транзакции текущего пользователя
            var q = _db.Transactions
                .Include(t => t.Account)
                .Include(t => t.Category)
                .Where(t => t.Account.OwnerId == userId);

            if (accountId.HasValue)
            {
                q = q.Where(t => t.AccountId == accountId.Value);
                SelectedAccountId = accountId;
            }

            // Сразу упорядочим по дате desc
            Transactions = await q
                .OrderByDescending(t => t.Date)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
