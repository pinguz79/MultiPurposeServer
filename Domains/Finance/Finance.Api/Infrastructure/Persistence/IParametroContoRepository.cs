using Finance.DataModel.Models;

namespace Finance.Api.Infrastructure.Persistence
{
    public interface IParametroContoRepository
    {
        Task Delete(Guid contoId, string name);
        Task<IReadOnlyList<ParametroConto>> GetAll(Guid contoId);
        Task<IReadOnlyList<ParametroConto>> GetByName(Guid contoId, string name);
        Task<bool> IsReferenced(string reference);
        Task<bool> NameExists(Guid contoId, string name);
        Task<IReadOnlyList<ParametroConto>> Replace(Guid contoId, string? currentName, string name, TipoParametroConto type, IReadOnlyList<ParametroConto> definitions);
    }
}
