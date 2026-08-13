using System.Collections;

using FluentAssertions;

using Moq;

using MultiPurposeServer.Shared.Utils.Attributes;
using MultiPurposeServer.Shared.Utils.Extensions;
using MultiPurposeServer.Shared.Utils.Normalization;

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

