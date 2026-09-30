using MultiPurposeServer.Shared.Contracts.Abstractions;

namespace Finance.Contracts.Requests
{
    public sealed record TariffaTrattaDefinitionRequest(decimal Value, DateOnly? ValidFrom = null, DateOnly? ValidTo = null) : IRequest;
}
