using MultiPurposeServer.Shared.Utils.Attributes.Abstractions;

namespace MultiPurposeServer.Shared.Utils.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class UniqueByAttribute(string keyPropertyName) : ValidationAttribute
    {
        public string KeyPropertyName => keyPropertyName;
    }
}
