using NUnit.Framework;
using System;
using System.Threading;

namespace DeferredEvents.Tests
{
    [TestFixture]
    [TestOf(typeof(EventHandlerExtensions))]
    public class EventHandlerExtensionsTests
    {
        [Test]
        public void NoEventArgs()
        {
            EventHandler<DeferredEventArgs> handler = (s, e) => { };

            Assert.Throws<ArgumentNullException>(() => handler.InvokeAsync(null, null));
        }

        [Test]
        public void NoHandler()
        {
            EventHandler<DeferredEventArgs> handler = null;

            var task = handler.InvokeAsync(null, new DeferredEventArgs());

            Assert.NotNull(task);
            Assert.IsTrue(task.IsCompleted);
        }

        [Test]
        public void NoHandler_Cancelled()
        {
            EventHandler<DeferredEventArgs> handler = null;

            Assert.Throws<OperationCanceledException>(() => handler.InvokeAsync(null, new DeferredEventArgs(), new CancellationToken(true)));
        }

        [Test]
        public void OneHandler()
        {
            EventHandler<DeferredEventArgs> handler = (s, e) => { };

            var task = handler.InvokeAsync(null, new DeferredEventArgs());

            Assert.NotNull(task);
            Assert.IsTrue(task.IsCompleted);
        }

        [Test]
        public void OneHandler_Pending()
        {
            EventHandler<DeferredEventArgs> handler = (s, e) => e.GetDeferral();

            var task = handler.InvokeAsync(null, new DeferredEventArgs());

            Assert.NotNull(task);
            Assert.IsFalse(task.IsCompleted);
        }

        [Test]
        public void OneHandler_Cancelled()
        {
            EventHandler<DeferredEventArgs> handler = (s, e) => { };

            Assert.Throws<OperationCanceledException>(() => handler.InvokeAsync(null, new DeferredEventArgs(), new CancellationToken(true)));
        }

        [Test]
        public void OneHandler_Cancelling()
        {
            var cts = new CancellationTokenSource();

            EventHandler<DeferredEventArgs> handler = (s, e) => cts.Cancel();
            var eventArgs = new DeferredEventArgs();

            var task = handler.InvokeAsync(null, eventArgs, cts.Token);

            Assert.NotNull(task);
            Assert.IsTrue(task.IsCanceled);

            cts.Dispose();
        }

        [Test]
        public void ReUseEventArgs()
        {
            EventHandler<DeferredEventArgs> handler = (s, e) => { };
            var eventArgs = new DeferredEventArgs();

            var task = handler.InvokeAsync(null, eventArgs);

            Assert.NotNull(task);
            Assert.IsTrue(task.IsCompleted);

            task = handler.InvokeAsync(null, eventArgs);

            Assert.NotNull(task);
            Assert.IsTrue(task.IsCompleted);
        }

        [Test]
        public void ReUseEventArgs_Pending()
        {
            EventHandler<DeferredEventArgs> handler = (s, e) => e.GetDeferral();
            var eventArgs = new DeferredEventArgs();

            var task = handler.InvokeAsync(null, eventArgs);

            Assert.NotNull(task);
            Assert.IsFalse(task.IsCompleted);

            Assert.Throws<InvalidOperationException>(() => handler.InvokeAsync(null, eventArgs));
        }
    }
}
