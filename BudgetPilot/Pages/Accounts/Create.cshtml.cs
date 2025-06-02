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
        /// List of currnecies (ISO-code + symbol) for list
        /// </summary>
        public List<SelectListItem> CurrencyOptions { get; private set; } = new();

        public void OnGet()
        {
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
            CurrencyOptions = _allCurrencies
                .Select(c => new SelectListItem 
                { 
                    Value = c.Code, 
                    Text = $"{c.Code} ({c.Symbol})" 
                })
                .ToList();

            Account.OwnerId   = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Account.CreatedAt = DateTime.UtcNow;

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
        /// Currency ISO-code + sumbol.
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
