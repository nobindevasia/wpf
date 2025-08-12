using System.Windows;
using D2G.Iris.ML.ConfigUI.WPF.ViewModels;

namespace D2G.Iris.ML.ConfigUI.WPF.Dialogs
{
    public partial class InputFieldDialog : Window
    {
        public InputFieldDialog()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is InputFieldDialogViewModel viewModel)
            {
                if (string.IsNullOrWhiteSpace(viewModel.FieldName))
                {
                    MessageBox.Show("Field name cannot be empty.", "Validation Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
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