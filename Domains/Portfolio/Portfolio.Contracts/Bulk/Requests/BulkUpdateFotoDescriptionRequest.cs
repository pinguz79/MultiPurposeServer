namespace Portfolio.Contracts.Bulk.Requests;

public sealed record BulkUpdateFotoDescriptionRequest(List<BulkUpdateFotoDescriptionItem> Items);
