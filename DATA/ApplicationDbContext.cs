using Microsoft.EntityFrameworkCore;
using BilleteraVirtual.API.Models;

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

            // 🔹 Forzar nombres de tabla y columna en minúsculas
            modelBuilder.Entity<User>().ToTable("users");
            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Cedula).HasColumnName("cedula");
                entity.Property(e => e.FirstName).HasColumnName("firstname");
                entity.Property(e => e.Email).HasColumnName("email");
                entity.Property(e => e.Clave).HasColumnName("clave");
            });

            modelBuilder.Entity<Account>().ToTable("accounts");
            modelBuilder.Entity<Account>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.UserId).HasColumnName("userid");
                entity.Property(e => e.Amount).HasColumnName("amount");
                entity.Property(e => e.Status).HasColumnName("status");
            });

            modelBuilder.Entity<Transaction>().ToTable("transactions");
            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.AccountSend).HasColumnName("accountsend");
                entity.Property(e => e.AccountRecived).HasColumnName("accountrecived");
                entity.Property(e => e.Amount).HasColumnName("amount");
                entity.Property(e => e.Status).HasColumnName("status");
            });
        }
    }
}
