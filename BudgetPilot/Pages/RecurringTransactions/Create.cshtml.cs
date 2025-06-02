using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BudgetPilot.Data;
using BudgetPilot.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BudgetPilot.Pages.RecurringTransactions
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public CreateModel(ApplicationDbContext db) => _db = db;

        [BindProperty]
        public RecurringTransaction Recurring { get; set; } = new();

        // Булево поле «доход/расход»
        [BindProperty]
        public bool IsIncome { get; set; } = false;

        public IList<Account>   Accounts   { get; private set; } = new List<Account>();
        public IList<Category>  Categories { get; private set; } = new List<Category>();

        public async Task OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Подгружаем кошельки текущего пользователя
            Accounts = await _db.Accounts
                .Where(a => a.OwnerId == userId)
                .OrderBy(a => a.Name)
                .ToListAsync();

            // Подгружаем категории
            Categories = await _db.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();

            // «По умолчанию» — сегодняшняя дата и активность = true
            Recurring.NextRunDate = DateTime.Today;
            Recurring.IsActive    = true;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Если валидация не прошла
            if (!ModelState.IsValid)
            {
                await OnGetAsync();
                return Page();
            }

            // Ставим CreatedAt
            Recurring.CreatedAt = DateTime.UtcNow;

            // 1) Выставляем знак суммы
            if (IsIncome)
                Recurring.Amount = Math.Abs(Recurring.Amount);
            else
                Recurring.Amount = -Math.Abs(Recurring.Amount);

            // 2) Проверяем аккаунт
            var account = await _db.Accounts.FindAsync(Recurring.AccountId);
            if (account == null || account.OwnerId != userId)
            {
                ModelState.AddModelError(string.Empty, "Selected account not found or access denied.");
                await OnGetAsync();
                return Page();
            }

            // 3) Сохраняем запись (баланс аккаунта при этом не меняем)
            _db.RecurringTransactions.Add(Recurring);
            await _db.SaveChangesAsync();

            return RedirectToPage("Index");
        }
    }
}
