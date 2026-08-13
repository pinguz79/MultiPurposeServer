using MultiPurposeServer.Shared.Contracts.Abstractions;
using MultiPurposeServer.Shared.Utils.Attributes;

namespace MultiPurposeServer.Shared.ContractsTests
{
    internal sealed class TestBulkItem(string? value) : IRequest
    {
        [Normalize]
        [Required]
        public string? Value { get; set; } = value;
    }
}
