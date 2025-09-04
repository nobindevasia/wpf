using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Data;
using System.Linq;
using System.Windows.Media;
using System.Diagnostics;
using Microsoft.Data.SqlClient;
using SciChart.Charting.Model.DataSeries;
using SciChart.Charting.Model.ChartSeries;
using SciChart.Charting.Visuals.PointMarkers;
using SciChart.Data.Model;
using D2G.Iris.ML.ConfigUI.WPF.Commands;
using D2G.Iris.ML.ConfigUI.WPF.Services;
using D2G.Iris.ML.Core.Models;
using D2G.Iris.ML.Data;

namespace D2G.Iris.ML.ConfigUI.WPF.ViewModels
{
    public class VisualisationViewModel : INotifyPropertyChanged
    {
        private readonly IDialogService _dialogService;
        private ObservableCollection<IRenderableSeriesViewModel> _chartSeries;
        private string _xAxisTitle = "Features";
        private string _yAxisTitle = "Features";
        private bool _hasChartData;
        private bool _isLoading;
        private string _loadingMessage = "Generating correlation heatmap...";
        private Func<DatabaseConfig>? _getDatabaseConfig;
        private Func<List<InputField>>? _getInputFields;

        public ObservableCollection<IRenderableSeriesViewModel> ChartSeries
        {
            get => _chartSeries;
            set
            {
                if (_chartSeries != value)
                {
                    _chartSeries = value;
                    OnPropertyChanged(nameof(ChartSeries));
                    HasChartData = value?.Count > 0;
                    Debug.WriteLine($"ChartSeries property set, count: {value?.Count ?? 0}");
                }
            }
        }

        public string XAxisTitle
        {
            get => _xAxisTitle;
            set
            {
                _xAxisTitle = value;
                OnPropertyChanged(nameof(XAxisTitle));
            }
        }

        public string YAxisTitle
        {
            get => _yAxisTitle;
            set
            {
                _yAxisTitle = value;
                OnPropertyChanged(nameof(YAxisTitle));
            }
        }

        public bool HasChartData
        {
            get => _hasChartData;
            set
            {
                _hasChartData = value;
                OnPropertyChanged(nameof(HasChartData));
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }

        public string LoadingMessage
        {
            get => _loadingMessage;
            set
            {
                _loadingMessage = value;
                OnPropertyChanged(nameof(LoadingMessage));
            }
        }

        public ICommand GenerateChartCommand { get; }

        public VisualisationViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService;
            
            // Initialize ChartSeries properly
            _chartSeries = new ObservableCollection<IRenderableSeriesViewModel>();
            Debug.WriteLine("ChartSeries initialized");
            
            GenerateChartCommand = new RelayCommand(() => GenerateCorrelationHeatmap(), () => CanGenerateChart());
        }
        

        public void SetDependencies(Func<DatabaseConfig> getDatabaseConfig, Func<List<InputField>> getInputFields)
        {
            _getDatabaseConfig = getDatabaseConfig;
            _getInputFields = getInputFields;
        }

        private bool CanGenerateChart()
        {
            return !_isLoading; // Simplified for debugging
        }

        private async void GenerateCorrelationHeatmap()
        {
            try
            {
                IsLoading = true;
                LoadingMessage = "Initializing correlation analysis...";
                HasChartData = false;

                var databaseConfig = _getDatabaseConfig?.Invoke();
                var inputFields = _getInputFields?.Invoke();
                
                Debug.WriteLine($"Database config: {(databaseConfig != null ? "Available" : "NULL")}");
                Debug.WriteLine($"Input fields: {(inputFields != null ? inputFields.Count.ToString() : "NULL")}");
                
                if (databaseConfig == null)
                {
                    Debug.WriteLine("ERROR: Database configuration is required.");
                    _dialogService.ShowErrorDialog("Database configuration is required.", "Error");
                    return;
                }

                if (inputFields == null || !inputFields.Any())
                {
                    Debug.WriteLine("ERROR: Input fields are required.");
                    _dialogService.ShowErrorDialog("Input fields are required.", "Error");
                    return;
                }

                var enabledFields = inputFields.Where(f => f.IsEnabled).ToList();
                if (enabledFields.Count < 2)
                {
                    Debug.WriteLine("ERROR: At least 2 numeric fields are required for correlation analysis.");
                    _dialogService.ShowErrorDialog("At least 2 numeric fields are required for correlation analysis.", "Error");
                    return;
                }

                LoadingMessage = "Connecting to database...";
                await Task.Delay(300);

                Debug.WriteLine($"Attempting to connect to database: {databaseConfig.Server}");
                
                // Run database operations on background thread
                var (correlationMatrix, numericColumns) = await Task.Run(() =>
                {
                    try
                    {
                        var sqlHandler = new SqlHandler(databaseConfig.TableName);
                        sqlHandler.Connect(databaseConfig);
                        var connectionString = sqlHandler.GetConnectionString();
                        
                        Debug.WriteLine("Database connection established");

                        string fullTableName = databaseConfig.TableName.Contains('[')
                            ? databaseConfig.TableName 
                            : (databaseConfig.TableName.Contains('.')
                                ? string.Join('.', databaseConfig.TableName.Split('.').Select(part => $"[{part}]"))
                                : $"[{databaseConfig.TableName}]");

                        var fieldNames = enabledFields.Select(f => $"[{f.Name}]");
                        var fieldsClause = string.Join(", ", fieldNames);

                        var query = string.IsNullOrWhiteSpace(databaseConfig.WhereClause) 
                            ? $"SELECT {fieldsClause} FROM {fullTableName}"
                            : $"SELECT {fieldsClause} FROM {fullTableName} WHERE {databaseConfig.WhereClause}";

                        Debug.WriteLine($"Executing query: {query}");
                        
                        var dataTable = ExecuteQuery(connectionString, query);
                        Debug.WriteLine($"Retrieved {dataTable.Rows.Count} rows");
                        
                        var numericCols = GetNumericColumns(dataTable);
                        Debug.WriteLine($"Found {numericCols.Count} numeric columns: {string.Join(", ", numericCols)}");
                        
                        if (numericCols.Count < 2)
                        {
                            throw new InvalidOperationException("At least 2 numeric columns are required for correlation analysis.");
                        }
                        
                        Debug.WriteLine("Calculating correlation matrix...");
                        var corrMatrix = CalculateCorrelationMatrix(dataTable, numericCols);
                        Debug.WriteLine("Correlation matrix calculated successfully");
                        
                        return (corrMatrix, numericCols);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Database operation error: {ex.Message}");
                        throw;
                    }
                });

                LoadingMessage = "Generating visualization...";
                await Task.Delay(300);

                // Debug: Log correlation matrix info
                Debug.WriteLine($"Correlation matrix calculated for {numericColumns.Count} columns");
                if (numericColumns.Count > 0 && correlationMatrix.GetLength(0) > 0)
                {
                    Debug.WriteLine($"Sample correlation value [0,1]: {(numericColumns.Count > 1 ? correlationMatrix[0, 1].ToString() : "N/A")}");
                }

                CreateHeatmapChart(correlationMatrix, numericColumns);

                HasChartData = true;
                _dialogService.ShowInfoDialog($"Real correlation analysis completed for {numericColumns.Count} numeric fields from your database.", "Analysis Complete");
            }
            catch (Exception ex)
            {
                _dialogService.ShowErrorDialog($"Error generating heatmap: {ex.Message}", "Heatmap Error");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private DataTable ExecuteQuery(string connectionString, string query)
        {
            using var connection = new SqlConnection(connectionString);
            using var command = new SqlCommand(query, connection);
            using var adapter = new SqlDataAdapter(command);
            
            var dataTable = new DataTable();
            connection.Open();
            adapter.Fill(dataTable);
            
            return dataTable;
        }

        private List<string> GetNumericColumns(DataTable dataTable)
        {
            var numericColumns = new List<string>();
            
            foreach (DataColumn column in dataTable.Columns)
            {
                if (IsNumericType(column.DataType))
                {
                    numericColumns.Add(column.ColumnName);
                }
            }
            
            return numericColumns;
        }

        private bool IsNumericType(Type dataType)
        {
            return dataType == typeof(byte) || dataType == typeof(sbyte) ||
                   dataType == typeof(short) || dataType == typeof(ushort) ||
                   dataType == typeof(int) || dataType == typeof(uint) ||
                   dataType == typeof(long) || dataType == typeof(ulong) ||
                   dataType == typeof(float) || dataType == typeof(double) ||
                   dataType == typeof(decimal);
        }

        private double[,] CalculateCorrelationMatrix(DataTable dataTable, List<string> numericColumns)
        {
            int fieldCount = numericColumns.Count;
            double[,] correlationMatrix = new double[fieldCount, fieldCount];
            
            // Convert data to numeric arrays
            var fieldData = new List<double[]>();
            
            foreach (var columnName in numericColumns)
            {
                var values = new List<double>();
                foreach (DataRow row in dataTable.Rows)
                {
                    if (row[columnName] != DBNull.Value && 
                        double.TryParse(row[columnName].ToString(), out double value))
                    {
                        values.Add(value);
                    }
                }
                fieldData.Add(values.ToArray());
            }

            // Calculate Pearson correlation coefficients
            for (int i = 0; i < fieldCount; i++)
            {
                for (int j = 0; j < fieldCount; j++)
                {
                    if (i == j)
                    {
                        correlationMatrix[i, j] = 1.0;
                    }
                    else
                    {
                        correlationMatrix[i, j] = CalculatePearsonCorrelation(fieldData[i], fieldData[j]);
                    }
                }
            }

            return correlationMatrix;
        }

        private double CalculatePearsonCorrelation(double[] x, double[] y)
        {
            if (x.Length != y.Length || x.Length == 0) return 0;

            double meanX = x.Average();
            double meanY = y.Average();

            double numerator = x.Zip(y, (xi, yi) => (xi - meanX) * (yi - meanY)).Sum();
            double denomX = Math.Sqrt(x.Sum(xi => Math.Pow(xi - meanX, 2)));
            double denomY = Math.Sqrt(y.Sum(yi => Math.Pow(yi - meanY, 2)));

            if (denomX == 0 || denomY == 0) return 0;

            return numerator / (denomX * denomY);
        }

        private void CreateHeatmapChart(double[,] correlationMatrix, List<string> fieldNames)
        {
            try
            {
                Debug.WriteLine("=== CreateHeatmapChart START ===");
                
                // Force clear and notify
                ChartSeries.Clear();
                OnPropertyChanged(nameof(ChartSeries));
                
                int size = fieldNames.Count;
                Debug.WriteLine($"Creating correlation heatmap for {size} fields");
                
                // Create a comprehensive data series showing all correlations in a single line
                var correlationSeries = new XyDataSeries<double, double> 
                { 
                    SeriesName = "Correlation Matrix",
                    AcceptsUnsortedData = true
                };
                
                int index = 0;
                Debug.WriteLine("Correlation Matrix Values:");
                Debug.WriteLine("==========================");
                
                // Print correlation matrix and add to chart
                for (int i = 0; i < size; i++)
                {
                    for (int j = 0; j < size; j++)
                    {
                        double correlation = correlationMatrix[i, j];
                        if (!double.IsNaN(correlation) && !double.IsInfinity(correlation))
                        {
                            correlationSeries.Append(index, correlation);
                            
                            // Print meaningful correlation info
                            if (i != j) // Skip self-correlations (always 1.0)
                            {
                                Debug.WriteLine($"{fieldNames[i]} vs {fieldNames[j]}: {correlation:F3}");
                            }
                            
                            index++;
                        }
                    }
                }
                
                Debug.WriteLine("==========================");
                Debug.WriteLine($"Total data points: {correlationSeries.Count}");
                
                // Create line chart to show correlation patterns
                var lineSeriesViewModel = new LineRenderableSeriesViewModel
                {
                    DataSeries = correlationSeries,
                    StrokeThickness = 2,
                    Stroke = Colors.Blue
                };
                
                ChartSeries.Add(lineSeriesViewModel);
                
                // Also create individual series for each field's correlations for better visualization
                var colors = new[] { Colors.Red, Colors.Green, Colors.Orange, Colors.Purple, Colors.Brown };
                
                for (int i = 0; i < Math.Min(size, 5); i++) // Limit to avoid clutter
                {
                    var fieldSeries = new XyDataSeries<double, double> 
                    { 
                        SeriesName = $"{fieldNames[i]} correlations",
                        AcceptsUnsortedData = true
                    };
                    
                    for (int j = 0; j < size; j++)
                    {
                        double correlation = correlationMatrix[i, j];
                        if (!double.IsNaN(correlation) && !double.IsInfinity(correlation))
                        {
                            fieldSeries.Append(j, correlation);
                        }
                    }
                    
                    var fieldLineViewModel = new LineRenderableSeriesViewModel
                    {
                        DataSeries = fieldSeries,
                        StrokeThickness = 1,
                        Stroke = colors[i % colors.Length]
                    };
                    
                    ChartSeries.Add(fieldLineViewModel);
                }
                
                // Force property notifications
                OnPropertyChanged(nameof(ChartSeries));
                
                // Update axis properties with meaningful labels
                XAxisTitle = "Field Pairs & Individual Field Index";
                YAxisTitle = "Correlation Coefficient (-1 to +1)";
                OnPropertyChanged(nameof(XAxisTitle));
                OnPropertyChanged(nameof(YAxisTitle));
                
                // Set chart data flag
                HasChartData = true;
                OnPropertyChanged(nameof(HasChartData));
                
                Debug.WriteLine($"Correlation visualization created successfully. ChartSeries count: {ChartSeries.Count}");
                Debug.WriteLine("=== CreateHeatmapChart END ===");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error creating correlation heatmap: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                
                // Fallback to single line chart
                CreateFallbackLineChart(correlationMatrix, fieldNames);
            }
        }
        
        private void CreateFallbackLineChart(double[,] correlationMatrix, List<string> fieldNames)
        {
            try
            {
                Debug.WriteLine("Creating fallback line chart...");
                
                var lineSeries = new XyDataSeries<double, double> 
                { 
                    SeriesName = "Correlation Values",
                    AcceptsUnsortedData = true
                };
                
                int index = 0;
                for (int i = 0; i < fieldNames.Count; i++)
                {
                    for (int j = 0; j < fieldNames.Count; j++)
                    {
                        double correlation = correlationMatrix[i, j];
                        if (!double.IsNaN(correlation) && !double.IsInfinity(correlation))
                        {
                            lineSeries.Append(index++, correlation);
                        }
                    }
                }
                
                var lineSeriesViewModel = new LineRenderableSeriesViewModel
                {
                    DataSeries = lineSeries,
                    StrokeThickness = 2,
                    Stroke = Colors.Green
                };

                ChartSeries.Add(lineSeriesViewModel);
                
                XAxisTitle = "Correlation Index";
                YAxisTitle = "Correlation Value";
                HasChartData = true;
                
                Debug.WriteLine("Fallback line chart created successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error creating fallback chart: {ex.Message}");
                _dialogService.ShowErrorDialog($"Error creating correlation chart: {ex.Message}", "Chart Error");
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public static class RandomExtensions
    {
        public static double NextGaussian(this Random random, double mean = 0, double stdDev = 1)
        {
            static double BoxMuller(Random rand)
            {
                double u1 = 1.0 - rand.NextDouble();
                double u2 = 1.0 - rand.NextDouble();
                return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
            }

            return mean + stdDev * BoxMuller(random);
        }
    }
}