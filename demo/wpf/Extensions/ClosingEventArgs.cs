using DeferredEvents;

namespace DeferredEvents.WPF
{
    public class ClosingEventArgs : DeferredEventArgs
    {
        public bool Cancel { get; set; }
    }
}
