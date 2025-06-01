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

namespace BudgetPilot.Pages.Transactions
{
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public EditModel(ApplicationDbContext db) => _db = db;

        [BindProperty]
        public Transaction Transaction { get; set; } = default!;

        // Добавляем булевое свойство для переключателя доход/расход в форме
        [BindProperty]
        public bool IsIncome { get; set; } = false;

        public IList<Account>   Accounts   { get; private set; } = default!;
        public IList<Category>  Categories { get; private set; } = default!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Подгружаем транзакцию с Account, чтобы узнать старый счёт и старую сумму
            Transaction = await _db.Transactions
                              .Include(t => t.Account)
                              .FirstOrDefaultAsync(t =>
                                  t.Id == id &&
                                  t.Account.OwnerId == userId
                              )
                          ?? throw new InvalidOperationException("Transaction not found or access denied");

            // Установим флажок по значению суммы
            IsIncome = Transaction.Amount >= 0;

            // Подгрузка списков для выпадающих
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

            // Подгружаем списки заново, если валидация не прошла
            Accounts = await _db.Accounts
                .Where(a => a.OwnerId == userId)
                .OrderBy(a => a.Name)
                .ToListAsync();
            Categories = await _db.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // 1) Подгружаем «старую» транзакцию, включая её Account:
            var existing = await _db.Transactions
                .Include(t => t.Account)
                .FirstOrDefaultAsync(t =>
                    t.Id == Transaction.Id &&
                    t.Account.OwnerId == userId
                );

            if (existing == null)
            {
                return NotFound();
            }

            // 2) Откатываем старое изменение баланса:
            existing.Account!.Balance -= existing.Amount;

            // 3) Обновляем саму транзакцию:
            existing.Date        = Transaction.Date;
            existing.Amount      = Transaction.Amount; // пока без учёта IsIncome
            existing.CategoryId  = Transaction.CategoryId;
            existing.Description = Transaction.Description;

            // 4) Устанавливаем знак для новой суммы:
            if (IsIncome)
            {
                existing.Amount = Math.Abs(existing.Amount);
            }
            else
            {
                existing.Amount = -Math.Abs(existing.Amount);
            }

            // 5) Если пользователь сменил кошелёк:
            if (existing.AccountId != Transaction.AccountId)
            {
                var newAccount = await _db.Accounts.FindAsync(Transaction.AccountId);
                if (newAccount == null || newAccount.OwnerId != userId)
                {
                    ModelState.AddModelError(string.Empty, "Selected account not found or access denied.");
                    return Page();
                }

                existing.AccountId = Transaction.AccountId;
                existing.Account   = newAccount!;
            }

            // 6) Применяем баланс (с уже обновлённым existing.Amount):
            existing.Account!.Balance += existing.Amount;

            existing.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return RedirectToPage("Index");
        }
    }
}
