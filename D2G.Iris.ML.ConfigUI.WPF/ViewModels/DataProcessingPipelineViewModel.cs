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

        public DataProcessingPipelineViewModel()
        {
            _dataBalancingViewModel = new DataBalancingViewModel();
            _featureEngineeringViewModel = new FeatureEngineeringViewModel();

            // Subscribe to changes in child ViewModels
            _dataBalancingViewModel.PropertyChanged += OnChildViewModelPropertyChanged;
            _featureEngineeringViewModel.PropertyChanged += OnChildViewModelPropertyChanged;
        }

        #region Properties

        // Direct access to child ViewModels - no duplication
        public DataBalancingViewModel DataBalancing => _dataBalancingViewModel;
        public FeatureEngineeringViewModel FeatureEngineering => _featureEngineeringViewModel;

        // Computed properties for UI binding - no storage duplication
        public bool IsDataBalancingEnabled
        {
            get => _dataBalancingViewModel.IsEnabled;
            set
            {
                if (value != _dataBalancingViewModel.IsEnabled)
                {
                    // Toggle method selection to enable/disable
                    _dataBalancingViewModel.SelectedMethod = value
                        ? DataBalanceMethod.SMOTE
                        : DataBalanceMethod.None;
                }
            }
        }

        public bool IsFeatureEngineeringEnabled
        {
            get => _featureEngineeringViewModel.SelectedMethod != FeatureSelectionMethod.None;
            set
            {
                var currentlyEnabled = _featureEngineeringViewModel.SelectedMethod != FeatureSelectionMethod.None;
                if (value != currentlyEnabled)
                {
                    // Toggle method selection to enable/disable
                    _featureEngineeringViewModel.SelectedMethod = value
                        ? FeatureSelectionMethod.Correlation
                        : FeatureSelectionMethod.None;
                }
            }
        }

        // Direct binding to child properties - no string conversion
        public int DataBalancingExecutionOrder
        {
            get => _dataBalancingViewModel.ExecutionOrder;
            set => _dataBalancingViewModel.ExecutionOrder = value;
        }

        public int FeatureEngineeringExecutionOrder
        {
            get => _featureEngineeringViewModel.ExecutionOrder;
            set => _featureEngineeringViewModel.ExecutionOrder = value;
        }

        #endregion

        #region Private Methods

        private void OnChildViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // Notify UI when relevant properties change
            if (e.PropertyName == nameof(DataBalancingViewModel.IsEnabled))
            {
                OnPropertyChanged(nameof(IsDataBalancingEnabled));
            }
            else if (e.PropertyName == nameof(FeatureEngineeringViewModel.SelectedMethod))
            {
                OnPropertyChanged(nameof(IsFeatureEngineeringEnabled));
            }
            else if (e.PropertyName == nameof(DataBalancingViewModel.ExecutionOrder))
            {
                OnPropertyChanged(nameof(DataBalancingExecutionOrder));
            }
            else if (e.PropertyName == nameof(FeatureEngineeringViewModel.ExecutionOrder))
            {
                OnPropertyChanged(nameof(FeatureEngineeringExecutionOrder));
            }
        }

        #endregion

        #region Public Methods

        public void LoadFromConfig(ModelConfig config)
        {
            _dataBalancingViewModel.SetConfiguration(config.DataBalancing);
            _featureEngineeringViewModel.SetConfiguration(config.FeatureEngineering);
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _dataBalancingViewModel.PropertyChanged -= OnChildViewModelPropertyChanged;
                _featureEngineeringViewModel.PropertyChanged -= OnChildViewModelPropertyChanged;

                _dataBalancingViewModel.Dispose();
                _featureEngineeringViewModel.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}