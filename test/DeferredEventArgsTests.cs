using NUnit.Framework;
using System;
using System.Threading;

namespace DeferredEvents.Tests
{
    [TestFixture(TestOf = typeof(DeferredEventArgs))]
    public class DeferredEventArgsTests
    {
        [Test]
        public void BeginInvoke()
        {
            var args = new DeferredEventArgs();
            args.BeginInvoke(CancellationToken.None);
        }

        [Test]
        public void BeginInvoke_Cancelled()
        {
            var args = new DeferredEventArgs();

            Assert.Throws<OperationCanceledException>(() => args.BeginInvoke(new CancellationToken(true)));
        }

        [Test]
        public void BeginInvoke_SecondCall()
        {
            var args = new DeferredEventArgs();
            args.BeginInvoke(CancellationToken.None);

            Assert.Throws<InvalidOperationException>(() => args.BeginInvoke(CancellationToken.None));
        }

        [Test]
        public void GetDeferral_BeforeBeginInvoke()
        {
            var args = new DeferredEventArgs();
            
            Assert.Throws<InvalidOperationException>(() => args.GetDeferral());
        }

        [Test]
        public void GetDeferral_AfterEndInvoke()
        {
            var args = new DeferredEventArgs();
            args.EndInvoke();

            Assert.Throws<InvalidOperationException>(() => args.GetDeferral());
        }

        [Test]
        public void NoDeferral()
        {
            var args = new DeferredEventArgs();

            args.BeginInvoke(CancellationToken.None);
            var task = args.EndInvoke();

            Assert.NotNull(task);
            Assert.IsTrue(task.IsCompleted);
        }

        [Test]
        public void OneDeferral_Pending()
        {
            var args = new DeferredEventArgs();

            args.BeginInvoke(CancellationToken.None);

            args.GetDeferral();

            var task = args.EndInvoke();

            Assert.NotNull(task);
            Assert.IsFalse(task.IsCompleted);
        }

        [Test]
        public void OneDeferral_Pending_Cancelled()
        {
            var cts = new CancellationTokenSource();

            var args = new DeferredEventArgs();

            args.BeginInvoke(cts.Token);

            args.GetDeferral();

            var task = args.EndInvoke();

            Assert.NotNull(task);
            Assert.IsFalse(task.IsCompleted);

            cts.Cancel();

            Assert.IsTrue(task.IsCanceled);
        }

        [Test]
        public void OneDeferral_Completed()
        {
            var args = new DeferredEventArgs();

            args.BeginInvoke(CancellationToken.None);

            args.GetDeferral()
                .Complete();

            var task = args.EndInvoke();

            Assert.NotNull(task);
            Assert.IsTrue(task.IsCompleted);
        }

        [Test]
        public void OneDeferral_Completed_AfterEndInvoke()
        {
            var args = new DeferredEventArgs();

            args.BeginInvoke(CancellationToken.None);

            var deferral = args.GetDeferral();

            var task = args.EndInvoke();

            Assert.NotNull(task);
            Assert.IsFalse(task.IsCompleted);

            deferral.Complete();

            Assert.IsTrue(task.IsCompleted);
        }

        [Test]
        public void TwoDeferral_OneComplete_SecondPending()
        {
            var args = new DeferredEventArgs();

            args.BeginInvoke(CancellationToken.None);

            args.GetDeferral()
                .Complete();

            var deferral2 = args.GetDeferral();

            var task = args.EndInvoke();

            Assert.NotNull(task);
            Assert.IsFalse(task.IsCompleted);

            deferral2.Complete();

            Assert.IsTrue(task.IsCompleted);
        }
    }
}