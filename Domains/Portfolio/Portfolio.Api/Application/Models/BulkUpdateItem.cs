namespace Portfolio.Api.Application.Models
{
    public sealed record BulkUpdateItem<TValue>(Guid Id, TValue Value);
}