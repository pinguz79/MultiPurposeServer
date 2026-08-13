namespace MultiPurposeServer.Shared.UtilsTests.Models.Validation
{
    internal sealed class UnsupportedValidationRequest
    {
        [UnsupportedValidation]
        public string? Value { get; set; }
    }
}


