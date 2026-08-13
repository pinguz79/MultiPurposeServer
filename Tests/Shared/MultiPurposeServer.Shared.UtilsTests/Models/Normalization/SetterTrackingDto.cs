using MultiPurposeServer.Shared.Utils.Attributes;

namespace MultiPurposeServer.Shared.UtilsTests.Models.Normalization
{
    internal sealed class SetterTrackingDto
    {
        private string? _value;

        public SetterTrackingDto(string? value)
        {
            Value = value;
        }

        public int SetterCalls { get; private set; }

        [Normalize]
        public string? Value
        {
            get => _value;
            set
            {
                SetterCalls++;
                _value = value;
            }
        }
    }
}

