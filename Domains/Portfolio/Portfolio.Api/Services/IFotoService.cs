using Portfolio.Data.Models;

namespace Portfolio.Api.Services
{
    public interface IFotoService
    {
        Task<Foto?> GetById(Guid photoId);
    }
}
