using System.Windows;
using D2G.Iris.ML.ConfigUI.WPF.ViewModels;
using D2G.Iris.ML.ConfigUI.WPF.Dialogs;

namespace D2G.Iris.ML.ConfigUI.WPF.Dialogs
{
    public partial class ParameterDialog : Window
    {
        public ParameterDialog()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ParameterDialogViewModel viewModel)
            {
                if (string.IsNullOrWhiteSpace(viewModel.ParameterName))
                {
                    MessageBox.Show("Parameter name cannot be empty.", "Validation Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(viewModel.ParameterValueString))
                {
                    MessageBox.Show("Parameter value cannot be empty.", "Validation Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (viewModel.HasError)
                {
                    MessageBox.Show($"Please correct the error: {viewModel.ErrorMessage}", "Validation Error",
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