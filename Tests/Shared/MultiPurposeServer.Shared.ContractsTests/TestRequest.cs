using MultiPurposeServer.Shared.Contracts.Abstractions;
using MultiPurposeServer.Shared.Utils.Attributes;

namespace MultiPurposeServer.Shared.Contracts.Tests.Abstractions
{
    internal sealed class TestRequest(string? value) : IRequest
    {
        [Normalize]
        [Required]
        public string? Value { get; set; } = value;
    }
}
