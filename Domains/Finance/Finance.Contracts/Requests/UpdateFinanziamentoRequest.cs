using MultiPurposeServer.Shared.Contracts.Abstractions;
using MultiPurposeServer.Shared.Utils.Attributes;

namespace Finance.Contracts.Requests
{
    public sealed record UpdateFinanziamentoRequest([property: Normalize] string? DisplayName = null, [property: Normalize] string? Lender = null,
        decimal? InitialPrincipal = null, decimal? Tan = null, decimal? Installment = null, DateOnly? FirstDueDate = null, int? InstallmentCount = null,
        int? DueDay = null, decimal? Insurance = null, decimal? Fees = null, bool? IsClosed = null) : IRequest;
}
