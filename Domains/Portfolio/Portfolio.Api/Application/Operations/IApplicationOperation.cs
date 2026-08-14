namespace Portfolio.Api.Application.Operations
{
    public interface IApplicationOperation : IAsyncDisposable
    {
        Task<IApplicationOperationCheckpoint> BeginCheckpoint();
        Task Complete();
    }
}
