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

        // Список «плановых» транзакций
        public List<RecurringTransaction> RecurringList { get; private set; } = new();

        // Список кошельков для фильтрации
        public List<Account> Accounts { get; private set; } = new();

        // Выбранный фильтр по кошельку (nullable)
        public int? SelectedAccountId { get; set; }

        public async Task OnGetAsync(int? accountId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // 1) Кошельки
            Accounts = await _db.Accounts
                .Where(a => a.OwnerId == userId)
                .OrderBy(a => a.Name)
                .AsNoTracking()
                .ToListAsync();

            // 2) Базовый запрос «плановых» транзакций пользователя
            var query = _db.RecurringTransactions
                .Include(r => r.Account)
                .Include(r => r.Category)
                .Where(r => r.Account.OwnerId == userId);

            // фильтрация по accountId
            if (accountId.HasValue)
            {
                query = query.Where(r => r.AccountId == accountId.Value);
                SelectedAccountId = accountId;
            }

            // 3) Сортировка по NextRunDate
             RecurringList = await query
                .OrderBy(r => r.NextRunDate)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}