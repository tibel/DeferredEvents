using DeferredEvents;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;

namespace WpfDeferredEvents
{
    public class WindowEx : Window
    {
        private bool _actuallyClosing;

        public new event EventHandler<ClosingEventArgs> Closing;

        protected override void OnClosing(CancelEventArgs e)
        {
            if (_actuallyClosing)
            {
                _actuallyClosing = false;
                return;
            }

            base.OnClosing(e);

            var closingEventArgs = new ClosingEventArgs() { Cancel = e.Cancel };
            var deferredEvent = Closing.InvokeAsync(this, closingEventArgs);

            if (deferredEvent.IsCompleted)
            {
                e.Cancel = closingEventArgs.Cancel;
            }
            else
            {
                e.Cancel = true;
                DeferredClose(deferredEvent, closingEventArgs);
            }
        }

        private async void DeferredClose(Task deferredEvent, ClosingEventArgs closingEventArgs)
        {
            await deferredEvent.ConfigureAwait(true);
            if (closingEventArgs.Cancel)
                return;

            _actuallyClosing = true;
            Close();
            _actuallyClosing = false;
        }
    }
}
