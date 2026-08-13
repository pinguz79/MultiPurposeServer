using FluentAssertions;

using MultiPurposeServer.Shared.Contracts.Abstractions;
using MultiPurposeServer.Shared.Utils.Validation.Exceptions;

namespace MultiPurposeServer.Shared.ContractsTests
{
    public sealed class IRequestTests
    {
        [Fact]
        public void Normalize_RequestWithNormalizableProperty_NormalizesProperty()
        {
            // Arrange
            IRequest request = new TestRequest("  Test value  ");

            // Act
            request.Normalize();

            // Assert
            ((TestRequest)request).Value.Should().Be("Test value");
        }

        [Fact]
        public void Validate_ValidRequest_DoesNotThrow()
        {
            // Arrange
            IRequest request = new TestRequest("Test value");

            // Act
            Action act = request.Validate;

            // Assert
            act.Should().NotThrow();
        }

        [Fact]
        public void Validate_InvalidRequest_ThrowsValidationException()
        {
            // Arrange
            IRequest request = new TestRequest("   ");

            // Act
            Action act = request.Validate;

            // Assert
            act.Should().Throw<ValidationException>();
        }

        [Fact]
        public void NormalizeAndValidate_RequestWithWhitespaceAroundValue_NormalizesAndDoesNotThrow()
        {
            // Arrange
            IRequest request = new TestRequest("  Test value  ");

            // Act
            Action act = () =>
            {
                request.Normalize();
                request.Validate();
            };

            // Assert
            act.Should().NotThrow();
            ((TestRequest)request).Value.Should().Be("Test value");
        }
    }
}
