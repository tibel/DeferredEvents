using System;
using System.Threading;

namespace DeferredEvents
{
    public sealed class EventDeferral : IDisposable
    {
        private DeferredEventArgs _eventArgs;

        public CancellationToken CancellationToken { get; }

        internal EventDeferral(DeferredEventArgs eventArgs, CancellationToken cancellationToken)
        {
            _eventArgs = eventArgs;
            CancellationToken = cancellationToken;
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
