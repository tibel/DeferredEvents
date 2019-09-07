using System.Threading.Tasks;

namespace WpfDeferredEvents
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : WindowEx
    {
        public MainWindow()
        {
            InitializeComponent();

            Closing += MainWindow_Closing;
        }

        private async void MainWindow_Closing(object sender, ClosingEventArgs e)
        {
            if (e.Cancel)
                return;

            using (e.GetDeferral())
            {
                await Task.Delay(2000);
            }
        }
    }
}
