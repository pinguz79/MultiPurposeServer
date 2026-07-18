namespace Portfolio.Api.Services.Models;

public sealed record BulkUpdateItem<TValue>(Guid Id, TValue Value);