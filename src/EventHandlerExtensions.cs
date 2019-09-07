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
            eventArgs.BeginInvoke(cancellationToken);
            eventHandler?.Invoke(sender, eventArgs);
            return eventArgs.EndInvoke();
        }
    }
}
