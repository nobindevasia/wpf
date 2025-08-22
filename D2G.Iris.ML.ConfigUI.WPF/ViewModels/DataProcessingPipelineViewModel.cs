using System;
using System.ComponentModel;
using D2G.Iris.ML.Core.Enums;
using D2G.Iris.ML.Core.Models;

namespace D2G.Iris.ML.ConfigUI.WPF.ViewModels
{
    public class DataProcessingPipelineViewModel : BaseViewModel
    {
        private readonly DataBalancingViewModel _dataBalancingViewModel;
        private readonly FeatureEngineeringViewModel _featureEngineeringViewModel;
        
        
        // New properties for checkboxes and execution order
        private bool _isDataBalancingEnabled = false;
        private bool _isFeatureEngineeringEnabled = false;
        private string _dataBalancingExecutionOrder = "1";
        private string _featureEngineeringExecutionOrder = "2";

        public DataProcessingPipelineViewModel()
        {
            _dataBalancingViewModel = new DataBalancingViewModel();
            _featureEngineeringViewModel = new FeatureEngineeringViewModel();

            // Subscribe to changes
            _dataBalancingViewModel.PropertyChanged += OnChildViewModelPropertyChanged;
            _featureEngineeringViewModel.PropertyChanged += OnChildViewModelPropertyChanged;

            UpdateAllStatus();
        }

        #region Properties

        public bool IsDataBalancingEnabled
        {
            get => _isDataBalancingEnabled;
            set
            {
                if (SetProperty(ref _isDataBalancingEnabled, value))
                {
                    // Update the DataBalancingViewModel's IsEnabled state
                    _dataBalancingViewModel.IsEnabled = value;
                    
                    // When checkbox changes, update the method selection
                    if (value && _dataBalancingViewModel.SelectedMethod == DataBalanceMethod.None)
                    {
                        _dataBalancingViewModel.SelectedMethod = DataBalanceMethod.SMOTE;
                    }
                    else if (!value)
                    {
                        _dataBalancingViewModel.SelectedMethod = DataBalanceMethod.None;
                    }
                }
            }
        }

        public bool IsFeatureEngineeringEnabled
        {
            get => _isFeatureEngineeringEnabled;
            set
            {
                if (SetProperty(ref _isFeatureEngineeringEnabled, value))
                {
                    // When checkbox changes, update the method selection
                    if (value && _featureEngineeringViewModel.SelectedMethod == FeatureSelectionMethod.None)
                    {
                        _featureEngineeringViewModel.SelectedMethod = FeatureSelectionMethod.Correlation;
                    }
                    else if (!value)
                    {
                        _featureEngineeringViewModel.SelectedMethod = FeatureSelectionMethod.None;
                    }
                }
            }
        }

        public string DataBalancingExecutionOrder
        {
            get => _dataBalancingExecutionOrder;
            set
            {
                if (SetProperty(ref _dataBalancingExecutionOrder, value))
                {
                    if (int.TryParse(value, out int order))
                    {
                        _dataBalancingViewModel.ExecutionOrder = order;
                        }
                }
            }
        }

        public string FeatureEngineeringExecutionOrder
        {
            get => _featureEngineeringExecutionOrder;
            set
            {
                if (SetProperty(ref _featureEngineeringExecutionOrder, value))
                {
                    if (int.TryParse(value, out int order))
                    {
                        _featureEngineeringViewModel.ExecutionOrder = order;
                        }
                }
            }
        }

        public DataBalancingViewModel DataBalancing => _dataBalancingViewModel;
        public FeatureEngineeringViewModel FeatureEngineering => _featureEngineeringViewModel;

        #endregion

        #region Private Methods

        private void OnChildViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DataBalancingViewModel.ExecutionOrder))
            {
                _dataBalancingExecutionOrder = _dataBalancingViewModel.ExecutionOrder.ToString();
                OnPropertyChanged(nameof(DataBalancingExecutionOrder));
            }
            else if (e.PropertyName == nameof(FeatureEngineeringViewModel.ExecutionOrder))
            {
                _featureEngineeringExecutionOrder = _featureEngineeringViewModel.ExecutionOrder.ToString();
                OnPropertyChanged(nameof(FeatureEngineeringExecutionOrder));
            }
            else if (e.PropertyName == nameof(DataBalancingViewModel.SelectedMethod))
            {
                // Sync checkbox with method selection
                bool shouldBeEnabled = _dataBalancingViewModel.SelectedMethod != DataBalanceMethod.None;
                if (_isDataBalancingEnabled != shouldBeEnabled)
                {
                    _isDataBalancingEnabled = shouldBeEnabled;
                    OnPropertyChanged(nameof(IsDataBalancingEnabled));
                }
            }
            else if (e.PropertyName == nameof(FeatureEngineeringViewModel.SelectedMethod))
            {
                // Sync checkbox with method selection  
                bool shouldBeEnabled = _featureEngineeringViewModel.SelectedMethod != FeatureSelectionMethod.None;
                if (_isFeatureEngineeringEnabled != shouldBeEnabled)
                {
                    _isFeatureEngineeringEnabled = shouldBeEnabled;
                    OnPropertyChanged(nameof(IsFeatureEngineeringEnabled));
                }
            }
        }

        private void UpdateAllStatus()
        {
            // Simplified - no longer need to update headers or pipeline status
            // The UI is self-contained with checkboxes and dropdowns
        }

        #endregion

        #region Public Methods

        public void LoadFromConfig(ModelConfig config)
        {
            _dataBalancingViewModel.SetConfiguration(config.DataBalancing);
            _featureEngineeringViewModel.SetConfiguration(config.FeatureEngineering);
            
            // Update checkbox and dropdown states based on loaded config
            _isDataBalancingEnabled = _dataBalancingViewModel.SelectedMethod != DataBalanceMethod.None;
            _isFeatureEngineeringEnabled = _featureEngineeringViewModel.SelectedMethod != FeatureSelectionMethod.None;
            _dataBalancingExecutionOrder = _dataBalancingViewModel.ExecutionOrder.ToString();
            _featureEngineeringExecutionOrder = _featureEngineeringViewModel.ExecutionOrder.ToString();
            
            // Set the IsEnabled state in child view models
            _dataBalancingViewModel.IsEnabled = _isDataBalancingEnabled;
            
            // Notify UI of changes
            OnPropertyChanged(nameof(IsDataBalancingEnabled));
            OnPropertyChanged(nameof(IsFeatureEngineeringEnabled));
            OnPropertyChanged(nameof(DataBalancingExecutionOrder));
            OnPropertyChanged(nameof(FeatureEngineeringExecutionOrder));
        }

        public void SaveToConfig(ModelConfig config)
        {
            config.DataBalancing = _dataBalancingViewModel.GetConfiguration();
            config.FeatureEngineering = _featureEngineeringViewModel.GetConfiguration();
        }

        public void ResetToDefaults()
        {
            _dataBalancingViewModel.SetConfiguration(null);
            _featureEngineeringViewModel.SetConfiguration(null);
        }

        #endregion
    }
}