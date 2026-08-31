using Finance.Api.Application;
using Finance.DataModel.Models;

namespace Finance.Api.Infrastructure.Persistence
{
    public interface ICategoriaRepository
    {
        Task<Categoria> Create(string name, string displayName);
        Task Delete(Categoria categoria);
        Task<IReadOnlyList<Categoria>> GetAll();
        Task<Categoria?> GetByName(string name);
        Task<CategoriaUsage> GetUsage(Guid id);
        Task<bool> NameExists(string name);
        Task RemoveReferences(Guid id);
        Task<Categoria> Update(Categoria categoria, string displayName);
    }
}
