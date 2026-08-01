using FluentAssertions;
using MultiPurposeServer.Shared.Contracts;
using MultiPurposeServer.Shared.Contracts.Abstractions;
using MultiPurposeServer.Shared.Utils.Attributes;
using Portfolio.Contracts.Bulk.Requests;
using Portfolio.Contracts.Requests;
using System.Reflection;

namespace Portfolio.ContractsTests
{
    public sealed class RequestValidationConfigurationTests
    {
        private static readonly HashSet<Type> ChildTypesRequiringRecursiveValidation =
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
            typeof(BulkUpdateFotoRequest)
        ];

        private static readonly Type[] ValidationConfigurationTypes =
        [
            .. RequestTypes,
            typeof(BulkOptions)
        ];

        [Fact]
        public void RequestTypes_WhenComparedWithContractsAssembly_ContainsAllConcreteRequests()
        {
            Type[] discoveredRequestTypes = typeof(CacheClearRequest).Assembly
                .GetTypes()
                .Where(type =>
                    type is { IsClass: true, IsAbstract: false } &&
                    typeof(IRequest).IsAssignableFrom(type))
                .ToArray();

            RequestTypes.Should().BeEquivalentTo(
                discoveredRequestTypes,
                "every concrete IRequest contract must be included in the validation configuration tests");
        }

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
            PropertyInfo property = GetProperty(requestType, propertyName);

            RequiredAttribute? attribute = property.GetCustomAttribute<RequiredAttribute>();

            attribute.Should().NotBeNull(
                $"{requestType.Name}.{propertyName} must have [{nameof(RequiredAttribute)}]");
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
        public void Property_WhenIsNotRequired_DoesNotHaveRequiredAttribute(Type requestType, string propertyName)
        {
            PropertyInfo property = GetProperty(requestType, propertyName);

            RequiredAttribute? attribute = property.GetCustomAttribute<RequiredAttribute>();

            attribute.Should().BeNull(
                $"{requestType.Name}.{propertyName} must not have [{nameof(RequiredAttribute)}]");
        }

        [Theory]
        [InlineData(typeof(UpdateAlbumRequest), nameof(UpdateAlbumRequest.Name))]
        [InlineData(typeof(UpdateAlbumRequest), nameof(UpdateAlbumRequest.Description))]
        [InlineData(typeof(BulkUpdateAlbumItem), nameof(BulkUpdateAlbumItem.Name))]
        [InlineData(typeof(BulkUpdateAlbumItem), nameof(BulkUpdateAlbumItem.Description))]
        public void Property_WhenBelongsToRequiredAtLeastOneGroup_HasRequiredAtLeastOneAttribute(
            Type requestType,
            string propertyName)
        {
            PropertyInfo property = GetProperty(requestType, propertyName);

            RequiredAtLeastOneAttribute? attribute =
                property.GetCustomAttribute<RequiredAtLeastOneAttribute>();

            attribute.Should().NotBeNull(
                $"{requestType.Name}.{propertyName} must have [{nameof(RequiredAtLeastOneAttribute)}]");
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
        [InlineData(typeof(BulkUpdateFotoRequest), nameof(BulkUpdateFotoRequest.Options))]
        [InlineData(typeof(BulkUpdateFotoRequest), nameof(BulkUpdateFotoRequest.Items))]
        public void Property_WhenDoesNotBelongToRequiredAtLeastOneGroup_DoesNotHaveRequiredAtLeastOneAttribute(
            Type requestType,
            string propertyName)
        {
            PropertyInfo property = GetProperty(requestType, propertyName);

            RequiredAtLeastOneAttribute? attribute =
                property.GetCustomAttribute<RequiredAtLeastOneAttribute>();

            attribute.Should().BeNull(
                $"{requestType.Name}.{propertyName} must not have [{nameof(RequiredAtLeastOneAttribute)}]");
        }

        [Theory]
        [InlineData(typeof(UpdateAlbumRequest))]
        [InlineData(typeof(BulkUpdateAlbumItem))]
        public void Request_WhenRequiredAtLeastOnePropertiesAreEvaluated_UsesSingleGroup(Type requestType)
        {
            string[] groups = requestType
                .GetProperties()
                .Select(property => property.GetCustomAttribute<RequiredAtLeastOneAttribute>())
                .Where(attribute => attribute is not null)
                .Select(attribute => attribute!.Group)
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            groups.Should().ContainSingle(
                $"{requestType.Name} required-at-least-one properties must belong to the same group");
        }

        [Theory]
        [InlineData(typeof(UpdateAlbumRequest))]
        [InlineData(typeof(BulkUpdateAlbumItem))]
        public void Request_WhenRequiredAtLeastOneGroupIsEvaluated_ContainsExpectedProperties(Type requestType)
        {
            string[] propertyNames = requestType
                .GetProperties()
                .Where(property =>
                    property.GetCustomAttribute<RequiredAtLeastOneAttribute>() is not null)
                .Select(property => property.Name)
                .ToArray();

            propertyNames.Should().BeEquivalentTo(
            [
                nameof(UpdateAlbumRequest.Name),
                nameof(UpdateAlbumRequest.Description)
            ]);
        }

        [Fact]
        public void ChildCollection_WhenElementValidationRequirementIsEvaluated_HasConsistentConfiguration()
        {
            List<string> inconsistencies = [];

            foreach (Type requestType in ValidationConfigurationTypes)
            {
                foreach (PropertyInfo property in requestType.GetProperties())
                {
                    Type? elementType = GetCollectionElementType(property.PropertyType);

                    if (elementType is null)
                        continue;

                    bool elementRequiresValidation =
                        ChildTypesRequiringRecursiveValidation.Contains(elementType);
                    bool hasValidateChildren =
                        property.GetCustomAttribute<ValidateChildrenAttribute>() is not null;

                    if (elementRequiresValidation == hasValidateChildren)
                        continue;

                    string requirement = elementRequiresValidation ? "requires" : "does not require";
                    string presence = hasValidateChildren ? "has" : "does not have";

                    inconsistencies.Add(
                        $"{requestType.Name}.{property.Name} {presence} " +
                        $"[{nameof(ValidateChildrenAttribute)}], but {elementType.Name} " +
                        $"{requirement} validation.");
                }
            }

            inconsistencies.Should().BeEmpty(
                "parent and child validation configuration must be consistent");
        }

        [Theory]
        [InlineData(typeof(CacheClearRequest), nameof(CacheClearRequest.ClearAlbumRoutingCache))]
        [InlineData(typeof(CacheClearRequest), nameof(CacheClearRequest.ClearPhotoRoutingCache))]
        [InlineData(typeof(CacheClearRequest), nameof(CacheClearRequest.ClearApiResponseCache))]
        public void Property_WhenBelongsToRequiredAtLeastOneTrueGroup_HasRequiredAtLeastOneTrueAttribute(
            Type requestType,
            string propertyName)
        {
            PropertyInfo property = GetProperty(requestType, propertyName);

            RequiredAtLeastOneTrueAttribute? attribute =
                property.GetCustomAttribute<RequiredAtLeastOneTrueAttribute>();

            attribute.Should().NotBeNull(
                $"{requestType.Name}.{propertyName} must have " +
                $"[{nameof(RequiredAtLeastOneTrueAttribute)}]");
        }

        [Fact]
        public void CacheClearRequest_WhenBooleanPropertiesBelongToSameGroup_HasConsistentRequiredAtLeastOneTrueGroup()
        {
            string[] propertyNames =
            [
                nameof(CacheClearRequest.ClearAlbumRoutingCache),
                nameof(CacheClearRequest.ClearPhotoRoutingCache),
                nameof(CacheClearRequest.ClearApiResponseCache)
            ];

            string[] groups = propertyNames
                .Select(propertyName =>
                    GetProperty(typeof(CacheClearRequest), propertyName)
                        .GetCustomAttribute<RequiredAtLeastOneTrueAttribute>()!
                        .Group)
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            groups.Should().ContainSingle(
                "all cache flags must belong to the same validation group");
        }

        [Fact]
        public void CacheClearRequest_WhenRequiredAtLeastOneTrueGroupIsEvaluated_ContainsAllCacheFlags()
        {
            string[] propertyNames = typeof(CacheClearRequest)
                .GetProperties()
                .Where(property =>
                    property.GetCustomAttribute<RequiredAtLeastOneTrueAttribute>() is not null)
                .Select(property => property.Name)
                .ToArray();

            propertyNames.Should().BeEquivalentTo(
            [
                nameof(CacheClearRequest.ClearAlbumRoutingCache),
                nameof(CacheClearRequest.ClearPhotoRoutingCache),
                nameof(CacheClearRequest.ClearApiResponseCache)
            ]);
        }

        private static Type? GetCollectionElementType(Type propertyType)
        {
            if (propertyType == typeof(string))
                return null;

            if (propertyType.IsArray)
                return propertyType.GetElementType();

            if (IsGenericEnumerable(propertyType))
                return propertyType.GetGenericArguments()[0];

            Type? enumerableType = propertyType
                .GetInterfaces()
                .FirstOrDefault(IsGenericEnumerable);

            return enumerableType?.GetGenericArguments()[0];
        }

        private static bool IsGenericEnumerable(Type type)
        {
            return type.IsGenericType &&
                   type.GetGenericTypeDefinition() == typeof(IEnumerable<>);
        }

        private static PropertyInfo GetProperty(Type requestType, string propertyName)
        {
            PropertyInfo? property = requestType.GetProperty(propertyName);

            property.Should().NotBeNull(
                $"{requestType.Name} must expose the public property {propertyName}");

            return property!;
        }
    }
}
