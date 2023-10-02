using Regira.System.Hosting.Background.Abstractions;
using System.Collections.Concurrent;

namespace Regira.System.Hosting.Background.Core;

public class BackgroundTaskQueue : IBackgroundTaskQueue
{
    public class Options
    {
        public int InitialConcurrentCount { get; set; }
        public IEnumerable<Func<CancellationToken, Task>>? InitialQueue { get; set; }
    }
    // https://www.c-sharpcorner.com/article/how-to-call-background-service-from-net-core-web-api/
    // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services
    private readonly ConcurrentQueue<Func<CancellationToken, Task>> _workItems;
    private readonly SemaphoreSlim _signal;
    public BackgroundTaskQueue(Options? options = null)
    {
        _workItems = new ConcurrentQueue<Func<CancellationToken, Task>>(options?.InitialQueue ?? new Queue<Func<CancellationToken, Task>>());
        _signal = new SemaphoreSlim(options?.InitialConcurrentCount ?? 0);
    }


    public void QueueBackgroundWorkItem(Func<CancellationToken, Task> workItem)
    {
        if (workItem == null)
        {
            throw new ArgumentNullException(nameof(workItem));
        }

        _workItems.Enqueue(workItem);
        _signal.Release();
    }
    public async Task<Func<CancellationToken, Task>?> DequeueAsync(CancellationToken cancellationToken)
    {
        await _signal.WaitAsync(cancellationToken);
        _workItems.TryDequeue(out var workItem);

        return workItem;
    }
}