using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Data.SqlClient;
using Microsoft.ML;
using D2G.Iris.ML.ConfigUI.WPF.Commands;
using D2G.Iris.ML.ConfigUI.WPF.Services;
using D2G.Iris.ML.Core.Models;
using D2G.Iris.ML.Data;

namespace D2G.Iris.ML.ConfigUI.WPF.ViewModels
{
    public class ExploratoryDataAnalysisViewModel : BaseViewModel
    {
        private readonly IDialogService _dialogService;
        private int _numberOfRows;
        private int _numberOfColumns;
        private int _totalMissingValues;
        private double _missingValuesPercentage;
        private ObservableCollection<FeatureTypeInfo> _featureTypes;
        private ObservableCollection<ColumnMissingInfo> _columnMissingValues;
        private Func<DatabaseConfig>? _getDatabaseConfig;
        private Func<List<InputField>>? _getInputFields;
        private bool _isLoading;
        private string _loadingMessage = "Loading data...";
        private VisualisationViewModel _visualisationViewModel;

        public ExploratoryDataAnalysisViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService;
            _featureTypes = new ObservableCollection<FeatureTypeInfo>();
            _columnMissingValues = new ObservableCollection<ColumnMissingInfo>();
            _visualisationViewModel = new VisualisationViewModel(dialogService);
            
            AnalyzeDataCommand = new RelayCommand(_ => AnalyzeData(), _ => CanAnalyzeData());
        }

        #region Properties

        public int NumberOfRows
        {
            get => _numberOfRows;
            set => SetProperty(ref _numberOfRows, value);
        }

        public int NumberOfColumns
        {
            get => _numberOfColumns;
            set => SetProperty(ref _numberOfColumns, value);
        }

        public int TotalMissingValues
        {
            get => _totalMissingValues;
            set => SetProperty(ref _totalMissingValues, value);
        }

        public double MissingValuesPercentage
        {
            get => _missingValuesPercentage;
            set => SetProperty(ref _missingValuesPercentage, value);
        }

        public ObservableCollection<FeatureTypeInfo> FeatureTypes
        {
            get => _featureTypes;
            set => SetProperty(ref _featureTypes, value);
        }

        public ObservableCollection<ColumnMissingInfo> ColumnMissingValues
        {
            get => _columnMissingValues;
            set => SetProperty(ref _columnMissingValues, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string LoadingMessage
        {
            get => _loadingMessage;
            set => SetProperty(ref _loadingMessage, value);
        }

        public VisualisationViewModel VisualisationViewModel
        {
            get => _visualisationViewModel;
            set => SetProperty(ref _visualisationViewModel, value);
        }

        #endregion

        #region Commands

        public ICommand AnalyzeDataCommand { get; }

        #endregion

        #region Public Methods

        public void SetDependencies(Func<DatabaseConfig> getDatabaseConfig, Func<List<InputField>> getInputFields)
        {
            _getDatabaseConfig = getDatabaseConfig;
            _getInputFields = getInputFields;
        }

        #endregion

        #region Private Methods

        private bool CanAnalyzeData()
        {
            return _getDatabaseConfig != null && _getInputFields != null && !_isLoading;
        }

        private async void AnalyzeData()
        {
            try
            {
                IsLoading = true;
                LoadingMessage = "Validating configuration...";

                var databaseConfig = _getDatabaseConfig?.Invoke();
                var inputFields = _getInputFields?.Invoke();
                
                if (databaseConfig == null)
                {
                    _dialogService.ShowErrorDialog("Database configuration is not available.", "Error");
                    return;
                }

                if (inputFields == null || !inputFields.Any())
                {
                    _dialogService.ShowErrorDialog("No input fields are configured.", "Error");
                    return;
                }

                var enabledFields = inputFields.Where(f => f.IsEnabled).ToList();
                if (!enabledFields.Any())
                {
                    _dialogService.ShowErrorDialog("No input fields are enabled for analysis.", "Error");
                    return;
                }

                LoadingMessage = "Loading data from database...";
                await Task.Delay(200);

                var sqlHandler = new SqlHandler(databaseConfig.TableName);
                sqlHandler.Connect(databaseConfig);
                var connectionString = sqlHandler.GetConnectionString();

                var dataTable = await Task.Run(() => 
                {
                    var dataLoader = new DatabaseDataLoader();
                    var enabledFieldNames = enabledFields.Select(f => f.Name).ToArray();
                    
                    // Use same data loading approach as training but convert to DataTable for EDA
                    return LoadDataTableFromSql(
                        connectionString,
                        databaseConfig.TableName,
                        enabledFieldNames,
                        databaseConfig.WhereClause);
                });
                
                LoadingMessage = "Analyzing data patterns...";
                await Task.Delay(200);

                AnalyzeDataTable(dataTable);

                LoadingMessage = "Generating chart previews...";
                await Task.Delay(200);

                await _visualisationViewModel.GenerateHistogramPreviewsAsync(dataTable);

                _dialogService.ShowInfoDialog($"Data analysis completed successfully for {enabledFields.Count} enabled fields.", "Analysis Complete");
            }
            catch (Exception ex)
            {
                _dialogService.ShowErrorDialog($"Error analyzing data: {ex.Message}", "Analysis Error");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private DataTable LoadDataTableFromSql(string connectionString, string tableName, string[] fieldNames, string whereClause)
        {
            string fullTableName = tableName.Contains('[')
                ? tableName 
                : (tableName.Contains('.')
                    ? string.Join('.', tableName.Split('.').Select(part => $"[{part}]"))
                    : $"[{tableName}]");

            var fieldNamesWithBrackets = fieldNames.Select(f => $"[{f}]");
            var fieldsClause = string.Join(", ", fieldNamesWithBrackets);

            var query = string.IsNullOrWhiteSpace(whereClause) 
                ? $"SELECT {fieldsClause} FROM {fullTableName}"
                : $"SELECT {fieldsClause} FROM {fullTableName} WHERE {whereClause}";

            using var connection = new SqlConnection(connectionString);
            using var command = new SqlCommand(query, connection);
            using var adapter = new SqlDataAdapter(command);
            
            var dataTable = new DataTable();
            connection.Open();
            adapter.Fill(dataTable);
            
            return dataTable;
        }

        private void AnalyzeDataTable(DataTable dataTable)
        {
            NumberOfRows = dataTable.Rows.Count;
            NumberOfColumns = dataTable.Columns.Count;

            AnalyzeFeatureTypes(dataTable);

            AnalyzeMissingValues(dataTable);
            AnalyzeMissingValues(dataTable);
        }

        private void AnalyzeFeatureTypes(DataTable dataTable)
        {
            var typeGroups = dataTable.Columns.Cast<DataColumn>()
                .GroupBy(col => GetFeatureType(col.DataType))
                .Select(g => new FeatureTypeInfo 
                { 
                    Type = g.Key, 
                    Count = g.Count() 
                })
                .OrderBy(x => x.Type);

            FeatureTypes.Clear();
            foreach (var typeInfo in typeGroups)
            {
                FeatureTypes.Add(typeInfo);
            }
        }

        private void AnalyzeMissingValues(DataTable dataTable)
        {
            var columnMissingInfo = new List<ColumnMissingInfo>();
            int totalCells = NumberOfRows * NumberOfColumns;
            int totalMissing = 0;

            foreach (DataColumn column in dataTable.Columns)
            {
                int missingCount = 0;
                
                foreach (DataRow row in dataTable.Rows)
                {
                    var value = row[column];
                    if (value == null || value == DBNull.Value || 
                        (value is string str && string.IsNullOrWhiteSpace(str)))
                    {
                        missingCount++;
                    }
                }

                totalMissing += missingCount;
                
                double missingPercentage = NumberOfRows > 0 ? (double)missingCount / NumberOfRows * 100 : 0;
                
                columnMissingInfo.Add(new ColumnMissingInfo
                {
                    ColumnName = column.ColumnName,
                    MissingCount = missingCount,
                    MissingPercentage = missingPercentage
                });
            }

            TotalMissingValues = totalMissing;
            MissingValuesPercentage = totalCells > 0 ? (double)totalMissing / totalCells * 100 : 0;

            ColumnMissingValues.Clear();
            foreach (var info in columnMissingInfo.OrderByDescending(x => x.MissingCount))
            {
                ColumnMissingValues.Add(info);
            }
        }

        private string GetFeatureType(Type dataType)
        {
            if (dataType == typeof(int) || dataType == typeof(long) || 
                dataType == typeof(short) || dataType == typeof(byte) ||
                dataType == typeof(float) || dataType == typeof(double) || 
                dataType == typeof(decimal))
            {
                return "Numeric";
            }
            else if (dataType == typeof(bool))
            {
                return "Boolean";
            }
            else if (dataType == typeof(DateTime))
            {
                return "Date";
            }
            else if (dataType == typeof(string))
            {
                return "Text/Categorical";
            }
            else
            {
                return "Other";
            }
        }

        #endregion
    }

    public class FeatureTypeInfo
    {
        public string Type { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class ColumnMissingInfo
    {
        public string ColumnName { get; set; } = string.Empty;
        public int MissingCount { get; set; }
        public double MissingPercentage { get; set; }
    }
}