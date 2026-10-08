using Finance.DataModel.Models;

namespace Finance.Api.Infrastructure.Persistence
{
    public interface ITariffarioRepository
    {
        Task<IReadOnlyList<Casello>> GetCaselli();
        Task<Casello?> GetCasello(Guid id);
        Task<bool> NameExists(string name, Guid? excludingId = null);
        Task<Casello> SaveCasello(Casello casello, string name);
        Task<bool> IsUsed(Guid caselloId, Guid? otherCaselloId = null);
        Task DeleteCasello(Casello casello);
        Task DeleteTratta(Guid caselloAId, Guid caselloBId);
        Task<IReadOnlyList<TariffaTratta>> GetTariffe(Guid? caselloAId = null, Guid? caselloBId = null);
        Task<IReadOnlyList<TariffaTratta>> Replace(Guid caselloAId, Guid caselloBId, IReadOnlyList<TariffaTratta> definitions);
    }
}
