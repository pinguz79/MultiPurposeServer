using Finance.DataModel.Models;

using MultiPurposeServer.Shared.Persistence.Operations;

namespace Finance.Api.Application
{
    public interface ICategoriaService
    {
        Task<IApplicationOperation> BeginOperation();
        Task<Categoria> Create(string name, string displayName);
        Task<CategoriaDeleteResult> Delete(string name, bool confirmReferences);
        Task<IReadOnlyList<(Categoria Categoria, CategoriaUsage Usage)>> GetAll();
        Task<(Categoria Categoria, CategoriaUsage Usage)?> GetByName(string name);
        Task<Categoria> Resolve(string name);
        Task<Categoria> Update(string name, string displayName);
    }
}
