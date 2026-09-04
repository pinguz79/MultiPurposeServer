using Finance.Contracts.Requests;
using Finance.DataModel.Models;

using MultiPurposeServer.Shared.Persistence.Operations;

namespace Finance.Api.Application
{
    public interface IParametroContoService
    {
        Task<IApplicationOperation> BeginOperation();
        Task<IReadOnlyList<ParametroConto>> Create(string contoName, string name, TipoParametroConto type, IReadOnlyList<ParametroContoDefinitionRequest> definitions);
        Task Delete(string contoName, string name);
        Task<IReadOnlyList<IReadOnlyList<ParametroConto>>> GetAll(string contoName);
        Task<IReadOnlyList<ParametroConto>> GetByName(string contoName, string name);
        Task<ParametroConto?> Resolve(Guid contoId, string name, DateOnly date);
        Task<IReadOnlyList<ParametroConto>> Update(string contoName, string name, IReadOnlyList<ParametroContoDefinitionRequest> definitions);
    }
}
