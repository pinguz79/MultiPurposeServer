using System.Text.Json.Serialization;

using MultiPurposeServer.Shared.Contracts.Abstractions;
using MultiPurposeServer.Shared.Utils.Attributes;

using Portfolio.Data.Enums;

using EnumDataType = System.ComponentModel.DataAnnotations.EnumDataTypeAttribute;

namespace Portfolio.Contracts.Bulk.Requests
{
    public sealed record BulkUpdateFotoItem(
        [property: Required] Guid Id,
        [property: Normalize, RequiredAtLeastOne] string? Description,
        [property: RequiredAtLeastOne, JsonConverter(typeof(JsonStringEnumConverter))]
        [param: EnumDataType(typeof(PhotoContentRating))]
        PhotoContentRating? ContentRating = null) : IRequest
    {
    }
}
