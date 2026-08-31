using Finance.DataModel.Models;

namespace Finance.Api.Infrastructure.Persistence
{
    public interface IMovimentoRepository
    {
        Task<Movimento> Create(Guid contoId, DateOnly date, string description, string formula, Guid? pianificazioneId = null);
        Task<Movimento?> GetById(Guid id);
        Task<IReadOnlyList<Movimento>> GetByContoThrough(Guid contoId, DateOnly to);
        Task<IReadOnlyList<DateOnly>> GetPreviousDates(Guid contoId, DateOnly date, int count);
        Task<IReadOnlyList<DateOnly>> GetNextDates(Guid contoId, DateOnly date, int count);
        Task<Movimento> Update(Guid id, DateOnly? date, string? description, string? formula);
    }
}
