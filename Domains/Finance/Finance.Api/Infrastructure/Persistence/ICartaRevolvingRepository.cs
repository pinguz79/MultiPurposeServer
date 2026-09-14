using Finance.DataModel.Models;

namespace Finance.Api.Infrastructure.Persistence
{
    public interface ICartaRevolvingRepository
    {
        Task<IReadOnlyList<Pianificazione>> GetPlans(string prefix);
        Task<bool> HasCorrelation(Guid firstId, Guid secondId);
        Task CreateCorrelation(Guid firstId, Guid secondId);
        Task Save();
    }
}
