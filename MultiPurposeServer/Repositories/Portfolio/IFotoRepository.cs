using MultiPurposeServer.Models.Portfolio;

namespace MultiPurposeServer.Repositories.Portfolio
{
    public interface IFotoRepository
    {
        Task<Foto> CreatePhoto(Guid id, string fileName);
    }
}
