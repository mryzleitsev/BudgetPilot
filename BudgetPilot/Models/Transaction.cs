using System;

namespace BudgetPilot.Models
{
    public class Transaction
    {
        public int     Id          { get; set; }   // PK
        public DateTime Date        { get; set; }  // Date
        public decimal Amount      { get; set; }  // Sum
        public string  Category    { get; set; } = null!;  // Category
        public string  Description { get; set; } = null!;  // Description
    }
}