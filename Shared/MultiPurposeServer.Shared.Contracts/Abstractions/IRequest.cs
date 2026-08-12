using MultiPurposeServer.Shared.Utils.Extensions;

namespace MultiPurposeServer.Shared.Contracts.Abstractions
{
    public interface IRequest
    {
        void Normalize() => NormalizationExtensions.Normalize(this);
        void Validate() => ValidationExtensions.Validate(this);
    }
}
