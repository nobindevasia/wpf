using System.Windows;

namespace D2G.Iris.ML.ConfigUI.WPF.Dialogs
{
    public partial class DatabaseExplorerDialog : Window
    {
        public DatabaseExplorerDialog()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModels.DatabaseExplorerViewModel vm &&
                (string.IsNullOrWhiteSpace(vm.SelectedDatabase) || vm.SelectedTable == null))
            {
                MessageBox.Show("Please select a database and a table.", "Selection Required",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
