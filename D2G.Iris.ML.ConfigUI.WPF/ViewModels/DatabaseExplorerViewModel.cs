using System.Collections.ObjectModel;
using D2G.Iris.ML.ConfigUI.WPF.Services;
using D2G.Iris.ML.Core.Models;

namespace D2G.Iris.ML.ConfigUI.WPF.ViewModels
{
    public class DatabaseExplorerViewModel : BaseViewModel
    {
        private readonly IDialogService _dialogService;
        private readonly DatabaseSchemaLoader _schemaLoader;
        private readonly string _server;

        public ObservableCollection<string> Databases { get; } = new();
        private string? _selectedDatabase;
        public string? SelectedDatabase
        {
            get => _selectedDatabase;
            set
            {
                if (SetProperty(ref _selectedDatabase, value))
                {
                    LoadTables();
                }
            }
        }

        public ObservableCollection<TableInfo> Tables { get; } = new();
        private TableInfo? _selectedTable;
        public TableInfo? SelectedTable
        {
            get => _selectedTable;
            set => SetProperty(ref _selectedTable, value);
        }

        public DatabaseExplorerViewModel(IDialogService dialogService, string server, string? database)
        {
            _dialogService = dialogService;
            _schemaLoader = new DatabaseSchemaLoader();
            _server = server;
            LoadDatabases();

            if (!string.IsNullOrWhiteSpace(database) && Databases.Contains(database))
            {
                SelectedDatabase = database;
            }
        }

        private void LoadDatabases()
        {
            try
            {
                Databases.Clear();
                var config = new DatabaseConfig { Server = _server };
                var dbs = _schemaLoader.LoadDatabases(config);
                foreach (var db in dbs)
                    Databases.Add(db);
            }
            catch (System.Exception ex)
            {
                _dialogService.ShowErrorDialog($"Error loading databases: {ex.Message}", "Database Explorer");
            }
        }

        private void LoadTables()
        {
            try
            {
                Tables.Clear();
                if (string.IsNullOrWhiteSpace(SelectedDatabase))
                    return;
                var config = new DatabaseConfig { Server = _server, Database = SelectedDatabase };
                var tables = _schemaLoader.LoadTables(config);
                foreach (var table in tables)
                    Tables.Add(table);
            }
            catch (System.Exception ex)
            {
                _dialogService.ShowErrorDialog($"Error loading tables: {ex.Message}", "Database Explorer");
            }
        }
    }
}
