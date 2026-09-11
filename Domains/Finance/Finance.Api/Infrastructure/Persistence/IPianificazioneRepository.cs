using Finance.DataModel.Models;

namespace Finance.Api.Infrastructure.Persistence
{
    public interface IPianificazioneRepository
    {
        Task<Pianificazione?> Get(Guid id);
        Task<IReadOnlyList<Pianificazione>> GetList(string? contoName);
        Task Delete(Pianificazione pianificazione, bool deleteMovimenti);
        Task<Periodicita> GetOrCreateMonthlyPeriodicity(int interval, int? dayOfMonth, bool endOfMonth);
        Task<Periodicita> GetOrCreateWeeklyPeriodicity(int interval, DayOfWeek dayOfWeek);
        Task<Pianificazione> Create(
            Guid contoId,
            Guid periodicitaId,
            string description,
            string movimentoDescription,
            string movimentoFormula,
            DateOnly validFrom,
            DateOnly validTo,
            ModalitaCategoria modalitaCategoria,
            string? voceRicorrenteCategoriaName,
            Guid? categoriaId);
    }
}
