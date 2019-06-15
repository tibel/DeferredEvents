using System;
using System.Threading;
using System.Threading.Tasks;

namespace DeferredEvents
{
    public class DeferredEventArgs : EventArgs
    {
        private readonly TaskCompletionSource<object> _tcs = new TaskCompletionSource<object>();
        private int _deferralsCount;

        public EventDeferral GetDeferral()
        {
            Interlocked.Increment(ref _deferralsCount);
            return new EventDeferral(this);
        }

        internal void CompleteDeferral()
        {
            var count = Interlocked.Decrement(ref _deferralsCount);
            if (count == 0)
                _tcs.TrySetResult(null);
        }

        internal Task WaitForCompletion(CancellationToken cancellationToken)
        {
            if (!_tcs.Task.IsCompleted && _deferralsCount == 0)
                _tcs.TrySetResult(null);

            if (cancellationToken.CanBeCanceled)
            {
                var cancellationRegistration = cancellationToken.Register(state => ((TaskCompletionSource<object>)state).TrySetCanceled(), _tcs);
                _tcs.Task.ContinueWith((_, state) => ((IDisposable)state).Dispose(), cancellationRegistration, TaskContinuationOptions.ExecuteSynchronously);
            }

            return _tcs.Task;
        }
    }
}
