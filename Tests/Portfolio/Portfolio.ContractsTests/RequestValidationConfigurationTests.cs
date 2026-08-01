using FluentAssertions;
using MultiPurposeServer.Shared.Contracts;
using MultiPurposeServer.Shared.Utils.Attributes;
using Portfolio.Contracts.Bulk.Requests;
using Portfolio.Contracts.Requests;
using System.Reflection;

namespace Portfolio.ContractsTests
{
    public sealed class RequestValidationConfigurationTests
    {
        private static readonly HashSet<Type> ValidatableChildTypes =
        [
            typeof(BulkUpdateAlbumItem),
            typeof(BulkUpdateFotoItem)
        ];

        private static readonly Type[] RequestTypes =
        [
            typeof(CacheClearRequest),
            typeof(CreateAlbumRequest),
            typeof(UpdateAlbumRequest),
            typeof(UpdatePhotoRequest),
            typeof(BulkUpdateAlbumItem),
            typeof(BulkOptions),
            typeof(BulkUpdateAlbumRequest),
            typeof(BulkUpdateFotoItem),
            typeof(BulkOptions),
            typeof(BulkUpdateFotoRequest)
        ];

        [Theory]
        [InlineData(typeof(CreateAlbumRequest), nameof(CreateAlbumRequest.Name))]
        [InlineData(typeof(UpdatePhotoRequest), nameof(UpdatePhotoRequest.Description))]
        [InlineData(typeof(BulkUpdateAlbumItem), nameof(BulkUpdateAlbumItem.Id))]
        [InlineData(typeof(BulkUpdateAlbumRequest), nameof(BulkUpdateAlbumRequest.Options))]
        [InlineData(typeof(BulkUpdateAlbumRequest), nameof(BulkUpdateAlbumRequest.Items))]
        [InlineData(typeof(BulkUpdateFotoItem), nameof(BulkUpdateFotoItem.Id))]
        [InlineData(typeof(BulkUpdateFotoItem), nameof(BulkUpdateFotoItem.Description))]
        [InlineData(typeof(BulkUpdateFotoRequest), nameof(BulkUpdateFotoRequest.Options))]
        [InlineData(typeof(BulkUpdateFotoRequest), nameof(BulkUpdateFotoRequest.Items))]
        public void Property_WhenIsRequired_HasRequiredAttribute(Type requestType, string propertyName)
        {
            // Arrange
            PropertyInfo property = GetProperty(requestType, propertyName);

            // Act
            RequiredAttribute? attribute = property.GetCustomAttribute<RequiredAttribute>();

            // Assert
            attribute.Should().NotBeNull($"{requestType.Name}.{propertyName} must have [{nameof(RequiredAttribute)}]");
        }

        [Theory]
        [InlineData(typeof(CacheClearRequest), nameof(CacheClearRequest.ClearAlbumRoutingCache))]
        [InlineData(typeof(CacheClearRequest), nameof(CacheClearRequest.ClearPhotoRoutingCache))]
        [InlineData(typeof(CacheClearRequest), nameof(CacheClearRequest.ClearApiResponseCache))]
        [InlineData(typeof(CreateAlbumRequest), nameof(CreateAlbumRequest.Parent))]
        [InlineData(typeof(CreateAlbumRequest), nameof(CreateAlbumRequest.Description))]
        [InlineData(typeof(UpdateAlbumRequest), nameof(UpdateAlbumRequest.Name))]
        [InlineData(typeof(UpdateAlbumRequest), nameof(UpdateAlbumRequest.Description))]
        [InlineData(typeof(BulkUpdateAlbumItem), nameof(BulkUpdateAlbumItem.Name))]
        [InlineData(typeof(BulkUpdateAlbumItem), nameof(BulkUpdateAlbumItem.Description))]
        [InlineData(typeof(BulkOptions), nameof(BulkOptions.ErrorStrategy))]
        [InlineData(typeof(BulkOptions), nameof(BulkOptions.ErrorStrategy))]
        public void Property_WhenIsNotRequired_DoesNotHaveRequiredAttribute(Type requestType, string propertyName)
        {
            // Arrange
            PropertyInfo property = GetProperty(requestType, propertyName);

            // Act
            RequiredAttribute? attribute = property.GetCustomAttribute<RequiredAttribute>();

            // Assert
            attribute.Should().BeNull($"{requestType.Name}.{propertyName} must not have [{nameof(RequiredAttribute)}]");
        }

        [Theory]
        [InlineData(typeof(UpdateAlbumRequest), nameof(UpdateAlbumRequest.Name))]
        [InlineData(typeof(UpdateAlbumRequest), nameof(UpdateAlbumRequest.Description))]
        [InlineData(typeof(BulkUpdateAlbumItem), nameof(BulkUpdateAlbumItem.Name))]
        [InlineData(typeof(BulkUpdateAlbumItem), nameof(BulkUpdateAlbumItem.Description))]
        public void Property_WhenBelongsToRequiredAtLeastOneGroup_HasRequiredAtLeastOneAttribute(Type requestType, string propertyName)
        {
            // Arrange
            PropertyInfo property = GetProperty(requestType, propertyName);

            // Act
            RequiredAtLeastOneAttribute? attribute = property.GetCustomAttribute<RequiredAtLeastOneAttribute>();

            // Assert
            attribute.Should().NotBeNull($"{requestType.Name}.{propertyName} must have [{nameof(RequiredAtLeastOneAttribute)}]");
        }

        [Theory]
        [InlineData(typeof(CacheClearRequest), nameof(CacheClearRequest.ClearAlbumRoutingCache))]
        [InlineData(typeof(CacheClearRequest), nameof(CacheClearRequest.ClearPhotoRoutingCache))]
        [InlineData(typeof(CacheClearRequest), nameof(CacheClearRequest.ClearApiResponseCache))]
        [InlineData(typeof(CreateAlbumRequest), nameof(CreateAlbumRequest.Name))]
        [InlineData(typeof(CreateAlbumRequest), nameof(CreateAlbumRequest.Parent))]
        [InlineData(typeof(CreateAlbumRequest), nameof(CreateAlbumRequest.Description))]
        [InlineData(typeof(UpdatePhotoRequest), nameof(UpdatePhotoRequest.Description))]
        [InlineData(typeof(BulkUpdateAlbumItem), nameof(BulkUpdateAlbumItem.Id))]
        [InlineData(typeof(BulkOptions), nameof(BulkOptions.ErrorStrategy))]
        [InlineData(typeof(BulkUpdateAlbumRequest), nameof(BulkUpdateAlbumRequest.Options))]
        [InlineData(typeof(BulkUpdateAlbumRequest), nameof(BulkUpdateAlbumRequest.Items))]
        [InlineData(typeof(BulkUpdateFotoItem), nameof(BulkUpdateFotoItem.Id))]
        [InlineData(typeof(BulkUpdateFotoItem), nameof(BulkUpdateFotoItem.Description))]
        [InlineData(typeof(BulkOptions), nameof(BulkOptions.ErrorStrategy))]
        [InlineData(typeof(BulkUpdateFotoRequest), nameof(BulkUpdateFotoRequest.Options))]
        [InlineData(typeof(BulkUpdateFotoRequest), nameof(BulkUpdateFotoRequest.Items))]
        public void Property_WhenDoesNotBelongToRequiredAtLeastOneGroup_DoesNotHaveRequiredAtLeastOneAttribute(Type requestType, string propertyName)
        {
            // Arrange
            PropertyInfo property = GetProperty(requestType, propertyName);

            // Act
            RequiredAtLeastOneAttribute? attribute = property.GetCustomAttribute<RequiredAtLeastOneAttribute>();

            // Assert
            attribute.Should().BeNull($"{requestType.Name}.{propertyName} must not have [{nameof(RequiredAtLeastOneAttribute)}]");
        }

        [Fact]
        public void ChildCollection_WhenElementValidationRequirementIsEvaluated_HasConsistentConfiguration()
        {
            // Arrange
            List<string> inconsistencies = [];

            // Act
            foreach (Type requestType in RequestTypes)
            {
                foreach (PropertyInfo property in requestType.GetProperties())
                {
                    Type? elementType = GetCollectionElementType(property.PropertyType);

                    if (elementType is null)
                        continue;

                    bool elementRequiresValidation = ValidatableChildTypes.Contains(elementType);
                    bool hasValidateChildren = property.GetCustomAttribute<ValidateChildrenAttribute>() is not null;

                    if (elementRequiresValidation == hasValidateChildren)
                        continue;

                    string requirement = elementRequiresValidation ? "requires" : "does not require";
                    string presence = hasValidateChildren ? "has" : "does not have";
                    inconsistencies.Add($"{requestType.Name}.{property.Name} {presence} [{nameof(ValidateChildrenAttribute)}], but {elementType.Name} {requirement} validation.");
                }
            }

            // Assert
            inconsistencies.Should().BeEmpty("parent and child validation configuration must be consistent");
        }

        [Theory]
        [InlineData(typeof(CacheClearRequest), nameof(CacheClearRequest.ClearAlbumRoutingCache))]
        [InlineData(typeof(CacheClearRequest), nameof(CacheClearRequest.ClearPhotoRoutingCache))]
        [InlineData(typeof(CacheClearRequest), nameof(CacheClearRequest.ClearApiResponseCache))]
        public void Property_WhenBelongsToRequiredAtLeastOneTrueGroup_HasRequiredAtLeastOneTrueAttribute(Type requestType, string propertyName)
        {
            // Arrange
            PropertyInfo property = GetProperty(requestType, propertyName);

            // Act
            RequiredAtLeastOneTrueAttribute? attribute = property.GetCustomAttribute<RequiredAtLeastOneTrueAttribute>();

            // Assert
            attribute.Should().NotBeNull($"{requestType.Name}.{propertyName} must have [{nameof(RequiredAtLeastOneTrueAttribute)}]");
        }

        [Fact]
        public void CacheClearRequest_WhenBooleanPropertiesBelongToSameGroup_HasConsistentRequiredAtLeastOneTrueGroup()
        {
            // Arrange
            string[] propertyNames =
            [
                nameof(CacheClearRequest.ClearAlbumRoutingCache),
                nameof(CacheClearRequest.ClearPhotoRoutingCache),
                nameof(CacheClearRequest.ClearApiResponseCache)
            ];

            // Act
            string[] groups = propertyNames.Select(propertyName => GetProperty(typeof(CacheClearRequest), propertyName).GetCustomAttribute<RequiredAtLeastOneTrueAttribute>()!.Group).Distinct().ToArray();

            // Assert
            groups.Should().ContainSingle("all cache flags must belong to the same validation group");
        }


        private static Type? GetCollectionElementType(Type propertyType)
        {
            if (propertyType == typeof(string))
                return null;

            if (propertyType.IsArray)
                return propertyType.GetElementType();

            if (IsGenericEnumerable(propertyType))
                return propertyType.GetGenericArguments()[0];

            Type? enumerableType = propertyType.GetInterfaces().FirstOrDefault(IsGenericEnumerable);

            return enumerableType?.GetGenericArguments()[0];
        }

        private static bool IsGenericEnumerable(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>);
        }

        private static PropertyInfo GetProperty(Type requestType, string propertyName)
        {
            PropertyInfo? property = requestType.GetProperty(propertyName);

            property.Should().NotBeNull($"{requestType.Name} must expose the public property {propertyName}");

            return property!;
        }
    }
}
