using System;
using System.Threading;

namespace DeferredEvents
{
    public sealed class EventDeferral : IDisposable
    {
        private DeferredEventArgs _eventArgs;

        internal EventDeferral(DeferredEventArgs eventArgs)
        {
            _eventArgs = eventArgs;
        }

        public void Complete()
        {
            var eventArgs = Interlocked.Exchange(ref _eventArgs, null);
            eventArgs?.CompleteDeferral();
        }

        void IDisposable.Dispose()
        {
            Complete();
        }
    }
}
