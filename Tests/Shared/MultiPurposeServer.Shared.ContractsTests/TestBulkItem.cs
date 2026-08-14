using MultiPurposeServer.Shared.Contracts.Abstractions;
using MultiPurposeServer.Shared.Utils.Attributes;

namespace MultiPurposeServer.Shared.ContractsTests
{
    internal sealed class TestBulkItem(string? value, Guid? id = null) : IRequest
    {
        public Guid Id { get; } = id ?? Guid.NewGuid();

        [Normalize]
        [Required]
        public string? Value { get; set; } = value;
    }
}
