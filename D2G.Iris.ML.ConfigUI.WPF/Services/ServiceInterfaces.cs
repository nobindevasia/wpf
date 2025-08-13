using System.Collections.Generic;
using System.Threading.Tasks;
using D2G.Iris.ML.Core.Models;

namespace D2G.Iris.ML.ConfigUI.WPF.Services
{
    public interface IConfigurationService
    {
        ModelConfig LoadConfiguration(string filePath);
        void SaveConfiguration(ModelConfig config, string filePath);
        bool ValidateConfiguration(ModelConfig config);
    }

    public interface IDatabaseSchemaLoader
    {
        List<string> LoadTableColumns(DatabaseConfig config);
        bool TestConnection(DatabaseConfig config);
    }

    public interface IDialogService
    {
        bool ShowConfirmationDialog(string message, string title);
        void ShowErrorDialog(string message, string title);
        void ShowInfoDialog(string message, string title);
        string? ShowSaveFileDialog(string filter, string defaultExtension, string defaultFileName);
        string? ShowOpenFileDialog(string filter);
        T? ShowDialog<T>(object viewModel) where T : class;
        bool? ShowInputFieldDialog(object viewModel);
        bool? ShowParameterDialog(object viewModel);
    }
}