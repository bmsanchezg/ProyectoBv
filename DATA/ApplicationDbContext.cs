using Microsoft.EntityFrameworkCore;
using BilleteraVirtual.API.Models;
using System.Security.Principal;

namespace BilleteraVirtual.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Account>()
                .ToTable(tb => tb.HasCheckConstraint("CK_Account_Id", "id BETWEEN 100000 AND 999999"));

            modelBuilder.Entity<Transaction>()
                .ToTable(tb => tb.HasCheckConstraint("CK_Transaction_Id", "id BETWEEN 10000000 AND 99999999"));
        }
    }
}
