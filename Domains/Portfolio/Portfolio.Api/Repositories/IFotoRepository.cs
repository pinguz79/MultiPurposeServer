using Portfolio.Data.Models;

namespace Portfolio.Api.Repositories;

public interface IFotoRepository
{
    Task<Foto> CreatePhoto(Guid albumId, string fileName);
    Task<Foto?> GetById(Guid photoId);
}
