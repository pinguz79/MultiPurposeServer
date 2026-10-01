using MultiPurposeServer.Shared.Contracts.Abstractions;
using MultiPurposeServer.Shared.Utils.Attributes;

namespace Finance.Contracts.Requests
{
    public sealed record CreateFinanziamentoRequest([property: Normalize, Required] string Name, [property: Normalize, Required] string DisplayName,
        [property: Normalize, Required] string Lender, decimal InitialPrincipal, decimal Tan, decimal Installment, DateOnly FirstDueDate, int InstallmentCount,
        int? DueDay = null, decimal Insurance = 0m, decimal Fees = 0m) : IRequest;
}
