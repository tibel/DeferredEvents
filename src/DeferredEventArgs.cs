using System;
using System.Threading;
using System.Threading.Tasks;

namespace DeferredEvents
{
    public class DeferredEventArgs : EventArgs
    {
        private volatile int _deferralsCount;
        private CancellationToken _cancellationToken;
        private TaskCompletionSource<object> _tcs;

        public EventDeferral GetDeferral()
        {
            _cancellationToken.ThrowIfCancellationRequested();

            var count = Interlocked.Increment(ref _deferralsCount);
            if (count < 2)
                ThrowEventNotActiveException();

            return new EventDeferral(this, _cancellationToken);
        }

        internal void CompleteDeferral()
        {
            var count = Interlocked.Decrement(ref _deferralsCount);
            if (count < 1)
                _tcs.TrySetResult(null);
        }

        internal void BeginInvoke(CancellationToken cancellationToken)
        {
            if (_deferralsCount > 0)
                ThrowEventActiveException();

            cancellationToken.ThrowIfCancellationRequested();

            _cancellationToken = cancellationToken;
            _tcs = null;
            _deferralsCount = 1;
        }

        internal Task EndInvoke()
        {
            if (_deferralsCount > 1)
                _tcs = new TaskCompletionSource<object>();

            if (Interlocked.Decrement(ref _deferralsCount) < 1)
                return _cancellationToken.IsCancellationRequested ? Task.FromCanceled(_cancellationToken) : Task.CompletedTask;

            if (_cancellationToken.CanBeCanceled)
            {
                var cancellationRegistration = _cancellationToken.Register(state => ((TaskCompletionSource<object>)state).TrySetCanceled(), _tcs);
                _tcs.Task.ContinueWith((_, state) => ((IDisposable)state).Dispose(), cancellationRegistration, TaskContinuationOptions.ExecuteSynchronously);
            }

            return _tcs.Task;
        }

        private static void ThrowEventNotActiveException()
        {
            throw new InvalidOperationException("Cannot get event deferral when event is not active");
        }

        private static void ThrowEventActiveException()
        {
            throw new InvalidOperationException("Event already active or not completed");
        }
    }
}
