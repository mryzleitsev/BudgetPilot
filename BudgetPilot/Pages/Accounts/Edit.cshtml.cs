// Pages/Accounts/Edit.cshtml.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetPilot.Data;
using BudgetPilot.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BudgetPilot.Pages.Accounts
{
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public EditModel(ApplicationDbContext db) => _db = db;

        [BindProperty]
        public Account Account { get; set; } = default!;

        /// <summary>
        /// Список валют (ISO-код + символ) для select
        /// </summary>
        public List<SelectListItem> CurrencyOptions { get; private set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Account = await _db.Accounts.FindAsync(id)
                      ?? throw new InvalidOperationException("Account not found");

            CurrencyOptions = _allCurrencies
                .Select(c => new SelectListItem 
                { 
                    Value = c.Code, 
                    Text = $"{c.Code} ({c.Symbol})",
                    Selected = (c.Code == Account.Currency)
                })
                .ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            CurrencyOptions = _allCurrencies
                .Select(c => new SelectListItem 
                { 
                    Value = c.Code, 
                    Text = $"{c.Code} ({c.Symbol})",
                    Selected = (c.Code == Account.Currency)
                })
                .ToList();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var existing = await _db.Accounts.FindAsync(Account.Id);
            if (existing == null) return NotFound();

            existing.Name        = Account.Name;
            existing.Balance     = Account.Balance;
            existing.Currency    = Account.Currency;
            existing.Type        = Account.Type;
            existing.Description = Account.Description;
            existing.UpdatedAt   = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return RedirectToPage("Index");
        }

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
