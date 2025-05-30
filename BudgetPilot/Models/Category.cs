using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BudgetPilot.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string Name { get; set; } = null!;

        // Nav to transactions
        public ICollection<Transaction> Transactions { get; set; }
            = new List<Transaction>();
    }
}