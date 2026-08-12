using MultiPurposeServer.Shared.Utils.Attributes.Abstractions;

namespace MultiPurposeServer.Shared.Utils.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class RequiredAtLeastOneAttribute(string group = "__default") : ValidationAttribute
    {
        public string Group { get; } = group;
    }
}
