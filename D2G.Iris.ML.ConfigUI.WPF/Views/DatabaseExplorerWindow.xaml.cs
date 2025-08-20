using System.Windows;
using System.Windows.Input;
using D2G.Iris.ML.ConfigUI.WPF.ViewModels;

namespace D2G.Iris.ML.ConfigUI.WPF.Views
{
    public partial class DatabaseExplorerWindow : Window
    {
        public DatabaseExplorerWindow()
        {
            InitializeComponent();
        }

        public DatabaseExplorerWindow(DatabaseSettingsViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is DatabaseSettingsViewModel viewModel && viewModel.SelectedTable != null)
            {
                viewModel.SelectTableCommand?.Execute(null);
            }
        }
    }
}