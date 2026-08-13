using FluentAssertions;

using MultiPurposeServer.Shared.Utils.Attributes;
using MultiPurposeServer.Shared.Utils.Extensions;
using MultiPurposeServer.Shared.Utils.Validation.Exceptions;
using MultiPurposeServer.Shared.UtilsTests.Models.Validation;

namespace MultiPurposeServer.Shared.UtilsTests.Validation
{
    public sealed class ValidatorTests
    {
        [Fact]
        public void Validate_WhenInstanceIsNull_ThrowsArgumentNullException()
        {
            object? instance = null;

            Action action = () => instance!.Validate();

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Validate_WhenDtoHasNoValidationErrors_DoesNotThrow()
        {
            RequiredStringRequest request = new() { Value = "Valid" };

            Action action = request.Validate;

            action.Should().NotThrow();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t\r\n")]
        public void Required_WhenStringIsMissing_ThrowsValidationException(string? value)
        {
            RequiredStringRequest request = new() { Value = value };

            Action action = request.Validate;

            ValidationException exception = action.Should().Throw<ValidationException>().Which;
            exception.Errors.Should().ContainKey(nameof(RequiredStringRequest.Value));
        }

        [Theory]
        [InlineData("Value")]
        [InlineData(" Value ")]
        public void Required_WhenStringHasContent_DoesNotThrow(string value)
        {
            RequiredStringRequest request = new() { Value = value };

            Action action = request.Validate;

            action.Should().NotThrow();
        }

        [Fact]
        public void Required_WhenCollectionIsNull_ThrowsValidationException()
        {
            RequiredCollectionRequest request = new() { Items = null };

            Action action = request.Validate;

            ValidationException exception = action.Should().Throw<ValidationException>().Which;
            exception.Errors.Should().ContainKey(nameof(RequiredCollectionRequest.Items));
        }

        [Fact]
        public void Required_WhenCollectionIsEmpty_ThrowsValidationException()
        {
            RequiredCollectionRequest request = new() { Items = [] };

            Action action = request.Validate;

            ValidationException exception = action.Should().Throw<ValidationException>().Which;
            exception.Errors.Should().ContainKey(nameof(RequiredCollectionRequest.Items));
        }

        [Fact]
        public void Required_WhenCollectionContainsOneElement_DoesNotThrow()
        {
            RequiredCollectionRequest request = new() { Items = [null] };

            Action action = request.Validate;

            action.Should().NotThrow();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(-1)]
        public void Required_WhenValueTypeHasAnyValue_DoesNotThrow(int value)
        {
            RequiredValueTypeRequest request = new() { Value = value };

            Action action = request.Validate;

            action.Should().NotThrow();
        }

        [Fact]
        public void Required_WhenNullableValueTypeIsNull_ThrowsValidationException()
        {
            RequiredNullableValueTypeRequest request = new() { Value = null };

            Action action = request.Validate;

            ValidationException exception = action.Should().Throw<ValidationException>().Which;
            exception.Errors.Should().ContainKey(nameof(RequiredNullableValueTypeRequest.Value));
        }

        [Fact]
        public void Required_WhenNullableValueTypeContainsDefaultValue_DoesNotThrow()
        {
            RequiredNullableValueTypeRequest request = new() { Value = 0 };

            Action action = request.Validate;

            action.Should().NotThrow();
        }

        [Fact]
        public void RequiredAtLeastOne_WhenAllValuesAreMissing_ThrowsValidationExceptionWithCombinedKey()
        {
            RequiredAtLeastOneRequest request = new();

            Action action = request.Validate;

            ValidationException exception = action.Should().Throw<ValidationException>().Which;
            exception.Errors.Should().ContainKey("First|Second");
        }

        [Theory]
        [InlineData("Value", null)]
        [InlineData(null, "Value")]
        [InlineData("", "Value")]
        [InlineData("   ", "Value")]
        public void RequiredAtLeastOne_WhenAtLeastOneValueIsPresent_DoesNotThrow(string? first, string? second)
        {
            RequiredAtLeastOneRequest request = new() { First = first, Second = second };

            Action action = request.Validate;

            action.Should().NotThrow();
        }

        [Fact]
        public void RequiredAtLeastOne_WhenTwoGroupsAreMissing_ReturnsTwoDistinctCombinedKeys()
        {
            MultipleRequiredAtLeastOneGroupsRequest request = new();

            Action action = request.Validate;

            ValidationException exception = action.Should().Throw<ValidationException>().Which;
            exception.Errors.Keys.Should().BeEquivalentTo(["A|B", "C|D"]);
        }

        [Fact]
        public void RequiredAtLeastOne_WhenValidationFails_MessageContainsAllGroupPropertyNames()
        {
            RequiredAtLeastOneRequest request = new();

            Action action = request.Validate;

            ValidationException exception = action.Should().Throw<ValidationException>().Which;
            string message = exception.Errors["First|Second"].Single();
            message.Should().Contain(nameof(RequiredAtLeastOneRequest.First));
            message.Should().Contain(nameof(RequiredAtLeastOneRequest.Second));
        }

        [Fact]
        public void ValidateChildren_WhenChildIsNull_DoesNotThrow()
        {
            ParentRequest request = new() { Child = null };

            Action action = request.Validate;

            action.Should().NotThrow();
        }

        [Fact]
        public void ValidateChildren_WhenChildIsInvalid_ReturnsNestedPropertyKey()
        {
            ParentRequest request = new() { Child = new ChildRequest() };

            Action action = request.Validate;

            ValidationException exception = action.Should().Throw<ValidationException>().Which;
            exception.Errors.Should().ContainKey("Child.Name");
        }

        [Fact]
        public void ValidateChildren_WhenCollectionContainsInvalidChild_ReturnsIndexedPropertyKey()
        {
            ParentCollectionRequest request = new() { Children = [new ChildRequest { Name = "Valid" }, new ChildRequest()] };

            Action action = request.Validate;

            ValidationException exception = action.Should().Throw<ValidationException>().Which;
            exception.Errors.Should().ContainKey("Children[1].Name");
            exception.Errors.Should().NotContainKey("Children[0].Name");
        }

        [Fact]
        public void ValidateChildren_WhenCollectionContainsNullElement_SkipsNullElement()
        {
            ParentNullableCollectionRequest request = new() { Children = [null, new ChildRequest()] };

            Action action = request.Validate;

            ValidationException exception = action.Should().Throw<ValidationException>().Which;
            exception.Errors.Should().ContainKey("Children[1].Name");
            exception.Errors.Should().NotContainKey("Children[0].Name");
        }

        [Fact]
        public void ValidateChildren_WhenNestedGroupIsInvalid_ReturnsPrefixedCombinedKey()
        {
            ParentGroupRequest request = new() { Child = new RequiredAtLeastOneRequest() };

            Action action = request.Validate;

            ValidationException exception = action.Should().Throw<ValidationException>().Which;
            exception.Errors.Should().ContainKey("Child.First|Child.Second");
        }

        [Fact]
        public void ValidateChildren_WhenCollectionItemGroupIsInvalid_ReturnsIndexedCombinedKey()
        {
            ParentGroupCollectionRequest request = new() { Children = [new RequiredAtLeastOneRequest()] };

            Action action = request.Validate;

            ValidationException exception = action.Should().Throw<ValidationException>().Which;
            exception.Errors.Should().ContainKey("Children[0].First|Children[0].Second");
        }

        [Fact]
        public void RequiredAndValidateChildren_WhenCollectionIsNull_ReturnsOnlyRequiredError()
        {
            RequiredParentCollectionRequest request = new() { Children = null };

            Action action = request.Validate;

            ValidationException exception = action.Should().Throw<ValidationException>().Which;
            exception.Errors.Should().ContainSingle();
            exception.Errors.Should().ContainKey(nameof(RequiredParentCollectionRequest.Children));
        }

        [Theory]
        [InlineData(true, false, false)]
        [InlineData(false, true, false)]
        [InlineData(false, false, true)]
        [InlineData(true, true, true)]
        public void RequiredAtLeastOneTrue_WhenAtLeastOneValueIsTrue_DoesNotThrow(bool first, bool second, bool third)
        {
            // Arrange
            RequiredAtLeastOneTrueRequest request = new() { First = first, Second = second, Third = third };

            // Act
            Action action = request.Validate;

            // Assert
            action.Should().NotThrow();
        }

        [Fact]
        public void RequiredAtLeastOneTrue_WhenAllValuesAreFalse_ThrowsValidationExceptionWithCombinedKey()
        {
            // Arrange
            RequiredAtLeastOneTrueRequest request = new();

            // Act
            Action action = request.Validate;

            // Assert
            ValidationException exception = action.Should().Throw<ValidationException>().Which;
            exception.Errors.Should().ContainKey("First|Second|Third");
        }

        [Fact]
        public void RequiredAtLeastOneTrue_WhenAllValuesAreFalse_ReturnsSingleGroupError()
        {
            // Arrange
            RequiredAtLeastOneTrueRequest request = new();

            // Act
            Action action = request.Validate;

            // Assert
            ValidationException exception = action.Should().Throw<ValidationException>().Which;
            exception.Errors.Should().ContainSingle();
        }

        [Fact]
        public void RequiredAtLeastOneTrue_WhenValidationFails_MessageContainsAllGroupPropertyNames()
        {
            // Arrange
            RequiredAtLeastOneTrueRequest request = new();

            // Act
            Action action = request.Validate;

            // Assert
            ValidationException exception = action.Should().Throw<ValidationException>().Which;
            string message = exception.Errors["First|Second|Third"].Single();
            message.Should().Contain(nameof(RequiredAtLeastOneTrueRequest.First));
            message.Should().Contain(nameof(RequiredAtLeastOneTrueRequest.Second));
            message.Should().Contain(nameof(RequiredAtLeastOneTrueRequest.Third));
        }

        [Fact]
        public void RequiredAtLeastOneTrue_WhenTwoGroupsAreFalse_ReturnsTwoDistinctCombinedKeys()
        {
            // Arrange
            MultipleRequiredAtLeastOneTrueGroupsRequest request = new();

            // Act
            Action action = request.Validate;

            // Assert
            ValidationException exception = action.Should().Throw<ValidationException>().Which;
            exception.Errors.Keys.Should().BeEquivalentTo(["A|B", "C|D"]);
        }

        [Fact]
        public void RequiredAtLeastOneTrue_WhenGroupContainsNonBooleanProperty_ThrowsInvalidOperationException()
        {
            // Arrange
            InvalidRequiredAtLeastOneTrueRequest request = new();

            // Act
            Action action = request.Validate;

            // Assert
            InvalidOperationException exception = action.Should().Throw<InvalidOperationException>().Which;
            exception.Message.Should().Contain(nameof(RequiredAtLeastOneTrueAttribute));
            exception.Message.Should().Contain(nameof(InvalidRequiredAtLeastOneTrueRequest.Value));
        }

        [Fact]
        public void Validate_WhenMultiplePropertiesAreInvalid_ReturnsAllErrors()
        {
            MultipleRequiredPropertiesRequest request = new();

            Action action = request.Validate;

            ValidationException exception = action.Should().Throw<ValidationException>().Which;
            exception.Errors.Keys.Should().BeEquivalentTo(
            [
                nameof(MultipleRequiredPropertiesRequest.First),
                nameof(MultipleRequiredPropertiesRequest.Second)
            ]);
        }

        [Fact]
        public void Validate_WhenDecoratedPropertyIsInherited_AppliesInheritedRule()
        {
            DerivedRequiredRequest request = new();

            Action action = request.Validate;

            ValidationException exception = action.Should().Throw<ValidationException>().Which;
            exception.Errors.Should().ContainKey(nameof(BaseRequiredRequest.Value));
        }

        [Fact]
        public void Validate_WhenDecoratedPropertyDoesNotHavePublicGetter_IgnoresProperty()
        {
            PrivateGetterRequest request = new() { Value = null };

            Action action = request.Validate;

            action.Should().NotThrow();
        }

        [Fact]
        public void Validate_WhenAttributeIsNotSupported_ThrowsNotSupportedException()
        {
            UnsupportedValidationRequest request = new();

            Action action = request.Validate;

            NotSupportedException exception = action.Should().Throw<NotSupportedException>().Which;
            exception.Message.Should().Contain(nameof(UnsupportedValidationAttribute));
        }

        [Fact]
        public void Required_WhenNonCollectionEnumerableIsEmpty_ThrowsValidationException()
        {
            RequiredEnumerableRequest request = new() { Items = Enumerable.Empty<int>() };

            Action action = request.Validate;

            ValidationException exception = action.Should().Throw<ValidationException>().Which;
            exception.Errors.Should().ContainKey(nameof(RequiredEnumerableRequest.Items));
        }

        [Fact]
        public void Required_WhenNonCollectionEnumerableContainsElement_DoesNotThrow()
        {
            RequiredEnumerableRequest request = new() { Items = Enumerable.Repeat(1, 1) };

            Action action = request.Validate;

            action.Should().NotThrow();
        }

        [Fact]
        public void ValidateChildren_WhenChildIsValid_DoesNotThrow()
        {
            ParentRequest request = new() { Child = new ChildRequest { Name = "Valid" } };

            Action action = request.Validate;

            action.Should().NotThrow();
        }



















    }
}
