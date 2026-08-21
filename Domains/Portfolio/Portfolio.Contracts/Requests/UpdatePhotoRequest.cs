using System.Text.Json.Serialization;

using MultiPurposeServer.Shared.Contracts.Abstractions;
using MultiPurposeServer.Shared.Utils.Attributes;

using Portfolio.DataModel.Enums;

namespace Portfolio.Contracts.Requests
{
    public sealed record UpdatePhotoRequest(
        [property: Normalize, RequiredAtLeastOne] string? Description,
        [property: RequiredAtLeastOne, JsonConverter(typeof(JsonStringEnumConverter))]
        [param: System.ComponentModel.DataAnnotations.EnumDataType(typeof(PhotoContentRating))]
        PhotoContentRating? ContentRating = null) : IRequest
    {
    }
}
