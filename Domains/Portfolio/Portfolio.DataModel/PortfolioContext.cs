using Microsoft.EntityFrameworkCore;

using Portfolio.DataModel.Models;

namespace Portfolio.DataModel
{
    public class PortfolioContext(DbContextOptions<PortfolioContext> options) : DbContext(options)
    {
        public DbSet<Album> Albums { get; set; }
        public DbSet<Foto> Foto { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Album>()
                .HasOne(a => a.Parent)
                .WithMany(a => a.Children)
                .HasForeignKey(a => a.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Album>()
                .HasIndex(a => new { a.ParentId, a.Path })
                .IsUnique();

            modelBuilder.Entity<Foto>()
                .HasOne(f => f.Album)
                .WithMany(f => f.Photos)
                .HasForeignKey(f => f.AlbumId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Foto>()
                .HasIndex(f => new { f.AlbumId, f.FileName })
                .IsUnique();
        }
    }
}
