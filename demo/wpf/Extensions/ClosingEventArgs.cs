using DeferredEvents;

namespace WpfDeferredEvents
{
    public class ClosingEventArgs : DeferredEventArgs
    {
        public bool Cancel { get; set; }
    }
}
