// Pages/Accounts/Create.cshtml.cs
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
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BudgetPilot.Pages.Accounts
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public CreateModel(ApplicationDbContext db) => _db = db;

        [BindProperty]
        public Account Account { get; set; } = new();

        /// <summary>
        /// Список валют (ISO-код + символ) для выпадающего списка
        /// </summary>
        public List<SelectListItem> CurrencyOptions { get; private set; } = new();

        public void OnGet()
        {
            // Проставляем дефолтную дату и OwnerId потом, перед сохранением
            // Но подгружаем список валют сюда
            CurrencyOptions = _allCurrencies
                .Select(c => new SelectListItem 
                { 
                    Value = c.Code, 
                    Text = $"{c.Code} ({c.Symbol})" 
                })
                .ToList();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Подгружаем список снова, чтобы форма могла его отобразить, если ModelState невалиден
            CurrencyOptions = _allCurrencies
                .Select(c => new SelectListItem 
                { 
                    Value = c.Code, 
                    Text = $"{c.Code} ({c.Symbol})" 
                })
                .ToList();

            // Проставляем owner и время вручную
            Account.OwnerId   = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Account.CreatedAt = DateTime.UtcNow;

            // Убираем OwnerId из валидации, так как оно не вводится в форме
            ModelState.Remove(nameof(Account.OwnerId));
            ModelState.Remove($"Account.{nameof(Account.OwnerId)}");

            if (!ModelState.IsValid)
            {
                return Page();
            }

            _db.Accounts.Add(Account);
            await _db.SaveChangesAsync();
            return RedirectToPage("Index");
        }

        /// <summary>
        /// Жёстко закодированный набор ISO-код + символ.
        /// </summary>
        private static readonly (string Code, string Symbol)[] _allCurrencies = new[]
        {
            ("USD", "$"),
            ("EUR", "€"),
            ("GBP", "£"),
            ("JPY", "¥"),
            ("CHF", "CHF"),
            ("CNY", "¥"),
            ("CAD", "CA$"),
            ("AUD", "A$"),
            ("UAH", "₴")
        };
    }
}
