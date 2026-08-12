using MultiPurposeServer.Shared.Utils.Attributes.Abstractions;

namespace MultiPurposeServer.Shared.Utils.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class RequiredAtLeastOneTrueAttribute(string group = "__default") : ValidationAttribute
    {
        public string Group => group;
    }
}
