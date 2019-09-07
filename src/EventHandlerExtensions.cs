using System;
using System.Threading;
using System.Threading.Tasks;

namespace DeferredEvents
{
    public static class EventHandlerExtensions
    {
        public static Task InvokeAsync<T>(this EventHandler<T> eventHandler, object sender, T eventArgs)
            where T : DeferredEventArgs
        {
            return InvokeAsync(eventHandler, sender, eventArgs, CancellationToken.None);
        }

        public static Task InvokeAsync<T>(this EventHandler<T> eventHandler, object sender, T eventArgs, CancellationToken cancellationToken)
            where T : DeferredEventArgs
        {
            if (cancellationToken.IsCancellationRequested)
                return Task.FromCanceled(new CancellationToken(true));

            if (!(eventHandler is object))
                return Task.CompletedTask;

            eventHandler(sender, eventArgs);

            return eventArgs.WaitForCompletion(cancellationToken);
        }
    }
}
