namespace Portfolio.Contracts.Bulk.Requests;

public sealed record BulkUpdateFotoDescriptionItem(Guid Id, string NewDescription);