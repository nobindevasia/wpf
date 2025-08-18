using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using D2G.Iris.ML.ConfigUI.WPF.Commands;
using D2G.Iris.ML.ConfigUI.WPF.Models;
using D2G.Iris.ML.ConfigUI.WPF.Services;
using D2G.Iris.ML.Core.Enums;
using D2G.Iris.ML.Core.Models;
using D2G.Iris.ML.Utils;

namespace D2G.Iris.ML.ConfigUI.WPF.ViewModels
{
    public class TrainingParametersViewModel : BaseViewModel
    {
        private readonly IDialogService _dialogService;
        private string _selectedAlgorithm = "fasttree";
        private decimal _testFraction = 0.2m;
        private ModelType _currentModelType = ModelType.BinaryClassification;
        private Models.ParameterItem? _selectedParameter;

        public TrainingParametersViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService;
            Parameters = new ObservableCollection<Models.ParameterItem>();
            InitializeCommands();
            UpdateAvailableAlgorithms();
        }

        #region Properties

        public string SelectedAlgorithm
        {
            get => _selectedAlgorithm;
            set
            {
                if (SetProperty(ref _selectedAlgorithm, value))
                {
                    OnAlgorithmChanged();
                }
            }
        }

        public decimal TestFraction
        {
            get => _testFraction;
            set => SetProperty(ref _testFraction, value);
        }

        public ObservableCollection<string> AvailableAlgorithms { get; } = new();

        public ObservableCollection<Models.ParameterItem> Parameters { get; }

        public Models.ParameterItem? SelectedParameter
        {
            get => _selectedParameter;
            set => SetProperty(ref _selectedParameter, value);
        }

        #endregion

        #region Commands

        public ICommand AddParameterCommand { get; private set; } = null!;
        public ICommand RemoveParameterCommand { get; private set; } = null!;

        #endregion

        private void InitializeCommands()
        {
            AddParameterCommand = new RelayCommand(AddParameter);
            RemoveParameterCommand = new RelayCommand(RemoveParameter, () => SelectedParameter != null);
        }

        public void SetModelType(ModelType modelType)
        {
            if (_currentModelType != modelType)
            {
                _currentModelType = modelType;
                UpdateAvailableAlgorithms();
                Parameters.Clear();
            }
        }

        private void UpdateAvailableAlgorithms()
        {
            AvailableAlgorithms.Clear();
            var algorithms = AlgorithmRegistry.GetAlgorithmsForModelType(_currentModelType);

            foreach (var algorithm in algorithms)
            {
                AvailableAlgorithms.Add(algorithm);
            }

            if (AvailableAlgorithms.Count > 0 && !AvailableAlgorithms.Contains(SelectedAlgorithm))
            {
                SelectedAlgorithm = AvailableAlgorithms[0];
            }
        }

        private void OnAlgorithmChanged()
        {
            Parameters.Clear();

            LoadAvailableParameters(SelectedAlgorithm);
        }

        private void LoadAvailableParameters(string algorithmName)
        {
            try
            {
                Type? optionsType = AlgorithmRegistry.GetOptionsType(algorithmName, _currentModelType);
                if (optionsType == null) return;

                // Clear existing parameters
                Parameters.Clear();

                // Get available parameters for this algorithm
                var properties = ParameterHelper.GetConfigurableProperties(optionsType);
                var fields = ParameterHelper.GetConfigurableFields(optionsType);

                // Add tooltips or descriptions for better UX
                OnPropertyChanged(nameof(AlgorithmTooltip));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading parameters for {algorithmName}: {ex.Message}");
            }
        }

        public string AlgorithmTooltip
        {
            get
            {
                try
                {
                    var optionsType = AlgorithmRegistry.GetOptionsType(_selectedAlgorithm, _currentModelType);
                    return optionsType != null
                        ? ParameterHelper.CreateParameterTooltip(optionsType)
                        : "No parameter information available.";
                }
                catch
                {
                    return "Error loading parameter information.";
                }
            }
        }

        private void AddParameter()
        {
            var dialogViewModel = new ParameterDialogViewModel(_selectedAlgorithm, _currentModelType);
            var result = _dialogService.ShowParameterDialog(dialogViewModel);

            if (result == true)
            {
                var existingParam = Parameters.FirstOrDefault(p =>
                    string.Equals(p.Name, dialogViewModel.ParameterName, StringComparison.OrdinalIgnoreCase));

                if (existingParam != null)
                {
                    if (_dialogService.ShowConfirmationDialog(
                        $"Parameter '{dialogViewModel.ParameterName}' already exists. Do you want to update it?",
                        "Duplicate Parameter"))
                    {
                        existingParam.Value = dialogViewModel.ParameterValue;
                    }
                    return;
                }

                Parameters.Add(new Models.ParameterItem
                {
                    Name = dialogViewModel.ParameterName,
                    Value = dialogViewModel.ParameterValue,
                    DisplayText = $"{dialogViewModel.ParameterName} ({dialogViewModel.ParameterValue?.GetType().Name ?? "object"})"
                });
            }
        }

        private void RemoveParameter()
        {
            if (SelectedParameter == null) return;

            if (_dialogService.ShowConfirmationDialog(
                $"Are you sure you want to remove the parameter '{SelectedParameter.Name}'?",
                "Confirm Removal"))
            {
                Parameters.Remove(SelectedParameter);
            }
        }

        public void SetConfiguration(TrainingParameters? parameters)
        {
            if (parameters == null) return;

            if (!string.IsNullOrEmpty(parameters.Algorithm))
            {
                var matchingAlgorithm = AvailableAlgorithms.FirstOrDefault(a =>
                    string.Equals(a, parameters.Algorithm, StringComparison.OrdinalIgnoreCase));

                SelectedAlgorithm = matchingAlgorithm ?? (AvailableAlgorithms.Count > 0 ? AvailableAlgorithms[0] : "fasttree");
            }

            TestFraction = (decimal)parameters.TestFraction;

            Parameters.Clear();
            if (parameters.AlgorithmParameters != null)
            {
                foreach (var param in parameters.AlgorithmParameters)
                {
                    Parameters.Add(new Models.ParameterItem
                    {
                        Name = param.Key,
                        Value = param.Value,
                        DisplayText = $"{param.Key} ({param.Value?.GetType().Name ?? "object"})"
                    });
                }
            }
        }

        public TrainingParameters GetConfiguration()
        {
            var algorithmParameters = new Dictionary<string, object>();
            foreach (var param in Parameters)
            {
                if (!string.IsNullOrEmpty(param.Name) && param.Value != null)
                {
                    algorithmParameters[param.Name] = param.Value;
                }
            }

            return new TrainingParameters
            {
                Algorithm = SelectedAlgorithm,
                TestFraction = (double)TestFraction,
                AlgorithmParameters = algorithmParameters
            };
        }
    }

    public class ParameterDialogViewModel : BaseViewModel
    {
        private string _parameterName = "";
        private string _parameterValueString = "";
        private object? _parameterValue;
        private string _valueHint = "";
        private string _errorMessage = "";
        private bool _hasError;
        private readonly string _algorithmName;
        private readonly ModelType _modelType;
        private Type? _selectedParameterType;

        public ParameterDialogViewModel(string algorithmName, ModelType modelType)
        {
            _algorithmName = algorithmName;
            _modelType = modelType;
            LoadAvailableParameters();
        }

        #region Properties

        public string ParameterName
        {
            get => _parameterName;
            set
            {
                if (SetProperty(ref _parameterName, value))
                {
                    OnParameterNameChanged();
                    OnPropertyChanged(nameof(IsValid));
                }
            }
        }

        public string ParameterValueString
        {
            get => _parameterValueString;
            set
            {
                if (SetProperty(ref _parameterValueString, value))
                {
                    ValidateAndSetValue();
                    OnPropertyChanged(nameof(IsValid));
                }
            }
        }

        public object? ParameterValue
        {
            get => _parameterValue;
            private set => SetProperty(ref _parameterValue, value);
        }

        public string ValueHint
        {
            get => _valueHint;
            private set => SetProperty(ref _valueHint, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            private set => SetProperty(ref _errorMessage, value);
        }

        public bool HasError
        {
            get => _hasError;
            private set => SetProperty(ref _hasError, value);
        }

        public List<string> AvailableParameters { get; private set; } = new List<string>();

        public string AlgorithmTooltip { get; private set; } = "";

        public bool IsValid => !HasError && !string.IsNullOrWhiteSpace(ParameterName) && !string.IsNullOrWhiteSpace(ParameterValueString);

        #endregion

        private void LoadAvailableParameters()
        {
            try
            {
                var optionsType = AlgorithmRegistry.GetOptionsType(_algorithmName, _modelType);
                if (optionsType != null)
                {
                    AvailableParameters = ParameterHelper.GetParameterDisplayList(optionsType);
                    AlgorithmTooltip = ParameterHelper.CreateParameterTooltip(optionsType);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading parameters: {ex.Message}");
            }
        }

        private void OnParameterNameChanged()
        {
            ClearError();

            try
            {
                var optionsType = AlgorithmRegistry.GetOptionsType(_algorithmName, _modelType);
                if (optionsType == null) return;

                // Find the property or field
                var property = optionsType.GetProperty(_parameterName);
                var field = optionsType.GetField(_parameterName);

                if (property != null)
                {
                    _selectedParameterType = property.PropertyType;
                    ValueHint = ParameterHelper.GetValueHint(property.PropertyType);

                    // Set default value
                    if (string.IsNullOrEmpty(ParameterValueString))
                    {
                        ParameterValueString = ParameterHelper.GetDefaultValue(property.PropertyType);
                    }
                }
                else if (field != null)
                {
                    _selectedParameterType = field.FieldType;
                    ValueHint = ParameterHelper.GetValueHint(field.FieldType);

                    // Set default value
                    if (string.IsNullOrEmpty(ParameterValueString))
                    {
                        ParameterValueString = ParameterHelper.GetDefaultValue(field.FieldType);
                    }
                }
                else
                {
                    _selectedParameterType = null;
                    ValueHint = "Enter a value";
                }
            }
            catch (Exception ex)
            {
                SetError($"Error getting parameter info: {ex.Message}");
            }
        }

        private void ValidateAndSetValue()
        {
            ClearError();

            if (string.IsNullOrWhiteSpace(ParameterValueString))
            {
                ParameterValue = null;
                return;
            }

            if (_selectedParameterType == null)
            {
                // Fallback to basic parsing
                TryBasicParsing();
                return;
            }

            try
            {
                ParameterValue = ParameterHelper.ConvertParameterValue(ParameterValueString, _selectedParameterType);
            }
            catch (Exception ex)
            {
                SetError($"Invalid value for {ParameterHelper.GetFriendlyTypeName(_selectedParameterType)}: {ex.Message}");
                // Still try basic parsing as fallback
                TryBasicParsing();
            }
        }

        private void TryBasicParsing()
        {
            try
            {
                // Try to parse the value based on common types
                if (int.TryParse(ParameterValueString, out int intValue))
                    ParameterValue = intValue;
                else if (double.TryParse(ParameterValueString, out double doubleValue))
                    ParameterValue = doubleValue;
                else if (bool.TryParse(ParameterValueString, out bool boolValue))
                    ParameterValue = boolValue;
                else
                    ParameterValue = ParameterValueString;
            }
            catch
            {
                ParameterValue = ParameterValueString;
            }
        }

        private void SetError(string message)
        {
            ErrorMessage = message;
            HasError = true;
            OnPropertyChanged(nameof(IsValid));
        }

        private void ClearError()
        {
            ErrorMessage = "";
            HasError = false;
            OnPropertyChanged(nameof(IsValid));
        }
    }
}