using System;
using System.Threading;
using System.Threading.Tasks;

namespace DeferredEvents
{
    public class DeferredEventArgs : EventArgs
    {
        private volatile int _deferralsCount;
        private TaskCompletionSource<object> _tcs;

        public EventDeferral GetDeferral()
        {
            var count = Interlocked.Increment(ref _deferralsCount);

            if (count == 0 && !(_tcs is object))
                _tcs = new TaskCompletionSource<object>();

            return new EventDeferral(this);
        }

        internal void CompleteDeferral()
        {
            var count = Interlocked.Decrement(ref _deferralsCount);

            if (count == 0)
                _tcs.SetResult(null);
        }

        internal Task WaitForCompletion(CancellationToken cancellationToken)
        {
            if (_deferralsCount == 0)
                return Task.CompletedTask;

            if (cancellationToken.CanBeCanceled)
            {
                var cancellationRegistration = cancellationToken.Register(state => ((TaskCompletionSource<object>)state).TrySetCanceled(), _tcs);
                _tcs.Task.ContinueWith((_, state) => ((IDisposable)state).Dispose(), cancellationRegistration, TaskContinuationOptions.ExecuteSynchronously);
            }

            return _tcs.Task;
        }
    }
}
