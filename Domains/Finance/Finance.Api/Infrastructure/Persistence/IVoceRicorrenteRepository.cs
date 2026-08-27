using Finance.DataModel.Models;

namespace Finance.Api.Infrastructure.Persistence
{
    public interface IVoceRicorrenteRepository
    {
        Task Delete(string name);
        Task<IReadOnlyList<VoceRicorrente>> GetAll();
        Task<IReadOnlyList<VoceRicorrente>> GetByName(string name);
        Task<bool> NameExists(string name);
        Task<IReadOnlyList<VoceRicorrente>> Replace(string? currentName, string name, IReadOnlyList<VoceRicorrente> definitions);
    }
}
