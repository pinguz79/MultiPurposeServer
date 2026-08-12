using System.Reflection;

using FluentAssertions;

using MultiPurposeServer.Shared.Contracts;
using MultiPurposeServer.Shared.Utils.Attributes;

using Portfolio.Contracts.Bulk.Requests;
using Portfolio.Contracts.Requests;

namespace Portfolio.ContractsTests
{
    public sealed class RequestNormalizationConfigurationTests
    {
        private static readonly HashSet<Type> NormalizableChildTypes =
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
            typeof(BulkUpdateAlbumRequest),
            typeof(BulkUpdateFotoItem),
            typeof(BulkUpdateFotoRequest),
            typeof(BulkOptions),
        ];

        [Theory]
        [InlineData(typeof(CreateAlbumRequest), nameof(CreateAlbumRequest.Name))]
        [InlineData(typeof(CreateAlbumRequest), nameof(CreateAlbumRequest.Description))]
        [InlineData(typeof(CreateAlbumRequest), nameof(CreateAlbumRequest.Path))]
        [InlineData(typeof(UpdateAlbumRequest), nameof(UpdateAlbumRequest.Name))]
        [InlineData(typeof(UpdateAlbumRequest), nameof(UpdateAlbumRequest.Description))]
        [InlineData(typeof(UpdatePhotoRequest), nameof(UpdatePhotoRequest.Description))]
        [InlineData(typeof(BulkUpdateAlbumItem), nameof(BulkUpdateAlbumItem.Name))]
        [InlineData(typeof(BulkUpdateAlbumItem), nameof(BulkUpdateAlbumItem.Description))]
        [InlineData(typeof(BulkUpdateFotoItem), nameof(BulkUpdateFotoItem.Description))]
        public void Property_WhenRequiresNormalization_HasNormalizeAttribute(Type requestType, string propertyName)
        {
            // Arrange
            PropertyInfo property = GetProperty(requestType, propertyName);

            // Act
            NormalizeAttribute? attribute = property.GetCustomAttribute<NormalizeAttribute>();

            // Assert
            attribute.Should().NotBeNull($"{requestType.Name}.{propertyName} must have [{nameof(NormalizeAttribute)}]");
        }

        [Theory]
        [InlineData(typeof(CreateAlbumRequest), nameof(CreateAlbumRequest.Parent))]
        [InlineData(typeof(CacheClearRequest), nameof(CacheClearRequest.ClearAlbumRoutingCache))]
        [InlineData(typeof(CacheClearRequest), nameof(CacheClearRequest.ClearPhotoRoutingCache))]
        [InlineData(typeof(CacheClearRequest), nameof(CacheClearRequest.ClearApiResponseCache))]
        [InlineData(typeof(BulkUpdateAlbumItem), nameof(BulkUpdateAlbumItem.Id))]
        [InlineData(typeof(BulkUpdateAlbumRequest), nameof(BulkUpdateAlbumRequest.Options))]
        [InlineData(typeof(BulkUpdateAlbumRequest), nameof(BulkUpdateAlbumRequest.Items))]
        [InlineData(typeof(BulkUpdateFotoItem), nameof(BulkUpdateFotoItem.Id))]
        [InlineData(typeof(BulkUpdateFotoRequest), nameof(BulkUpdateFotoRequest.Options))]
        [InlineData(typeof(BulkUpdateFotoRequest), nameof(BulkUpdateFotoRequest.Items))]
        [InlineData(typeof(BulkOptions), nameof(BulkOptions.ErrorStrategy))]
        public void Property_WhenDoesNotRequireNormalization_DoesNotHaveNormalizeAttribute(Type requestType, string propertyName)
        {
            // Arrange
            PropertyInfo property = GetProperty(requestType, propertyName);

            // Act
            NormalizeAttribute? attribute = property.GetCustomAttribute<NormalizeAttribute>();

            // Assert
            attribute.Should().BeNull($"{requestType.Name}.{propertyName} must not have [{nameof(NormalizeAttribute)}]");
        }

        [Fact]
        public void ChildCollection_WhenElementNormalizationRequirementIsEvaluated_HasConsistentConfiguration()
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
                    {
                        continue;
                    }

                    bool elementRequiresNormalization = NormalizableChildTypes.Contains(elementType);
                    bool hasNormalizeChildren = property.GetCustomAttribute<NormalizeChildrenAttribute>() is not null;

                    if (elementRequiresNormalization == hasNormalizeChildren)
                    {
                        continue;
                    }

                    string requirement = elementRequiresNormalization ? "requires" : "does not require";
                    string presence = hasNormalizeChildren ? "has" : "does not have";
                    inconsistencies.Add($"{requestType.Name}.{property.Name} {presence} [{nameof(NormalizeChildrenAttribute)}], but {elementType.Name} {requirement} normalization.");
                }
            }

            // Assert
            inconsistencies.Should().BeEmpty("parent and child normalization configuration must be consistent");
        }

        private static Type? GetCollectionElementType(Type propertyType)
        {
            if (propertyType == typeof(string))
            {
                return null;
            }

            if (propertyType.IsArray)
            {
                return propertyType.GetElementType();
            }

            if (IsGenericEnumerable(propertyType))
            {
                return propertyType.GetGenericArguments()[0];
            }

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
