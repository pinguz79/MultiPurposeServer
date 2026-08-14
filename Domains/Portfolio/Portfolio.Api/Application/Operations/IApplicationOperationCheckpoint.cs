namespace Portfolio.Api.Application.Operations
{
    public interface IApplicationOperationCheckpoint : IAsyncDisposable
    {
        Task Complete();
    }
}
