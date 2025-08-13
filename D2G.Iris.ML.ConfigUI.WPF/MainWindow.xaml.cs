using System.Windows;
using D2G.Iris.ML.ConfigUI.WPF.ViewModels;

namespace D2G.Iris.ML.ConfigUI.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(MainWindowViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        protected override void OnClosed(System.EventArgs e)
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                viewModel.Dispose();
            }
            base.OnClosed(e);
        }
    }
}