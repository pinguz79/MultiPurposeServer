using System.Net;
using System.Net.Http.Json;

using FluentAssertions;

using Moq;

using Portfolio.Api.Application.Operations;
using Portfolio.Api.IntegrationTests.Infrastructure;
using Portfolio.Contracts.Bulk.Requests;
using Portfolio.Contracts.Requests;
using Portfolio.Data.Enums;
using Portfolio.Data.Models;

namespace Portfolio.Api.IntegrationTests.Pipeline
{
    public class RequestPipelineTests
    {
        [Fact]
        public async Task CreateAlbum_WhenRequestIsValid_BindsNormalizesAndCallsService()
        {
            // Arrange
            await using var host = new PortfolioApiTestHost();
            var album = new Album { Id = Guid.NewGuid(), Name = "Fashion", Path = "Fashion" };
            var request = new CreateAlbumRequest("  Fashion  ");

            host.AlbumService.Setup(service => service.CreateAlbum("Fashion", null, null, null)).ReturnsAsync(album);

            // Act
            var response = await host.Client.PostAsJsonAsync("/Portfolio/BackEnd/Album/CreateNew", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            host.AlbumService.Verify(service => service.CreateAlbum("Fashion", null, null, null), Times.Once);
        }

        [Fact]
        public async Task CreateAlbum_WhenNormalizedRequestIsInvalid_ReturnsBadRequestWithoutCallingService()
        {
            // Arrange
            await using var host = new PortfolioApiTestHost();
            var request = new CreateAlbumRequest("   ");

            // Act
            var response = await host.Client.PostAsJsonAsync("/Portfolio/BackEnd/Album/CreateNew", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            host.AlbumService.Verify(service => service.CreateAlbum(It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<string?>(), It.IsAny<string?>()), Times.Never);
        }

        [Fact]
        public async Task BulkUpdateAlbum_WhenNestedItemIsInvalid_ReturnsBadRequestWithoutCallingService()
        {
            // Arrange
            await using var host = new PortfolioApiTestHost();
            var request = new BulkUpdateAlbumRequest(new(), [new BulkUpdateAlbumItem(Guid.NewGuid(), "   ", null)]);

            // Act
            var response = await host.Client.PutAsJsonAsync("/Portfolio/BackEnd/Bulk/Album/Update", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            host.AlbumService.Verify(service => service.BeginOperation(), Times.Never);
        }

        [Fact]
        public async Task BulkUpdateAlbum_WhenItemsAreEmpty_ReturnsBadRequestWithoutCallingService()
        {
            // Arrange
            await using var host = new PortfolioApiTestHost();
            var request = new BulkUpdateAlbumRequest(new(), []);

            // Act
            var response = await host.Client.PutAsJsonAsync("/Portfolio/BackEnd/Bulk/Album/Update", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            host.AlbumService.Verify(service => service.BeginOperation(), Times.Never);
        }

        [Fact]
        public async Task BulkUpdateAlbum_WhenRequestContainsDuplicateIds_ReturnsBadRequestWithoutCallingService()
        {
            // Arrange
            await using var host = new PortfolioApiTestHost();
            var albumId = Guid.NewGuid();
            var request = new BulkUpdateAlbumRequest(new(),
            [
                new BulkUpdateAlbumItem(albumId, "Fashion", null),
                new BulkUpdateAlbumItem(albumId, null, "Description")
            ]);

            // Act
            var response = await host.Client.PutAsJsonAsync("/Portfolio/BackEnd/Bulk/Album/Update", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            host.AlbumService.Verify(service => service.BeginOperation(), Times.Never);
        }

        [Fact]
        public async Task UpdateAlbum_WhenRequestHasNoFields_ReturnsBadRequestWithoutCallingService()
        {
            // Arrange
            await using var host = new PortfolioApiTestHost();
            var request = new UpdateAlbumRequest(null, null);

            // Act
            var response = await host.Client.PutAsJsonAsync($"/Portfolio/BackEnd/Album/{Guid.NewGuid()}", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            host.AlbumService.Verify(service => service.BeginOperation(), Times.Never);
        }

        [Fact]
        public async Task UpdateAlbum_WhenFieldsContainOuterSpaces_PassesNormalizedValuesToService()
        {
            // Arrange
            await using var host = new PortfolioApiTestHost();
            var albumId = Guid.NewGuid();
            var request = new UpdateAlbumRequest("  Fashion  ", "  Editorial  ");
            var operation = new Mock<IApplicationOperation>();
            var album = new Album { Id = albumId, Name = "Fashion", Description = "Editorial", Path = "Fashion" };

            host.AlbumService.Setup(service => service.BeginOperation()).ReturnsAsync(operation.Object);
            host.AlbumService.Setup(service => service.UpdateName(albumId, "Fashion")).ReturnsAsync(album);
            host.AlbumService.Setup(service => service.UpdateDescription(albumId, "Editorial")).ReturnsAsync(album);
            operation.Setup(value => value.Complete()).Returns(Task.CompletedTask);

            // Act
            var response = await host.Client.PutAsJsonAsync($"/Portfolio/BackEnd/Album/{albumId}", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            host.AlbumService.Verify(service => service.UpdateName(albumId, "Fashion"), Times.Once);
            host.AlbumService.Verify(service => service.UpdateDescription(albumId, "Editorial"), Times.Once);
        }

        [Fact]
        public async Task BulkUpdateFoto_WhenItemsAreEmpty_ReturnsBadRequestWithoutCallingService()
        {
            // Arrange
            await using var host = new PortfolioApiTestHost();
            var request = new BulkUpdateFotoRequest(new(), []);

            // Act
            var response = await host.Client.PutAsJsonAsync("/Portfolio/BackEnd/Bulk/Foto/Update", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            host.FotoService.Verify(service => service.BeginOperation(), Times.Never);
        }

        [Fact]
        public async Task BulkUpdateFoto_WhenNestedItemIsInvalid_ReturnsBadRequestWithoutCallingService()
        {
            // Arrange
            await using var host = new PortfolioApiTestHost();
            var request = new BulkUpdateFotoRequest(new(), [new BulkUpdateFotoItem(Guid.NewGuid(), "   ")]);

            // Act
            var response = await host.Client.PutAsJsonAsync("/Portfolio/BackEnd/Bulk/Foto/Update", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            host.FotoService.Verify(service => service.BeginOperation(), Times.Never);
        }

        [Fact]
        public async Task BulkUpdateFoto_WhenRequestContainsDuplicateIds_ReturnsBadRequestWithoutCallingService()
        {
            // Arrange
            await using var host = new PortfolioApiTestHost();
            var photoId = Guid.NewGuid();
            var request = new BulkUpdateFotoRequest(new(),
            [
                new BulkUpdateFotoItem(photoId, "Portrait"),
                new BulkUpdateFotoItem(photoId, null, PhotoContentRating.Restricted)
            ]);

            // Act
            var response = await host.Client.PutAsJsonAsync("/Portfolio/BackEnd/Bulk/Foto/Update", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            host.FotoService.Verify(service => service.BeginOperation(), Times.Never);
        }

        [Fact]
        public async Task UpdatePhoto_WhenRequestHasNoFields_ReturnsBadRequestWithoutCallingService()
        {
            // Arrange
            await using var host = new PortfolioApiTestHost();
            var request = new UpdatePhotoRequest(null, null);

            // Act
            var response = await host.Client.PutAsJsonAsync($"/Portfolio/BackEnd/Foto/{Guid.NewGuid()}", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            host.FotoService.Verify(service => service.BeginOperation(), Times.Never);
        }

        [Fact]
        public async Task UpdatePhoto_WhenDescriptionContainsOuterSpaces_PassesNormalizedDescriptionToService()
        {
            // Arrange
            await using var host = new PortfolioApiTestHost();
            var photoId = Guid.NewGuid();
            var request = new UpdatePhotoRequest("  Portrait  ");
            var operation = new Mock<IApplicationOperation>();
            var photo = new Foto { Id = photoId, FileName = "Portrait.jpg", Description = "Portrait", ContentRating = PhotoContentRating.Standard };

            host.FotoService.Setup(service => service.BeginOperation()).ReturnsAsync(operation.Object);
            host.FotoService.Setup(service => service.UpdateDescription(photoId, "Portrait")).ReturnsAsync(photo);
            operation.Setup(value => value.Complete()).Returns(Task.CompletedTask);

            // Act
            var response = await host.Client.PutAsJsonAsync($"/Portfolio/BackEnd/Foto/{photoId}", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            host.FotoService.Verify(service => service.UpdateDescription(photoId, "Portrait"), Times.Once);
        }

        [Fact]
        public async Task ClearCache_WhenNoCacheIsSelected_ReturnsBadRequestWithoutCallingService()
        {
            // Arrange
            await using var host = new PortfolioApiTestHost();
            var request = new CacheClearRequest(false, false, false);

            // Act
            var response = await host.Client.PostAsJsonAsync("/Portfolio/BackEnd/Cache/Clear", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            host.CacheService.Verify(service => service.Clear(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAlbum_WhenServiceThrowsKeyNotFoundException_ReturnsNotFound()
        {
            // Arrange
            await using var host = new PortfolioApiTestHost();
            var albumId = Guid.NewGuid();

            host.AlbumService.Setup(service => service.DeleteEmptyAlbum(albumId)).ThrowsAsync(new KeyNotFoundException());

            // Act
            var response = await host.Client.DeleteAsync($"/Portfolio/BackEnd/Album/{albumId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task UpdateAlbum_WhenServiceThrowsKeyNotFoundException_ReturnsNotFoundWithoutCompletingOperation()
        {
            // Arrange
            await using var host = new PortfolioApiTestHost();
            var albumId = Guid.NewGuid();
            var request = new UpdateAlbumRequest("Fashion", null);
            var operation = new Mock<IApplicationOperation>();

            host.AlbumService.Setup(service => service.BeginOperation()).ReturnsAsync(operation.Object);
            host.AlbumService.Setup(service => service.UpdateName(albumId, "Fashion")).ThrowsAsync(new KeyNotFoundException());

            // Act
            var response = await host.Client.PutAsJsonAsync($"/Portfolio/BackEnd/Album/{albumId}", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            operation.Verify(value => value.Complete(), Times.Never);
            operation.Verify(value => value.DisposeAsync(), Times.Once);
        }
    }
}
