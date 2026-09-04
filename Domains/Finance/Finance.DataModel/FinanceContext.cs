using Finance.DataModel.Models;

using Microsoft.EntityFrameworkCore;

namespace Finance.DataModel
{
    public class FinanceContext(DbContextOptions<FinanceContext> options) : DbContext(options)
    {
        public DbSet<Categoria> Categorie { get; set; }
        public DbSet<Conto> Conti { get; set; }
        public DbSet<CorrelazionePianificazione> CorrelazioniPianificazioni { get; set; }
        public DbSet<Movimento> Movimenti { get; set; }
        public DbSet<ParametroConto> ParametriConto { get; set; }
        public DbSet<Periodicita> Periodicita { get; set; }
        public DbSet<Pianificazione> Pianificazioni { get; set; }
        public DbSet<VoceRicorrente> VociRicorrenti { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Categoria>()
                .Property(categoria => categoria.Name)
                .UseCollation("NOCASE");

            modelBuilder.Entity<Categoria>()
                .HasIndex(categoria => categoria.Name)
                .IsUnique();

            modelBuilder.Entity<Conto>()
                .Property(conto => conto.Name)
                .UseCollation("NOCASE");

            modelBuilder.Entity<Conto>()
                .HasIndex(conto => conto.Name)
                .IsUnique();

            modelBuilder.Entity<Conto>()
                .Property(conto => conto.InitialBalance)
                .HasConversion(value => decimal.ToInt64(value * 100m), value => value / 100m);

            modelBuilder.Entity<CorrelazionePianificazione>()
                .HasIndex(correlazione => new { correlazione.PianificazioneAId, correlazione.PianificazioneBId })
                .IsUnique();

            modelBuilder.Entity<CorrelazionePianificazione>()
                .HasOne(correlazione => correlazione.PianificazioneA)
                .WithMany(pianificazione => pianificazione.CorrelazioniComeA)
                .HasForeignKey(correlazione => correlazione.PianificazioneAId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CorrelazionePianificazione>()
                .HasOne(correlazione => correlazione.PianificazioneB)
                .WithMany(pianificazione => pianificazione.CorrelazioniComeB)
                .HasForeignKey(correlazione => correlazione.PianificazioneBId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Movimento>()
                .HasOne(movimento => movimento.Conto)
                .WithMany(conto => conto.Movimenti)
                .HasForeignKey(movimento => movimento.ContoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Movimento>()
                .HasIndex(movimento => new { movimento.ContoId, movimento.Date, movimento.Id });

            modelBuilder.Entity<Movimento>()
                .HasOne(movimento => movimento.Categoria)
                .WithMany(categoria => categoria.Movimenti)
                .HasForeignKey(movimento => movimento.CategoriaId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Movimento>()
                .HasOne(movimento => movimento.Pianificazione)
                .WithMany(pianificazione => pianificazione.Movimenti)
                .HasForeignKey(movimento => movimento.PianificazioneId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Pianificazione>()
                .HasOne(pianificazione => pianificazione.Conto)
                .WithMany()
                .HasForeignKey(pianificazione => pianificazione.ContoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Pianificazione>()
                .HasOne(pianificazione => pianificazione.Categoria)
                .WithMany(categoria => categoria.Pianificazioni)
                .HasForeignKey(pianificazione => pianificazione.CategoriaId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Pianificazione>()
                .HasOne(pianificazione => pianificazione.Periodicita)
                .WithMany(periodicita => periodicita.Pianificazioni)
                .HasForeignKey(pianificazione => pianificazione.PeriodicitaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Periodicita>()
                .HasIndex(periodicita => new
                {
                    periodicita.Frequenza,
                    periodicita.Intervallo,
                    periodicita.GiornoSettimana,
                    periodicita.SettimanaMese,
                    periodicita.GiornoMese,
                    periodicita.MeseAnno,
                    periodicita.FineMese,
                })
                .IsUnique();

            modelBuilder.Entity<ParametroConto>()
                .Property(parametro => parametro.Name)
                .UseCollation("NOCASE");

            modelBuilder.Entity<ParametroConto>()
                .HasIndex(parametro => new { parametro.ContoId, parametro.Name, parametro.Index })
                .IsUnique();

            modelBuilder.Entity<ParametroConto>()
                .HasOne(parametro => parametro.Conto)
                .WithMany(conto => conto.Parametri)
                .HasForeignKey(parametro => parametro.ContoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<VoceRicorrente>()
                .Property(voce => voce.Name)
                .UseCollation("NOCASE");

            modelBuilder.Entity<VoceRicorrente>()
                .HasIndex(voce => new { voce.Name, voce.Index })
                .IsUnique();

            modelBuilder.Entity<VoceRicorrente>()
                .Property(voce => voce.Value)
                .HasConversion(value => decimal.ToInt64(value * 100m), value => value / 100m);

            modelBuilder.Entity<VoceRicorrente>()
                .HasOne(voce => voce.Categoria)
                .WithMany(categoria => categoria.VociRicorrenti)
                .HasForeignKey(voce => voce.CategoriaId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
