using FluentAssertions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi;
using Moq;
using Portfolio.Api.Authentication;
using Portfolio.Api.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace Portfolio.Api.ControllerTests.Swagger
{
    public partial class PortfolioSecurityOperationFilterTests
    {
        private readonly PortfolioSecurityOperationFilter _filter = new();

        [Fact]
        public void Apply_WhenEndpointHasNoAuthorizeAttribute_DoesNotModifyOperation()
        {
            // Arrange
            var operation = new OpenApiOperation();
            var context = CreateContext<AnonymousController>(nameof(AnonymousController.Get));

            // Act
            _filter.Apply(operation, context);

            // Assert
            operation.Security.Should().BeNullOrEmpty();
            operation.Responses.Should().BeNullOrEmpty();
        }

        [Fact]
        public void Apply_WhenMethodHasFrontEndPolicy_AddsFrontEndAndBackEndRequirements()
        {
            // Arrange
            var operation = new OpenApiOperation();
            var context = CreateContext<MethodPolicyController>(nameof(MethodPolicyController.FrontEnd));

            // Act
            _filter.Apply(operation, context);

            // Assert
            AssertAuthenticationResponses(operation);
            GetSecuritySchemeNames(operation).Should().BeEquivalentTo(
            [
                PortfolioApiKeyAuthenticationDefaults.FrontEndSwaggerScheme,
                PortfolioApiKeyAuthenticationDefaults.BackEndSwaggerScheme
            ]);
        }

        [Fact]
        public void Apply_WhenMethodHasBackEndPolicy_AddsOnlyBackEndRequirement()
        {
            // Arrange
            var operation = new OpenApiOperation();
            var context = CreateContext<MethodPolicyController>(nameof(MethodPolicyController.BackEnd));

            // Act
            _filter.Apply(operation, context);

            // Assert
            AssertAuthenticationResponses(operation);
            GetSecuritySchemeNames(operation).Should().ContainSingle().Which.Should().Be(PortfolioApiKeyAuthenticationDefaults.BackEndSwaggerScheme);
        }

        [Fact]
        public void Apply_WhenControllerHasFrontEndPolicy_AddsFrontEndAndBackEndRequirements()
        {
            // Arrange
            var operation = new OpenApiOperation();
            var context = CreateContext<FrontEndController>(nameof(FrontEndController.Get));

            // Act
            _filter.Apply(operation, context);

            // Assert
            AssertAuthenticationResponses(operation);
            GetSecuritySchemeNames(operation).Should().BeEquivalentTo(
            [
                PortfolioApiKeyAuthenticationDefaults.FrontEndSwaggerScheme,
                PortfolioApiKeyAuthenticationDefaults.BackEndSwaggerScheme
            ]);
        }

        [Fact]
        public void Apply_WhenControllerHasBackEndPolicy_AddsOnlyBackEndRequirement()
        {
            // Arrange
            var operation = new OpenApiOperation();
            var context = CreateContext<BackEndController>(nameof(BackEndController.Get));

            // Act
            _filter.Apply(operation, context);

            // Assert
            AssertAuthenticationResponses(operation);
            GetSecuritySchemeNames(operation).Should().ContainSingle().Which.Should().Be(PortfolioApiKeyAuthenticationDefaults.BackEndSwaggerScheme);
        }

        [Fact]
        public void Apply_WhenControllerAndMethodHaveFrontEndPolicy_DoesNotDuplicateRequirements()
        {
            // Arrange
            var operation = new OpenApiOperation();
            var context = CreateContext<FrontEndController>(nameof(FrontEndController.FrontEnd));

            // Act
            _filter.Apply(operation, context);

            // Assert
            AssertAuthenticationResponses(operation);
            operation.Security.Should().HaveCount(2);

            GetSecuritySchemeNames(operation).Should().BeEquivalentTo(
            [
                PortfolioApiKeyAuthenticationDefaults.FrontEndSwaggerScheme,
                PortfolioApiKeyAuthenticationDefaults.BackEndSwaggerScheme
            ]);
        }

        [Fact]
        public void Apply_WhenBothFrontEndAndBackEndPoliciesArePresent_BackEndPolicyTakesPrecedence()
        {
            // Arrange
            var operation = new OpenApiOperation();
            var context = CreateContext<FrontEndController>(nameof(FrontEndController.BackEnd));

            // Act
            _filter.Apply(operation, context);

            // Assert
            AssertAuthenticationResponses(operation);
            GetSecuritySchemeNames(operation).Should().ContainSingle().Which.Should().Be(PortfolioApiKeyAuthenticationDefaults.BackEndSwaggerScheme);
        }

        [Fact]
        public void Apply_WhenAuthorizeAttributeHasNoPolicy_AddsAuthenticationResponsesWithoutSecurityRequirement()
        {
            // Arrange
            var operation = new OpenApiOperation();
            var context = CreateContext<MethodPolicyController>(nameof(MethodPolicyController.Authenticated));

            // Act
            _filter.Apply(operation, context);

            // Assert
            AssertAuthenticationResponses(operation);
            operation.Security.Should().BeNullOrEmpty();
        }

        [Fact]
        public void Apply_WhenAuthenticationResponsesAlreadyExist_DoesNotOverwriteThem()
        {
            // Arrange
            var unauthorizedResponse = new OpenApiResponse { Description = "Existing unauthorized response." };
            var forbiddenResponse = new OpenApiResponse { Description = "Existing forbidden response." };

            var operation = new OpenApiOperation
            {
                Responses = new OpenApiResponses
                {
                    ["401"] = unauthorizedResponse,
                    ["403"] = forbiddenResponse
                }
            };

            var context = CreateContext<MethodPolicyController>(nameof(MethodPolicyController.BackEnd));

            // Act
            _filter.Apply(operation, context);

            // Assert
            operation.Responses["401"].Should().BeSameAs(unauthorizedResponse);
            operation.Responses["403"].Should().BeSameAs(forbiddenResponse);
        }

        [Fact]
        public void Apply_WhenResponsesAreNull_CreatesResponseCollection()
        {
            // Arrange
            var operation = new OpenApiOperation();
            var context = CreateContext<MethodPolicyController>(nameof(MethodPolicyController.BackEnd));

            // Act
            _filter.Apply(operation, context);

            // Assert
            operation.Responses.Should().NotBeNull();
            operation.Responses.Should().ContainKeys("401", "403");
        }

        private static OperationFilterContext CreateContext<TController>(string methodName)
        {
            var methodInfo = typeof(TController).GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public)!;
            var schemaGenerator = new Mock<ISchemaGenerator>();
            var document = new OpenApiDocument
            {
                Components = new OpenApiComponents()
            };

            return new OperationFilterContext(new ApiDescription(), schemaGenerator.Object, new SchemaRepository(), document, methodInfo);
        }

        private static List<string> GetSecuritySchemeNames(OpenApiOperation operation)
        {
            return operation.Security?
                .SelectMany(requirement => requirement.Keys)
                .OfType<OpenApiSecuritySchemeReference>()
                .Select(reference => reference.Reference.Id)
                .ToList()
                ?? [];
        }

        private static void AssertAuthenticationResponses(OpenApiOperation operation)
        {
            operation.Responses.Should().ContainKeys("401", "403");
            operation.Responses["401"].Description.Should().Be("API key missing or invalid.");
            operation.Responses["403"].Description.Should().Be("The supplied API key does not grant access to this endpoint.");
        }
    }
}