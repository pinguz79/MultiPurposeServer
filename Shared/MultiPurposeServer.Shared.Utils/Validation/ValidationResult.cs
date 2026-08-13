namespace MultiPurposeServer.Shared.Utils.Validation
{
    public sealed class ValidationResult
    {
        private readonly Dictionary<string, List<string>> _errors = [];

        public bool IsValid => _errors.Count == 0;

        public IReadOnlyDictionary<string, IReadOnlyList<string>> Errors => _errors.ToDictionary(pair => pair.Key, pair => (IReadOnlyList<string>)pair.Value);
        public bool HasErrors(string propertyName) => _errors.ContainsKey(propertyName);
        public IReadOnlyList<string> GetErrors(string propertyName) => _errors.TryGetValue(propertyName, out List<string>? propertyErrors) ? propertyErrors : [];

        internal void AddError(string propertyName, string message)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);
            ArgumentException.ThrowIfNullOrWhiteSpace(message);


            if (!_errors.TryGetValue(propertyName, out List<string>? propertyErrors))
            {
                propertyErrors = [];
                _errors.Add(propertyName, propertyErrors);
            }

            if (!propertyErrors.Contains(message))
            {
                propertyErrors.Add(message);
            }
        }
    }
}
