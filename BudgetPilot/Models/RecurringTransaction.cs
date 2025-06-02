using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace BudgetPilot.Models
{
    public enum FrequencyType
    {
        Daily = 0,
        Weekly = 1,
        Monthly = 2
    }

    public class RecurringTransaction
    {
        public int Id { get; set; }

        [DataType(DataType.Date)]
        public DateTime NextRunDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public int CategoryId { get; set; }
        [ValidateNever]
        public Category? Category { get; set; }

        [StringLength(200)]
        public string Description { get; set; } = null!;

        public int AccountId { get; set; }
        [ValidateNever]
        public Account? Account { get; set; }

        [Required]
        public FrequencyType Frequency { get; set; } = FrequencyType.Monthly;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastRunDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
