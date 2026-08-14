using MultiPurposeServer.Shared.Contracts.Abstractions;
using MultiPurposeServer.Shared.Contracts.Enums;
using MultiPurposeServer.Shared.Utils.Attributes;

namespace MultiPurposeServer.Shared.Contracts
{
    public sealed record BulkOptions(
        [property: EnumDefined] BulkPersistenceStrategy PersistenceStrategy = BulkPersistenceStrategy.PartialSuccess,
        [property: EnumDefined] BulkEvaluationStrategy EvaluationStrategy = BulkEvaluationStrategy.EvaluateAll) : IRequest;
}
