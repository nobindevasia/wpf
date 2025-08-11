using System.Windows.Input;
using D2G.Iris.ML.ConfigUI.WPF.Commands;
using D2G.Iris.ML.ConfigUI.WPF.Services;
using D2G.Iris.ML.Core.Models;

namespace D2G.Iris.ML.ConfigUI.WPF.ViewModels
{
    public class DatabaseSettingsViewModel : BaseViewModel
    {
        private readonly IDialogService _dialogService;
        private string _server = "localhost";
        private string _database = "IrisData";
        private string _tableName = "DataTable";
        private string _outputTableName = "";
        private string _whereClause = "";

        public DatabaseSettingsViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService;
            InitializeCommands();
        }

        #region Properties

        public string Server
        {
            get => _server;
            set => SetProperty(ref _server, value);
        }

        public string Database
        {
            get => _database;
            set => SetProperty(ref _database, value);
        }

        public string TableName
        {
            get => _tableName;
            set => SetProperty(ref _tableName, value);
        }

        public string OutputTableName
        {
            get => _outputTableName;
            set => SetProperty(ref _outputTableName, value);
        }

        public string WhereClause
        {
            get => _whereClause;
            set => SetProperty(ref _whereClause, value);
        }

        #endregion

        #region Commands

        public ICommand TestConnectionCommand { get; private set; } = null!;

        #endregion

        private void InitializeCommands()
        {
            TestConnectionCommand = new RelayCommand(TestConnection);
        }

        private void TestConnection()
        {
            try
            {
                var config = GetConfiguration();
                var schemaLoader = new DatabaseSchemaLoader();

                if (schemaLoader.TestConnection(config))
                {
                    _dialogService.ShowInfoDialog("Connection successful!", "Database Connection");
                }
                else
                {
                    _dialogService.ShowErrorDialog("Connection failed. Please check your settings.", "Database Connection");
                }
            }
            catch (System.Exception ex)
            {
                _dialogService.ShowErrorDialog($"Connection error: {ex.Message}", "Database Connection");
            }
        }

        public void SetConfiguration(DatabaseConfig? config)
        {
            if (config == null) return;

            Server = config.Server ?? "localhost";
            Database = config.Database ?? "IrisData";
            TableName = config.TableName ?? "DataTable";
            OutputTableName = config.OutputTableName ?? "";
            WhereClause = config.WhereClause ?? "";
        }

        public DatabaseConfig GetConfiguration()
        {
            return new DatabaseConfig
            {
                Server = Server,
                Database = Database,
                TableName = TableName,
                OutputTableName = OutputTableName,
                WhereClause = WhereClause
            };
        }
    }
}