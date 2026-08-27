using Finance.Contracts.Requests;
using Finance.DataModel.Models;

using MultiPurposeServer.Shared.Persistence.Operations;

namespace Finance.Api.Application
{
    public interface IVoceRicorrenteService
    {
        Task<IApplicationOperation> BeginOperation();
        Task<IReadOnlyList<VoceRicorrente>> Create(string name, IReadOnlyList<VoceRicorrenteDefinitionRequest> definitions);
        Task Delete(string name);
        Task<IReadOnlyList<IReadOnlyList<VoceRicorrente>>> GetAll();
        Task<IReadOnlyList<VoceRicorrente>> GetByName(string name);
        Task<IReadOnlyList<VoceRicorrente>> Update(string currentName, string name, IReadOnlyList<VoceRicorrenteDefinitionRequest> definitions);
    }
}
