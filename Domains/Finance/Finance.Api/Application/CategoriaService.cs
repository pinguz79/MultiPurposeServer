using Finance.Api.Infrastructure.Persistence;
using Finance.DataModel.Models;

using MultiPurposeServer.Shared.Persistence.EntityFramework;
using MultiPurposeServer.Shared.Persistence.Operations;

namespace Finance.Api.Application
{
    public class CategoriaService(
        ICategoriaRepository repository,
        EntityFrameworkPersistenceCoordinator<DataModel.FinanceContext> persistence) : ICategoriaService
    {
        public async Task<IApplicationOperation> BeginOperation() => new ApplicationOperation(await persistence.BeginTransaction());

        public async Task<Categoria> Create(string name, string displayName)
        {
            string normalizedName = ContoService.NormalizeName(name);
            string normalizedDisplayName = NormalizeDisplayName(displayName);

            return await repository.NameExists(normalizedName)
                ? throw new DuplicateNameException(normalizedName)
                : await repository.Create(normalizedName, normalizedDisplayName);
        }

        public async Task<CategoriaDeleteResult> Delete(string name, bool confirmReferences)
        {
            Categoria categoria = await Resolve(name);
            CategoriaUsage usage = await repository.GetUsage(categoria.Id);

            if (usage.Total > 0 && !confirmReferences)
            {
                return new CategoriaDeleteResult(false, usage);
            }

            await repository.RemoveReferences(categoria.Id);
            await repository.Delete(categoria);

            return new CategoriaDeleteResult(true, usage);
        }

        public async Task<IReadOnlyList<(Categoria Categoria, CategoriaUsage Usage)>> GetAll()
        {
            IReadOnlyList<Categoria> categories = await repository.GetAll();
            var result = new List<(Categoria, CategoriaUsage)>(categories.Count);

            foreach (Categoria categoria in categories)
            {
                result.Add((categoria, await repository.GetUsage(categoria.Id)));
            }

            return result;
        }

        public async Task<(Categoria Categoria, CategoriaUsage Usage)?> GetByName(string name)
        {
            Categoria? categoria = await repository.GetByName(name);

            return categoria is null ? null : (categoria, await repository.GetUsage(categoria.Id));
        }

        public async Task<Categoria> Resolve(string name)
        {
            string normalizedName = ContoService.NormalizeName(name);

            return await repository.GetByName(normalizedName) ?? throw new KeyNotFoundException($"Categoria '{normalizedName}' was not found.");
        }

        public async Task<Categoria> Update(string name, string displayName)
        {
            Categoria categoria = await Resolve(name);

            return await repository.Update(categoria, NormalizeDisplayName(displayName));
        }

        private static string NormalizeDisplayName(string value)
        {
            string normalized = value.Trim();

            return normalized.Length == 0 ? throw new ArgumentException("DisplayName cannot be empty.", nameof(value)) : normalized;
        }
    }
}
