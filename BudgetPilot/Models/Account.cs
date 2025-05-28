using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace BudgetPilot.Models
{
    public class Account
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = null!;            // e.g. "Cash", "Visa ****1234"

        [Column(TypeName = "decimal(18,2)")]
        public decimal Balance { get; set; }                 // Current balance

        [Required, StringLength(3)]
        public string Currency { get; set; } = "USD";        // ISO code

        [Required, StringLength(50)]
        public string Type { get; set; } = "Cash";           // e.g. "Cash", "Bank", "E-wallet"

        public string? Description { get; set; }             // Optional note

        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // FK to the user who owns this account
        public string OwnerId { get; set; } = null!;
        
        [ValidateNever]
        [ForeignKey(nameof(OwnerId))]
        public IdentityUser? Owner { get; set; }

        // Navigation to transactions
        public ICollection<Transaction> Transactions { get; set; }
            = new List<Transaction>();
    }
}