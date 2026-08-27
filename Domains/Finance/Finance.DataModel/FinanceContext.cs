using Finance.DataModel.Models;

using Microsoft.EntityFrameworkCore;

namespace Finance.DataModel
{
    public class FinanceContext(DbContextOptions<FinanceContext> options) : DbContext(options)
    {
        public DbSet<Conto> Conti { get; set; }
        public DbSet<Movimento> Movimenti { get; set; }
        public DbSet<VoceRicorrente> VociRicorrenti { get; set; }

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

            modelBuilder.Entity<Movimento>()
                .HasOne(movimento => movimento.Conto)
                .WithMany(conto => conto.Movimenti)
                .HasForeignKey(movimento => movimento.ContoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Movimento>()
                .HasIndex(movimento => new { movimento.ContoId, movimento.Date, movimento.Id });

            modelBuilder.Entity<VoceRicorrente>()
                .Property(voce => voce.Name)
                .UseCollation("NOCASE");

            modelBuilder.Entity<VoceRicorrente>()
                .HasIndex(voce => new { voce.Name, voce.Index })
                .IsUnique();

            modelBuilder.Entity<VoceRicorrente>()
                .Property(voce => voce.Value)
                .HasConversion(value => decimal.ToInt64(value * 100m), value => value / 100m);
        }
    }
}
