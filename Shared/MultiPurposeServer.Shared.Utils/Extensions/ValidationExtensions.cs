using MultiPurposeServer.Shared.Utils.Validation;

namespace MultiPurposeServer.Shared.Utils.Extensions
{
    public static class ValidationExtensions
    {
        public static void Validate(this object instance) => Validator.Validate(instance);
    }
}
