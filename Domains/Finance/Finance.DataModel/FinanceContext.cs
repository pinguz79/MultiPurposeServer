using Finance.DataModel.Models;

using Microsoft.EntityFrameworkCore;

namespace Finance.DataModel
{
    public class FinanceContext(DbContextOptions<FinanceContext> options) : DbContext(options)
    {
        public DbSet<Conto> Conti { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Conto>()
                .Property(conto => conto.Name)
                .UseCollation("NOCASE");

            modelBuilder.Entity<Conto>()
                .HasIndex(conto => conto.Name)
                .IsUnique();

            modelBuilder.Entity<Conto>()
                .Property(conto => conto.InitialBalance)
                .HasConversion(value => decimal.ToInt64(value * 100m), value => value / 100m);
        }
    }
}
