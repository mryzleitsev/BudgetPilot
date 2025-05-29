using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace BudgetPilot.Models
{
    public class Transaction
    {
        public int Id { get; set; }

        public DateTime Date { get; set; }                   // When it happened

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }                  // Fixed precision

        public string Category { get; set; } = null!;        // e.g. "Food", "Salary"
        public string Description { get; set; } = null!;     // Note

        // Link to Account
        [ValidateNever]
        [ForeignKey(nameof(AccountId))]
        public int AccountId { get; set; }
        public Account? Account { get; set; } = null!;

        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}