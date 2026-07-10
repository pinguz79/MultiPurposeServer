using Portfolio.Api.Repositories;
using Portfolio.Data.Models;

namespace Portfolio.Api.Services
{
    public class FotoService(IFotoRepository fotoRepository) : IFotoService
    {
        public Task<Foto?> GetById(Guid photoId) => fotoRepository.GetById(photoId);
    }
}
