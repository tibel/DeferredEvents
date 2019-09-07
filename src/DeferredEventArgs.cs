using System;
using System.Threading;
using System.Threading.Tasks;

namespace DeferredEvents
{
    public class DeferredEventArgs : EventArgs
    {
        private const int NEW        = 0;
        private const int ACTIVE     = 1;
        private const int COMPLETING = 2;
        private const int COMPLETED  = 3;

        private volatile int _deferralsCount;
        private volatile int _state;

        private TaskCompletionSource<object> _tcs;
        private CancellationToken _cancellationToken;

        public EventDeferral GetDeferral()
        {
            if (_state != ACTIVE)
                throw new InvalidOperationException("Cannot get event deferral after event started completing");

            _cancellationToken.ThrowIfCancellationRequested();
            Interlocked.Increment(ref _deferralsCount);
            return new EventDeferral(this, _cancellationToken);
        }

        internal void CompleteDeferral()
        {
            var count = Interlocked.Decrement(ref _deferralsCount);
            if (count == 0 && Interlocked.CompareExchange(ref _state, COMPLETED, COMPLETING) == COMPLETING)
                _tcs.TrySetResult(null);
        }

        internal void BeginInvoke(CancellationToken cancellationToken)
        {
            if (_state != NEW)
                throw new InvalidOperationException("Cannot re-use deferred event args");

            cancellationToken.ThrowIfCancellationRequested();
            _cancellationToken = cancellationToken;
            _state = ACTIVE;
        }

        internal Task EndInvoke()
        {
            if (_deferralsCount != 0)
                _tcs = new TaskCompletionSource<object>();

            _state = COMPLETING;

            if (_deferralsCount == 0 && Interlocked.CompareExchange(ref _state, COMPLETED, COMPLETING) == COMPLETING)
                return _cancellationToken.IsCancellationRequested ? Task.FromCanceled(_cancellationToken) : Task.CompletedTask;

            if (_cancellationToken.CanBeCanceled)
            {
                var cancellationRegistration = _cancellationToken.Register(state => ((TaskCompletionSource<object>)state).TrySetCanceled(), _tcs);
                _tcs.Task.ContinueWith((_, state) => ((IDisposable)state).Dispose(), cancellationRegistration, TaskContinuationOptions.ExecuteSynchronously);
            }

            return _tcs.Task;
        }
    }
}
