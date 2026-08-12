namespace MultiPurposeServer.Shared.Utils.Validation
{
    public sealed class ValidationResult
    {
        private readonly Dictionary<string, List<string>> errors = [];

        public bool IsValid => errors.Count == 0;

        public IReadOnlyDictionary<string, IReadOnlyList<string>> Errors => errors.ToDictionary(pair => pair.Key, pair => (IReadOnlyList<string>)pair.Value);
        public bool HasErrors(string propertyName) => errors.ContainsKey(propertyName);
        public IReadOnlyList<string> GetErrors(string propertyName) => errors.TryGetValue(propertyName, out List<string>? propertyErrors) ? propertyErrors : [];

        internal void AddError(string propertyName, string message)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);
            ArgumentException.ThrowIfNullOrWhiteSpace(message);


            if (!errors.TryGetValue(propertyName, out List<string>? propertyErrors))
            {
                propertyErrors = [];
                errors.Add(propertyName, propertyErrors);
            }

            if (!propertyErrors.Contains(message))
            {
                propertyErrors.Add(message);
            }
        }
    }
}
