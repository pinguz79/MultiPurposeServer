using MultiPurposeServer.DBContexts.Portfolio;
using MultiPurposeServer.Models.Portfolio;

namespace MultiPurposeServer.Repositories.Portfolio
{
    public class FotoRepository(PortfolioContext db) : IFotoRepository
    {
        public async Task<Foto> CreatePhoto(Guid albumId, string fileName)
        {
            var photo = new Foto { AlbumId = albumId, FileName = fileName };
            db.Foto.Add(photo);
            await db.SaveChangesAsync();
            return photo;
        }
    }
}
