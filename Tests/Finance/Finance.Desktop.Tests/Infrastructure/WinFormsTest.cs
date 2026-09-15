namespace Finance.Desktop.Tests.Infrastructure
{
    public static class WinFormsTest
    {
        public static Task Run(Func<Task> action)
        {
            var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var thread = new Thread(() =>
            {
                SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());
                SynchronizationContext.Current!.Post(async _ =>
                {
                    try
                    {
                        await action();
                        completion.SetResult();
                    }
                    catch (Exception exception)
                    {
                        completion.SetException(exception);
                    }
                    finally
                    {
                        Application.ExitThread();
                    }
                }, null);
                Application.Run();
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Start();
            return completion.Task;
        }
    }
}
