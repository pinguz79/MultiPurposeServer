using Finance.DataModel;
using Finance.DataModel.Models;

using Microsoft.EntityFrameworkCore;

using MultiPurposeServer.Shared.Persistence.EntityFramework;

namespace Finance.Api.Infrastructure.Persistence
{
    public class MovimentoRepository(FinanceContext db, EntityFrameworkPersistenceCoordinator<FinanceContext> persistence) : IMovimentoRepository
    {
        public async Task<Movimento> Create(
            Guid contoId,
            DateOnly date,
            string description,
            string formula,
            Guid? pianificazioneId = null,
            Guid? categoriaId = null)
        {
            var movimento = new Movimento
            {
                Id = Guid.NewGuid(),
                ContoId = contoId,
                Date = date,
                Description = description,
                Formula = formula,
                PianificazioneId = pianificazioneId,
                CategoriaId = categoriaId,
            };

            db.Movimenti.Add(movimento);
            await SaveIfRequired();

            return movimento;
        }

        public async Task<Movimento?> GetById(Guid id) => await db.Movimenti.FirstOrDefaultAsync(movimento => movimento.Id == id);

        public async Task<IReadOnlyList<Movimento>> GetByContoThrough(Guid contoId, DateOnly to) => await db.Movimenti
            .Where(movimento => movimento.ContoId == contoId && movimento.Date <= to)
            .OrderBy(movimento => movimento.Date)
            .ThenBy(movimento => movimento.Id)
            .ToListAsync();

        public async Task<IReadOnlyList<DateOnly>> GetPreviousDates(Guid contoId, DateOnly date, int count) => await db.Movimenti
            .Where(movimento => movimento.ContoId == contoId && movimento.Date < date)
            .OrderByDescending(movimento => movimento.Date)
            .ThenByDescending(movimento => movimento.Id)
            .Select(movimento => movimento.Date)
            .Take(count)
            .ToListAsync();

        public async Task<IReadOnlyList<DateOnly>> GetNextDates(Guid contoId, DateOnly date, int count) => await db.Movimenti
            .Where(movimento => movimento.ContoId == contoId && movimento.Date > date)
            .OrderBy(movimento => movimento.Date)
            .ThenBy(movimento => movimento.Id)
            .Select(movimento => movimento.Date)
            .Take(count)
            .ToListAsync();

        public async Task<Movimento> Update(Guid id, DateOnly? date, string? description, string? formula, Guid? categoriaId, bool clearCategory)
        {
            var movimento = await GetById(id) ?? throw new KeyNotFoundException($"Movimento '{id}' was not found.");

            movimento.Date = date ?? movimento.Date;
            movimento.Description = description ?? movimento.Description;
            movimento.Formula = formula ?? movimento.Formula;
            movimento.CategoriaId = clearCategory ? null : categoriaId ?? movimento.CategoriaId;
            await SaveIfRequired();

            return movimento;
        }

        private async Task<int> SaveIfRequired() => persistence.IsTransactionActive ? 0 : await db.SaveChangesAsync();
    }
}
