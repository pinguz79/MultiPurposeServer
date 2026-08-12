using System.Collections;

using FluentAssertions;

using Moq;

using MultiPurposeServer.Shared.Utils.Attributes;
using MultiPurposeServer.Shared.Utils.Extensions;
using MultiPurposeServer.Shared.Utils.Normalization;

namespace MultiPurposeServer.Shared.Tests.Utils.Normalization
{
    public sealed class NormalizerTests
    {
        public static TheoryData<string?, string?> StringNormalizationCases => new()
        {
            { null, null },
            { string.Empty, null },
            { " ", null },
            { "\t\r\n", null },
            { "\u00A0", null },
            { "Marco", "Marco" },
            { " Marco", "Marco" },
            { "Marco ", "Marco" },
            { "  Marco  ", "Marco" },
            { "\tMarco\r\n", "Marco" },
            { "Marco   Lepri", "Marco   Lepri" },
            { "àèìòù", "àèìòù" },
            { "  àèìòù  ", "àèìòù" }
        };

        [Theory]
        [MemberData(nameof(StringNormalizationCases))]
        public void Normalize_StringValue_NormalizesAsExpected(string? value, string? expected)
        {
            // Arrange
            StringDto instance = new() { Value = value };

            // Act
            instance.Normalize();

            // Assert
            instance.Value.Should().Be(expected);
        }

        [Fact]
        public void Normalize_StringAtMaximumPracticalLength_TrimsWithoutChangingContent()
        {
            // Arrange
            string content = new('x', 1_000_000);
            StringDto instance = new() { Value = $" {content} " };

            // Act
            instance.Normalize();

            // Assert
            instance.Value.Should().Be(content);
            instance.Value!.Length.Should().Be(1_000_000);
        }

        [Fact]
        public void Normalize_AlreadyNormalizedValue_DoesNotInvokeSetterAgain()
        {
            // Arrange
            SetterTrackingDto instance = new("Marco");
            int setterCallsBeforeNormalization = instance.SetterCalls;

            // Act
            instance.Normalize();

            // Assert
            instance.SetterCalls.Should().Be(setterCallsBeforeNormalization);
        }

        [Fact]
        public void Normalize_ValueRequiringNormalization_InvokesSetterOnce()
        {
            // Arrange
            SetterTrackingDto instance = new(" Marco ");
            int setterCallsBeforeNormalization = instance.SetterCalls;

            // Act
            instance.Normalize();

            // Assert
            instance.Value.Should().Be("Marco");
            instance.SetterCalls.Should().Be(setterCallsBeforeNormalization + 1);
        }

        [Fact]
        public void Normalize_ObjectWithMultipleProperties_NormalizesOnlyDecoratedProperties()
        {
            // Arrange
            MultiplePropertiesDto instance = new() { FirstName = " Marco ", LastName = " Lepri ", Notes = " unchanged " };

            // Act
            instance.Normalize();

            // Assert
            instance.FirstName.Should().Be("Marco");
            instance.LastName.Should().Be("Lepri");
            instance.Notes.Should().Be(" unchanged ");
        }

        [Fact]
        public void Normalize_ObjectWithoutNormalizationAttributes_DoesNothing()
        {
            // Arrange
            UndecoratedDto instance = new() { Value = " Marco " };

            // Act
            instance.Normalize();

            // Assert
            instance.Value.Should().Be(" Marco ");
        }

        [Fact]
        public void Normalize_NullObject_ThrowsArgumentNullException()
        {
            // Arrange
            object instance = null!;

            // Act
            Action action = () => Normalizer.Normalize(instance);

            // Assert
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Normalize_NullCollection_ThrowsArgumentNullException()
        {
            // Arrange
            IEnumerable<StringDto> instances = null!;

            // Act
            Action action = () => Normalizer.Normalize(instances);

            // Assert
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Normalize_EmptyCollection_DoesNotThrow()
        {
            // Arrange
            List<StringDto> instances = [];

            // Act
            Action action = () => instances.Normalize();

            // Assert
            action.Should().NotThrow();
        }

        [Fact]
        public void Normalize_Collection_NormalizesEveryElement()
        {
            // Arrange
            List<StringDto> instances =
            [
                new() { Value = " One " },
                new() { Value = "  " },
                new() { Value = null },
                new() { Value = "Four" }
            ];

            // Act
            instances.Normalize();

            // Assert
            instances[0].Value.Should().Be("One");
            instances[1].Value.Should().BeNull();
            instances[2].Value.Should().BeNull();
            instances[3].Value.Should().Be("Four");
        }

        [Fact]
        public void Normalize_CollectionContainingNullElements_SkipsNullElements()
        {
            // Arrange
            List<StringDto?> instances = [new() { Value = " One " }, null, new() { Value = " Two " }];

            // Act
            Normalizer.Normalize(instances);

            // Assert
            instances[0]!.Value.Should().Be("One");
            instances[1].Should().BeNull();
            instances[2]!.Value.Should().Be("Two");
        }

        [Fact]
        public void Normalize_LargeCollection_NormalizesEveryElement()
        {
            // Arrange
            const int itemCount = 100_000;
            List<StringDto> instances = Enumerable.Range(0, itemCount).Select(index => new StringDto { Value = $" {index} " }).ToList();

            // Act
            instances.Normalize();

            // Assert
            instances.Count.Should().Be(itemCount);
            instances.Should().OnlyContain(instance => instance.Value == instance.Value!.Trim());
            instances[0].Value.Should().Be("0");
            instances[^1].Value.Should().Be((itemCount - 1).ToString());
        }

        [Fact]
        public void Normalize_Enumerable_EnumeratesSourceOnce()
        {
            // Arrange
            List<StringDto> source = [new() { Value = " One " }, new() { Value = " Two " }];
            Mock<IEnumerable<StringDto>> instances = new(MockBehavior.Strict);
            instances.Setup(value => value.GetEnumerator()).Returns(() => source.GetEnumerator()).Verifiable(Times.Once);

            // Act
            Normalizer.Normalize(instances.Object);

            // Assert
            instances.Verify();
            source[0].Value.Should().Be("One");
            source[1].Value.Should().Be("Two");
        }

        [Fact]
        public void NormalizeChildren_NullCollection_DoesNothing()
        {
            // Arrange
            ParentDto instance = new() { Children = null };

            // Act
            Action action = () => instance.Normalize();

            // Assert
            action.Should().NotThrow();
        }

        [Fact]
        public void NormalizeChildren_EmptyCollection_DoesNothing()
        {
            // Arrange
            ParentDto instance = new() { Children = [] };

            // Act
            Action action = () => instance.Normalize();

            // Assert
            action.Should().NotThrow();
        }

        [Fact]
        public void NormalizeChildren_Collection_NormalizesEveryNonNullChild()
        {
            // Arrange
            ParentDto instance = new()
            {
                Children =
                [
                    new() { Value = " One " },
                    null,
                    new() { Value = "  " }
                ]
            };

            // Act
            instance.Normalize();

            // Assert
            instance.Children![0]!.Value.Should().Be("One");
            instance.Children[1].Should().BeNull();
            instance.Children[2]!.Value.Should().BeNull();
        }

        [Fact]
        public void NormalizeChildren_Array_NormalizesEveryChild()
        {
            // Arrange
            ArrayParentDto instance = new()
            {
                Children =
                [
                    new() { Value = " One " },
                    new() { Value = " Two " }
                ]
            };

            // Act
            instance.Normalize();

            // Assert
            instance.Children![0].Value.Should().Be("One");
            instance.Children[1].Value.Should().Be("Two");
        }

        [Fact]
        public void NormalizeChildren_ReadOnlyCollectionWithoutSetter_NormalizesEveryChild()
        {
            // Arrange
            ReadOnlyParentDto instance = new([new() { Value = " One " }, new() { Value = " Two " }]);

            // Act
            instance.Normalize();

            // Assert
            instance.Children[0].Value.Should().Be("One");
            instance.Children[1].Value.Should().Be("Two");
        }

        [Fact]
        public void NormalizeChildren_MultipleLevels_NormalizesAllDescendants()
        {
            // Arrange
            RootDto instance = new()
            {
                Branches =
                [
                    new()
                    {
                        Name = " Branch ",
                        Leaves = [new() { Value = " Leaf " }]
                    }
                ]
            };

            // Act
            instance.Normalize();

            // Assert
            instance.Branches![0].Name.Should().Be("Branch");
            instance.Branches[0].Leaves![0].Value.Should().Be("Leaf");
        }

        [Fact]
        public void Normalize_DerivedObject_NormalizesInheritedAndDerivedProperties()
        {
            // Arrange
            DerivedDto instance = new() { BaseValue = " Base ", DerivedValue = " Derived " };

            // Act
            instance.Normalize();

            // Assert
            instance.BaseValue.Should().Be("Base");
            instance.DerivedValue.Should().Be("Derived");
        }

        [Fact]
        public void Normalize_BaseReferenceToDerivedObject_UsesRuntimeType()
        {
            // Arrange
            BaseDto instance = new DerivedDto { BaseValue = " Base ", DerivedValue = " Derived " };

            // Act
            instance.Normalize();

            // Assert
            instance.BaseValue.Should().Be("Base");
            ((DerivedDto)instance).DerivedValue.Should().Be("Derived");
        }

        [Fact]
        public void Normalize_ValueWithNoPublicSetter_ThrowsInvalidOperationException()
        {
            // Arrange
            NoPublicSetterDto instance = new(" Value ");

            // Act
            Action action = () => instance.Normalize();

            // Assert
            InvalidOperationException exception = action.Should().Throw<InvalidOperationException>().Which;
            exception.Message.Should().Contain(nameof(NoPublicSetterDto.Value));
            exception.Message.Should().Contain(nameof(NormalizeAttribute));
        }

        [Fact]
        public void Normalize_ValueWithNoPublicGetter_ThrowsInvalidOperationException()
        {
            // Arrange
            NoPublicGetterDto instance = new();
            instance.SetValue(" Value ");

            // Act
            Action action = () => instance.Normalize();

            // Assert
            InvalidOperationException exception = action.Should().Throw<InvalidOperationException>().Which;
            exception.Message.Should().Contain(nameof(NoPublicGetterDto.Value));
            exception.Message.Should().Contain("public getter");
        }

        [Fact]
        public void Normalize_UnsupportedValueType_ThrowsNotSupportedException()
        {
            // Arrange
            UnsupportedValueDto instance = new() { Value = 42 };

            // Act
            Action action = () => instance.Normalize();

            // Assert
            NotSupportedException exception = action.Should().Throw<NotSupportedException>().Which;
            exception.Message.Should().Contain(nameof(UnsupportedValueDto.Value));
            exception.Message.Should().Contain(typeof(int).FullName!);
        }

        [Fact]
        public void Normalize_DirectCollectionAttribute_ThrowsInvalidOperationException()
        {
            // Arrange
            WrongCollectionAttributeDto instance = new() { Children = [] };

            // Act
            Action action = () => instance.Normalize();

            // Assert
            InvalidOperationException exception = action.Should().Throw<InvalidOperationException>().Which;
            exception.Message.Should().Contain(nameof(WrongCollectionAttributeDto.Children));
            exception.Message.Should().Contain(nameof(NormalizeChildrenAttribute));
        }

        [Fact]
        public void NormalizeChildren_StringProperty_ThrowsInvalidOperationException()
        {
            // Arrange
            WrongChildrenStringDto instance = new() { Value = " Value " };

            // Act
            Action action = () => instance.Normalize();

            // Assert
            InvalidOperationException exception = action.Should().Throw<InvalidOperationException>().Which;
            exception.Message.Should().Contain(nameof(WrongChildrenStringDto.Value));
            exception.Message.Should().Contain(nameof(NormalizeChildrenAttribute));
        }

        [Fact]
        public void NormalizeChildren_NonEnumerableProperty_ThrowsInvalidOperationException()
        {
            // Arrange
            WrongChildrenScalarDto instance = new() { Child = new StringDto() };

            // Act
            Action action = () => instance.Normalize();

            // Assert
            InvalidOperationException exception = action.Should().Throw<InvalidOperationException>().Which;
            exception.Message.Should().Contain(nameof(WrongChildrenScalarDto.Child));
            exception.Message.Should().Contain(nameof(IEnumerable));
        }

        [Fact]
        public void Normalize_PropertyWithBothAttributes_ThrowsInvalidOperationException()
        {
            // Arrange
            ConflictingAttributesDto instance = new() { Value = [] };

            // Act
            Action action = () => instance.Normalize();

            // Assert
            InvalidOperationException exception = action.Should().Throw<InvalidOperationException>().Which;
            exception.Message.Should().Contain(nameof(ConflictingAttributesDto.Value));
            exception.Message.Should().Contain(nameof(NormalizeAttribute));
            exception.Message.Should().Contain(nameof(NormalizeChildrenAttribute));
        }

        public sealed class StringDto
        {
            [Normalize]
            public string? Value { get; set; }
        }

        private sealed class SetterTrackingDto
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

        private sealed class MultiplePropertiesDto
        {
            [Normalize]
            public string? FirstName { get; set; }

            [Normalize]
            public string? LastName { get; set; }

            public string? Notes { get; set; }
        }

        private sealed class UndecoratedDto
        {
            public string? Value { get; set; }
        }

        private sealed class ParentDto
        {
            [NormalizeChildren]
            public List<StringDto?>? Children { get; set; }
        }

        private sealed class ArrayParentDto
        {
            [NormalizeChildren]
            public StringDto[]? Children { get; set; }
        }

        private sealed class ReadOnlyParentDto(IReadOnlyList<StringDto> children)
        {
            [NormalizeChildren]
            public IReadOnlyList<StringDto> Children { get; } = children;
        }

        private sealed class RootDto
        {
            [NormalizeChildren]
            public List<BranchDto>? Branches { get; set; }
        }

        private sealed class BranchDto
        {
            [Normalize]
            public string? Name { get; set; }

            [NormalizeChildren]
            public List<StringDto>? Leaves { get; set; }
        }

        private class BaseDto
        {
            [Normalize]
            public string? BaseValue { get; set; }
        }

        private sealed class DerivedDto : BaseDto
        {
            [Normalize]
            public string? DerivedValue { get; set; }
        }

        private sealed class NoPublicSetterDto(string? value)
        {
            [Normalize]
            public string? Value { get; private set; } = value;
        }

        private sealed class NoPublicGetterDto
        {
            [Normalize]
            public string? Value { private get; set; }

            public void SetValue(string? value) => Value = value;
        }

        private sealed class UnsupportedValueDto
        {
            [Normalize]
            public int Value { get; set; }
        }

        private sealed class WrongCollectionAttributeDto
        {
            [Normalize]
            public List<StringDto>? Children { get; set; }
        }

        private sealed class WrongChildrenStringDto
        {
            [NormalizeChildren]
            public string? Value { get; set; }
        }

        private sealed class WrongChildrenScalarDto
        {
            [NormalizeChildren]
            public StringDto? Child { get; set; }
        }

        private sealed class ConflictingAttributesDto
        {
            [Normalize]
            [NormalizeChildren]
            public List<StringDto>? Value { get; set; }
        }
    }
}
