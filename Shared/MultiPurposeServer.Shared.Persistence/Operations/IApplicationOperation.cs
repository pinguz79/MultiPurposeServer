namespace MultiPurposeServer.Shared.Persistence.Operations
{
    public interface IApplicationOperation : IAsyncDisposable
    {
        Task<IApplicationOperationCheckpoint> BeginCheckpoint();
        Task Complete();
    }
}
