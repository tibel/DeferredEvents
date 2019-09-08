using DeferredEvents;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;

namespace DeferredEvents.WPF
{
    public class WindowEx : Window
    {
        private bool _actuallyClosing;
        private bool _deferredClosing;

        public new event EventHandler<ClosingEventArgs> Closing;

        protected override void OnClosing(CancelEventArgs e)
        {
            if (_actuallyClosing)
            {
                _actuallyClosing = false;
                return;
            }

            if (_deferredClosing)
                return;

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
                _deferredClosing = true;
                DeferredClose(deferredEvent, closingEventArgs);
            }
        }

        private async void DeferredClose(Task deferredEvent, ClosingEventArgs closingEventArgs)
        {
            await deferredEvent.ConfigureAwait(true);

            _deferredClosing = false;

            if (closingEventArgs.Cancel)
                return;

            _actuallyClosing = true;
            Close();
            _actuallyClosing = false;
        }
    }
}
