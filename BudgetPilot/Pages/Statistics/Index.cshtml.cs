using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using BudgetPilot.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BudgetPilot.Pages.Statistics
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        public IndexModel(ApplicationDbContext db) => _db = db;

        // 1) Current balances per account:
        public List<AccountBalanceDto> AccountBalances { get; private set; } = new();

        // 2) stats by transactions per month per account (last 12 months)
        public List<MonthlyWalletStatsDto> MonthlyStats { get; private set; } = new();

        // 3) stats by Recurring 
        public List<RecurringWalletStatsDto> RecurringStats { get; private set; } = new();
        
        // 4) Sum for last month
        public List<CurrentMonthTxPerAccountDto> CurrentMonthTxPerAccount { get; private set; } = new();

        // 5) Balance sum
        public List<SumByCurrencyDto> SumByCurrency { get; private set; } = new();

        public async Task OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // -----------------------------------
            // 1) Current balances per account
            // -----------------------------------
            var accounts = await _db.Accounts
                .Where(a => a.OwnerId == userId)
                .AsNoTracking()
                .ToListAsync();

            AccountBalances = accounts
                .Select(a => new AccountBalanceDto
                {
                    AccountId   = a.Id,
                    AccountName = a.Name,
                    Currency    = a.Currency,
                    Balance     = a.Balance
                })
                .ToList();

            // -----------------------------------
            // 2) Transactions per month per account (last 12 months)
            // -----------------------------------
            var oneYearAgo = DateTime.Today.AddMonths(-11);
            var txQuery = _db.Transactions
                .Include(t => t.Account)
                .Where(t => t.Account.OwnerId == userId && t.Date >= oneYearAgo)
                .AsNoTracking();

            var grouped = await txQuery
                .GroupBy(t => new { t.Date.Year, t.Date.Month, t.AccountId, t.Account.Name, t.Account.Currency })
                .Select(g => new
                {
                    Year         = g.Key.Year,
                    Month        = g.Key.Month,
                    AccountId    = g.Key.AccountId,
                    AccountName  = g.Key.Name,
                    Currency     = g.Key.Currency,
                    TotalIncome  = g.Where(x => x.Amount > 0).Sum(x => x.Amount),
                    TotalExpense = g.Where(x => x.Amount < 0).Sum(x => x.Amount)
                })
                .ToListAsync();

            MonthlyStats = grouped
                .Select(g => new MonthlyWalletStatsDto
                {
                    Year         = g.Year,
                    Month        = g.Month,
                    AccountId    = g.AccountId,
                    AccountName  = g.AccountName,
                    Currency     = g.Currency,
                    IncomeSum    = g.TotalIncome,
                    ExpenseSum   = g.TotalExpense
                })
                .OrderBy(s => s.Year)
                .ThenBy(s => s.Month)
                .ToList();

            // -------------------------------------------------
            // 3) Recurring transactions statistics
            // -------------------------------------------------
            var firstOfThisMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var firstOfNextMonth = firstOfThisMonth.AddMonths(1);

            var recurringList = await _db.RecurringTransactions
                .Include(r => r.Account)
                .Where(r => r.Account.OwnerId == userId && r.IsActive)
                .AsNoTracking()
                .ToListAsync();

            RecurringStats = recurringList
                .GroupBy(r => new { r.AccountId, r.Account.Name, r.Account.Currency })
                .Select(g => new RecurringWalletStatsDto
                {
                    AccountId    = g.Key.AccountId,
                    AccountName  = g.Key.Name,
                    Currency     = g.Key.Currency,
                    ThisMonthSum = g
                        .Where(r => r.NextRunDate.Year == firstOfThisMonth.Year
                                 && r.NextRunDate.Month == firstOfThisMonth.Month)
                        .Sum(r => r.Amount),
                    NextMonthSum = g
                        .Where(r => r.NextRunDate.Year == firstOfNextMonth.Year
                                 && r.NextRunDate.Month == firstOfNextMonth.Month)
                        .Sum(r => r.Amount),
                    TotalPlannedInPeriod = g.Sum(r => r.Amount)
                })
                .ToList();

            // -----------------------------------
            // 4) Data for doughnut
            // -----------------------------------
            var startOfMonth = firstOfThisMonth;

            var txThisMonth = await _db.Transactions
                .Include(t => t.Account)
                .Where(t => t.Account.OwnerId == userId && t.Date >= startOfMonth)
                .AsNoTracking()
                .ToListAsync();

            CurrentMonthTxPerAccount = txThisMonth
                .GroupBy(t => new { t.AccountId, t.Account.Name })
                .Select(g => new CurrentMonthTxPerAccountDto
                {
                    AccountId   = g.Key.AccountId,
                    AccountName = g.Key.Name,
                    TotalVolume = g.Sum(t => (t.Amount > 0 ? t.Amount : Math.Abs(t.Amount)))
                })
                .ToList();

            // -----------------------------------
            // 5) sum of currnecy balances by account
            // -----------------------------------
            SumByCurrency = accounts
                .GroupBy(a => a.Currency)
                .Select(g => new SumByCurrencyDto
                {
                    Currency      = g.Key,
                    TotalBalance  = g.Sum(a => a.Balance)
                })
                .ToList();
        }


        // ====== DTO-classes: ======

        public class AccountBalanceDto
        {
            public int AccountId   { get; set; }
            public string AccountName { get; set; } = "";
            public string Currency  { get; set; } = "";
            public decimal Balance  { get; set; }
        }

        public class MonthlyWalletStatsDto
        {
            public int Year         { get; set; }
            public int Month        { get; set; }
            public int AccountId    { get; set; }
            public string AccountName { get; set; } = "";
            public string Currency  { get; set; } = "";
            public decimal IncomeSum  { get; set; }
            public decimal ExpenseSum { get; set; }
        }

        public class RecurringWalletStatsDto
        {
            public int AccountId     { get; set; }
            public string AccountName { get; set; } = "";
            public string Currency   { get; set; } = "";
            public decimal ThisMonthSum       { get; set; }
            public decimal NextMonthSum       { get; set; }
            public decimal TotalPlannedInPeriod { get; set; }
        }

        // ====== НОВЫЕ DTO: ======

        /// <summary>
        /// ststs of the month (transaction volume) and acc
        /// </summary>
        public class CurrentMonthTxPerAccountDto
        {
            public int AccountId   { get; set; }
            public string AccountName { get; set; } = "";
            public decimal TotalVolume { get; set; }
        }

        /// <summary>
        /// Balance by currency
        /// </summary>
        public class SumByCurrencyDto
        {
            public string Currency     { get; set; } = "";
            public decimal TotalBalance { get; set; }
        }
    }
}
