using System.Reflection;

using FluentAssertions;

using MultiPurposeServer.Shared.Contracts;
using MultiPurposeServer.Shared.Contracts.Abstractions;
using MultiPurposeServer.Shared.Contracts.Requests;
using MultiPurposeServer.Shared.Utils.Attributes;
using MultiPurposeServer.Shared.Utils.Validation.Exceptions;

namespace MultiPurposeServer.Shared.ContractsTests
{
    public sealed class BulkRequestTests
    {
        [Fact]
        public void Constructor_OptionsAndItemsProvided_ExposesValuesThroughIBulk()
        {
            // Arrange
            var options = new BulkOptions();
            IReadOnlyCollection<TestBulkItem> items = [new TestBulkItem("First"), new TestBulkItem("Second")];

            // Act
            IBulk<TestBulkItem> request = new TestBulkRequest(options, items);

            // Assert
            request.Options.Should().BeSameAs(options);
            request.Items.Should().BeSameAs(items);
        }

        [Fact]
        public void BulkRequest_Type_ImplementsIRequestAndIBulk()
        {
            // Arrange
            var request = new TestBulkRequest(new BulkOptions(), []);

            // Act
            var implementsRequest = request is IRequest;
            var implementsBulk = request is IBulk<TestBulkItem>;

            // Assert
            implementsRequest.Should().BeTrue();
            implementsBulk.Should().BeTrue();
        }

        [Fact]
        public void Options_Property_HasRequiredAttribute()
        {
            // Arrange
            PropertyInfo property = GetProperty(nameof(BulkRequest<TestBulkItem>.Options));

            // Act
            var attribute = property.GetCustomAttribute<RequiredAttribute>();

            // Assert
            attribute.Should().NotBeNull();
        }

        [Fact]
        public void Items_Property_HasRequiredAttribute()
        {
            // Arrange
            PropertyInfo property = GetProperty(nameof(BulkRequest<TestBulkItem>.Items));

            // Act
            var attribute = property.GetCustomAttribute<RequiredAttribute>();

            // Assert
            attribute.Should().NotBeNull();
        }

        [Fact]
        public void Items_Property_HasNormalizeChildrenAttribute()
        {
            // Arrange
            PropertyInfo property = GetProperty(nameof(BulkRequest<TestBulkItem>.Items));

            // Act
            var attribute = property.GetCustomAttribute<NormalizeChildrenAttribute>();

            // Assert
            attribute.Should().NotBeNull();
        }

        [Fact]
        public void Items_Property_HasValidateChildrenAttribute()
        {
            // Arrange
            PropertyInfo property = GetProperty(nameof(BulkRequest<TestBulkItem>.Items));

            // Act
            var attribute = property.GetCustomAttribute<ValidateChildrenAttribute>();

            // Assert
            attribute.Should().NotBeNull();
        }

        [Fact]
        public void Normalize_ItemsWithNormalizableProperties_NormalizesAllItems()
        {
            // Arrange
            var first = new TestBulkItem("  First  ");
            var second = new TestBulkItem("  Second  ");
            IRequest request = new TestBulkRequest(new BulkOptions(), [first, second]);

            // Act
            request.Normalize();

            // Assert
            first.Value.Should().Be("First");
            second.Value.Should().Be("Second");
        }

        [Fact]
        public void Validate_ValidItems_DoesNotThrow()
        {
            // Arrange
            IRequest request = new TestBulkRequest(new BulkOptions(), [new TestBulkItem("First"), new TestBulkItem("Second")]);

            // Act
            Action act = request.Validate;

            // Assert
            act.Should().NotThrow();
        }

        [Fact]
        public void Validate_InvalidChildItem_ThrowsValidationException()
        {
            // Arrange
            IRequest request = new TestBulkRequest(new BulkOptions(), [new TestBulkItem("Valid"), new TestBulkItem("   ")]);

            // Act
            Action act = request.Validate;

            // Assert
            act.Should().Throw<ValidationException>();
        }

        private static PropertyInfo GetProperty(string name) => typeof(BulkRequest<TestBulkItem>).GetProperty(name) ?? throw new InvalidOperationException($"Property '{name}' was not found.");
    }
}
