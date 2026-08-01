namespace MultiPurposeServer.Shared.Utils.Attributes.Abstractions
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public abstract class ValidationAttribute : Attribute
    {
    }
}