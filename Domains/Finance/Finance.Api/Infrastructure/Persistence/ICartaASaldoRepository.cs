using Finance.DataModel.Models;

namespace Finance.Api.Infrastructure.Persistence
{
    public interface ICartaASaldoRepository
    {
        Task<CorrelazionePianificazione> CreateCorrelation(Guid firstId, Guid secondId);
        Task<CorrelazionePianificazione?> GetCorrelation(Guid firstId, Guid secondId);
        Task<IReadOnlyList<Pianificazione>> GetPlans(string debitDescription, string resetDescription);
        Task Save();
    }
}
