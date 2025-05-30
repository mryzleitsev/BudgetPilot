using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BudgetPilot.Models;    

namespace BudgetPilot.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        
        public DbSet<Transaction> Transactions { get; set; } = default!;
        public DbSet<Account>     Accounts     { get; set; } = default!;
        public DbSet<Category> Categories { get; set; } = default!;
        
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Ensure decimal precision
            builder.Entity<Account>()
                .Property(a => a.Balance)
                .HasPrecision(18, 2);

            builder.Entity<Transaction>()
                .Property(t => t.Amount)
                .HasPrecision(18, 2);
            
            builder.Entity<Transaction>()
                .Property(t => t.CategoryId)
                .HasDefaultValue(8);
            
            builder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Food" },
                new Category { Id = 2, Name = "Transport" },
                new Category { Id = 3, Name = "Entertainment" },
                new Category { Id = 4, Name = "Utilities" },
                new Category { Id = 5, Name = "Health" },
                new Category { Id = 6, Name = "Shopping" },
                new Category { Id = 7, Name = "Salary" },
                new Category { Id = 8, Name = "Other" }
            );
        }
    }
}