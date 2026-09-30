using Finance.DataModel.Models;

namespace Finance.Api.Infrastructure.Persistence
{
    public interface ITariffarioRepository
    {
        Task<IReadOnlyList<Casello>> GetCaselli();
        Task<Casello?> GetCasello(Guid id);
        Task<bool> NameExists(string name, Guid? excludingId = null);
        Task<Casello> SaveCasello(Casello casello, string name);
        Task<IReadOnlyList<TariffaTratta>> GetTariffe(Guid? caselloAId = null, Guid? caselloBId = null);
        Task<IReadOnlyList<TariffaTratta>> Replace(Guid caselloAId, Guid caselloBId, IReadOnlyList<TariffaTratta> definitions);
    }
}
