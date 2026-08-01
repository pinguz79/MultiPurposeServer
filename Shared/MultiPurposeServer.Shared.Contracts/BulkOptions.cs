using MultiPurposeServer.Shared.Contracts.Enums;

namespace MultiPurposeServer.Shared.Contracts
{
    public sealed record BulkOptions(BulkErrorStrategy ErrorStrategy = BulkErrorStrategy.WarningAndContinue);
}