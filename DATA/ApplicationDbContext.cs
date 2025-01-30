using Microsoft.EntityFrameworkCore;
using BilleteraVirtual.API.Models;

namespace BilleteraVirtual.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Account> Accounts { get; set; }  // 🔹 Se mantiene PascalCase aquí
        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Account>().ToTable("accounts");  // 🔹 Nombre exacto de la tabla
            modelBuilder.Entity<Account>()
                .Property(a => a.Id)
                .HasColumnName("id");  // 🔹 Forzamos que se use "id" en PostgreSQL

            modelBuilder.Entity<Account>()
                .Property(a => a.UserId)
                .HasColumnName("userid");  // 🔹 Forzamos "userid"

            modelBuilder.Entity<Account>()
                .Property(a => a.Amount)
                .HasColumnName("amount");  // 🔹 Forzamos "amount"

            modelBuilder.Entity<Account>()
                .Property(a => a.Status)
                .HasColumnName("status");  // 🔹 Forzamos "status"
        }
    }
}
