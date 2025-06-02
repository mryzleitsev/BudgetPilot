using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BudgetPilot.Data;
using BudgetPilot.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BudgetPilot.Pages.RecurringTransactions
{
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public EditModel(ApplicationDbContext db) => _db = db;

        [BindProperty]
        public RecurringTransaction Recurring { get; set; } = default!;

        // Если true — доход, иначе — расход
        [BindProperty]
        public bool IsIncome { get; set; } = false;

        public IList<Account>   Accounts   { get; private set; } = new List<Account>();
        public IList<Category>  Categories { get; private set; } = new List<Category>();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // 1) Подгружаем «плановую» транзакцию вместе с Объектами Account и Category
            Recurring = await _db.RecurringTransactions
                .Include(r => r.Account)
                .Include(r => r.Category)
                .FirstOrDefaultAsync(r =>
                    r.Id == id &&
                    r.Account.OwnerId == userId
                )
                ?? throw new InvalidOperationException("Recurring transaction not found or access denied");

            // 2) Устанавливаем флажок «доход/расход» по знаку суммы
            IsIncome = Recurring.Amount >= 0;

            // 3) Подгружаем списки для dropdown-ов
            Accounts = await _db.Accounts
                .Where(a => a.OwnerId == userId)
                .OrderBy(a => a.Name)
                .ToListAsync();

            Categories = await _db.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Если валидация не прошла — заново подгружаем списки и возвращаемся
            if (!ModelState.IsValid)
            {
                Accounts = await _db.Accounts
                    .Where(a => a.OwnerId == userId)
                    .OrderBy(a => a.Name)
                    .ToListAsync();

                Categories = await _db.Categories
                    .OrderBy(c => c.Name)
                    .ToListAsync();

                return Page();
            }

            // 1) Подгружаем «старую» сущность из БД вместе с её Account
            var existing = await _db.RecurringTransactions
                .Include(r => r.Account)
                .FirstOrDefaultAsync(r =>
                    r.Id == Recurring.Id &&
                    r.Account.OwnerId == userId
                );

            if (existing == null)
                return NotFound();

            // 2) Обновляем поля
            existing.NextRunDate = Recurring.NextRunDate;
            existing.Frequency   = Recurring.Frequency;
            existing.Description = Recurring.Description;

            // 3) Устанавливаем знак суммы
            if (IsIncome)
                existing.Amount = Math.Abs(Recurring.Amount);
            else
                existing.Amount = -Math.Abs(Recurring.Amount);

            // 4) Обновляем категорию
            existing.CategoryId = Recurring.CategoryId;

            // 5) Если пользователь сменил кошелёк
            if (existing.AccountId != Recurring.AccountId)
            {
                var newAccount = await _db.Accounts.FindAsync(Recurring.AccountId);
                if (newAccount == null || newAccount.OwnerId != userId)
                {
                    ModelState.AddModelError(string.Empty, "Selected account not found or access denied.");

                    // Перезагрузим списки и вернёмся
                    Accounts = await _db.Accounts
                        .Where(a => a.OwnerId == userId)
                        .OrderBy(a => a.Name)
                        .ToListAsync();

                    Categories = await _db.Categories
                        .OrderBy(c => c.Name)
                        .ToListAsync();

                    return Page();
                }

                existing.AccountId = newAccount.Id;
                existing.Account   = newAccount;
            }

            // 6) Активность
            existing.IsActive = Recurring.IsActive;

            await _db.SaveChangesAsync();
            return RedirectToPage("Index");
        }
    }
}
