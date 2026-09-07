using Finance.Api.Application;
using Finance.Api.Infrastructure.Caching;
using Finance.DataModel;
using Finance.DataModel.Models;

using Microsoft.EntityFrameworkCore;

using MultiPurposeServer.Shared.Persistence.EntityFramework;

namespace Finance.Api.Infrastructure.Persistence
{
    public class CategoriaRepository(
        FinanceContext db,
        EntityFrameworkPersistenceCoordinator<FinanceContext> persistence,
        FormulaEvaluationCache? cache = null) : ICategoriaRepository
    {
        public async Task<Categoria> Create(string name, string displayName)
        {
            var categoria = new Categoria { Id = Guid.NewGuid(), Name = name, DisplayName = displayName };
            db.Categorie.Add(categoria);
            await SaveIfRequired();

            return categoria;
        }

        public async Task Delete(Categoria categoria)
        {
            db.Categorie.Remove(categoria);
            await SaveIfRequired();
        }

        public async Task<IReadOnlyList<Categoria>> GetAll()
            => await db.Categorie.OrderBy(categoria => categoria.DisplayName).ThenBy(categoria => categoria.Name).ToListAsync();

        public async Task<Categoria?> GetByName(string name) => await db.Categorie.FirstOrDefaultAsync(categoria => categoria.Name == name);

        public async Task<CategoriaUsage> GetUsage(Guid id) => new(
            await db.VociRicorrenti.CountAsync(voce => voce.CategoriaId == id),
            await db.Pianificazioni.CountAsync(pianificazione => pianificazione.CategoriaId == id),
            await db.Movimenti.CountAsync(movimento => movimento.CategoriaId == id));

        public async Task<bool> NameExists(string name) => await db.Categorie.AnyAsync(categoria => categoria.Name == name);

        public async Task RemoveReferences(Guid id)
        {
            await db.VociRicorrenti.Where(voce => voce.CategoriaId == id)
                .ExecuteUpdateAsync(update => update.SetProperty(voce => voce.CategoriaId, (Guid?)null));
            await db.Movimenti.Where(movimento => movimento.CategoriaId == id)
                .ExecuteUpdateAsync(update => update.SetProperty(movimento => movimento.CategoriaId, (Guid?)null));
            await db.Pianificazioni.Where(pianificazione => pianificazione.CategoriaId == id)
                .ExecuteUpdateAsync(update => update
                    .SetProperty(pianificazione => pianificazione.CategoriaId, (Guid?)null)
                    .SetProperty(pianificazione => pianificazione.ModalitaCategoria, ModalitaCategoria.Nessuna));
        }

        public async Task<Categoria> Update(Categoria categoria, string displayName)
        {
            categoria.DisplayName = displayName;
            await SaveIfRequired();

            return categoria;
        }

        private async Task<int> SaveIfRequired()
        {
            cache?.Invalidate();

            return persistence.IsTransactionActive ? 0 : await db.SaveChangesAsync();
        }
    }
}
