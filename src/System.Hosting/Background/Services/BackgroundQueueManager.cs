using Microsoft.Extensions.DependencyInjection;
using Regira.System.Hosting.Background.Abstractions;
using Regira.System.Hosting.Background.Models;

namespace Regira.System.Hosting.Background.Services;

public class BackgroundQueueManager : BackgroundQueueManager<BackgroundTask>, IBackgroundQueueManager
{
    public BackgroundQueueManager(IBackgroundTaskQueue queue, IBackgroundTaskManager backgroundTaskManager, IServiceProvider serviceProvider)
        : base(queue, backgroundTaskManager, serviceProvider)
    {
    }
}
public class BackgroundQueueManager<TTask> : IBackgroundQueueManager<TTask>
    where TTask : class, IBackgroundTask, new()
{
    public event Action<Exception, TTask, IServiceProvider>? OnError;


    private readonly IBackgroundTaskQueue _queue;
    private readonly IBackgroundTaskManager<TTask> _backgroundTaskManager;
    private readonly IServiceProvider _serviceProvider;
    public BackgroundQueueManager(IBackgroundTaskQueue queue, IBackgroundTaskManager<TTask> backgroundTaskManager, IServiceProvider serviceProvider)
    {
        _queue = queue;
        _backgroundTaskManager = backgroundTaskManager;
        _serviceProvider = serviceProvider;
    }


    public TTask Execute(Action<IServiceProvider, TTask> action)
    {
        return Execute<object?>((sp, bgTask) =>
        {
            action(sp, bgTask);
            return null!;
        });
    }
    public TTask Execute<TResult>(Func<IServiceProvider, TTask, Task<TResult>> func)
    {
        var bgTask = new TTask
        {
            Id = Guid.NewGuid().ToString()
        };
        _queue.QueueBackgroundWorkItem(async token =>
        {
            bgTask.Start(token);

            using var scope = _serviceProvider.CreateScope();
            try
            {
                var result = await func(scope.ServiceProvider, bgTask);
                bgTask.Finish(result);
            }
            catch (Exception ex)
            {
                bgTask.SetError(ex);
                OnError?.Invoke(ex, bgTask, scope.ServiceProvider);
            }
        });
        _backgroundTaskManager.Add(bgTask);

        return bgTask;
    }
}