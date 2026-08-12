namespace MultiPurposeServer.Shared.Utils.Validation.Exceptions
{
    [Serializable]
    public sealed class ValidationException(IReadOnlyDictionary<string, IReadOnlyList<string>> errors) : Exception
    {
        public IReadOnlyDictionary<string, IReadOnlyList<string>> Errors => errors ?? throw new ArgumentNullException(nameof(errors));
    }
}
