namespace MultiPurposeServer.Shared.Persistence.Operations
{
    public interface IApplicationOperationCheckpoint : IAsyncDisposable
    {
        Task Complete();
    }
}
