namespace MultiPurposeServer.Shared.Utils.Validation
{
    internal sealed class ValidationContext(ValidationResult result, IReadOnlyList<ValidationPathSegment>? path = null)
    {
        private readonly IReadOnlyList<ValidationPathSegment> _path = path ?? [];

        public ValidationResult Result => result;

        public void AddError(string propertyName, string message) => Result.AddError(ValidationKeyFormatter.FormatProperty(_path, propertyName), message);

        public void AddError(IEnumerable<string> propertyNames, string message) => Result.AddError(ValidationKeyFormatter.FormatGroup(_path, propertyNames), message);

        public ValidationContext CreateChild(string propertyName) => new(Result, [.. _path, new ValidationPathSegment(propertyName)]);

        public ValidationContext CreateCollectionItem(string propertyName, int index) => new(Result, [.. _path, new ValidationPathSegment(propertyName, index)]);
    }
}
