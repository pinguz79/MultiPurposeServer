using Finance.DataModel.Models;

namespace Finance.Api.Infrastructure.Persistence
{
    public interface IMovimentoRepository
    {
        Task<Movimento> Create(Guid contoId, DateOnly date, string description, string formula, Guid? pianificazioneId = null, Guid? categoriaId = null);
        Task<Movimento?> GetById(Guid id);
        Task<IReadOnlyList<Movimento>> GetBefore(DateOnly date);
        Task Consolidate(Guid id, string formula);
        Task<IReadOnlyList<Movimento>> GetByContoAfter(Guid contoId, DateOnly from);
        Task<IReadOnlyList<Movimento>> GetByContoThrough(Guid contoId, DateOnly to);
        Task<IReadOnlyList<DateOnly>> GetPreviousDates(Guid contoId, DateOnly date, int count);
        Task<IReadOnlyList<DateOnly>> GetNextDates(Guid contoId, DateOnly date, int count);
        Task<Movimento> Update(Guid id, DateOnly? date, string? description, string? formula, Guid? categoriaId, bool clearCategory);
    }
}
