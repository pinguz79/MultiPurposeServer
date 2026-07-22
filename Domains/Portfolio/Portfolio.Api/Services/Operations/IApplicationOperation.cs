namespace Portfolio.Api.Services.Operations
{
    public interface IApplicationOperation : IAsyncDisposable
    {
        Task Complete();
    }
}