namespace Skating.Desktop.Tests
{
    internal static class WinFormsTest
    {
        internal static Task Run(Action action)
        {
            var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var thread = new Thread(() =>
            {
                try
                {
                    action();
                    completion.SetResult();
                }
                catch (Exception exception)
                {
                    completion.SetException(exception);
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Start();
            return completion.Task.WaitAsync(TimeSpan.FromSeconds(20));
        }
    }
}
