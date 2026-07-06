using Microsoft.EntityFrameworkCore;
using MultiPurposeServer.DBContexts.Portfolio;
using MultiPurposeServer.Models.Portfolio;

namespace MultiPurposeServer.Repositories.Portfolio
{
    public class AlbumRepository(PortfolioContext db) : IAlbumRepository
    {
        public async Task<Album> CreateAlbum(string name, Guid? parent, string? path = null)
        {
            var album = new Album { Name = name, ParentId = parent, Path = path };

            db.Albums.Add(album);
            await db.SaveChangesAsync();

            return album;
        }

        public async Task<List<Album>> GetAlbums(Guid? id) => await db.Albums.Where(a => a.ParentId == id).ToListAsync();

        public async Task<int> Save() => await db.SaveChangesAsync();

        public async Task<List<Album>> GetAllAlbums() => await db.Albums.ToListAsync();
    }
}
