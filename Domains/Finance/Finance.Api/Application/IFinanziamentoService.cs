using Finance.Contracts.Requests;
using Finance.DataModel.Models;

namespace Finance.Api.Application
{
    public interface IFinanziamentoService
    {
        Task<IReadOnlyList<Finanziamento>> GetAll(bool includeClosed = false);
        Task<Finanziamento> Resolve(string name);
        Task<Finanziamento> Create(CreateFinanziamentoRequest request);
        Task<Finanziamento> Update(string name, UpdateFinanziamentoRequest request);
        Task SaveAlignment(string name, int number, decimal principal, DateOnly today, bool create);
        Task DeleteAlignment(string name, int number);
    }
}
