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
        private ParameterItem? _selectedParameter;

        public TrainingParametersViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService;
            Parameters = new ObservableCollection<ParameterItem>();
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

        public ObservableCollection<ParameterItem> Parameters { get; }

        public ParameterItem? SelectedParameter
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
            // Clear existing parameters when algorithm changes
            Parameters.Clear();

            // Load default parameters if available
            LoadAvailableParameters(SelectedAlgorithm);
        }

        private void LoadAvailableParameters(string algorithmName)
        {
            try
            {
                Type? optionsType = AlgorithmRegistry.GetOptionsType(algorithmName, _currentModelType);
                if (optionsType == null) return;

                var properties = ParameterHelper.GetConfigurableProperties(optionsType);
                var fields = ParameterHelper.GetConfigurableFields(optionsType);

                // Add some default parameters based on the algorithm
                if (algorithmName.ToLower() == "fasttree" || algorithmName.ToLower() == "fastforest")
                {
                    Parameters.Add(new ParameterItem
                    {
                        Name = "NumberOfLeaves",
                        Value = 20,
                        DisplayText = "NumberOfLeaves (int)"
                    });
                }
                else if (algorithmName.ToLower() == "lightgbm")
                {
                    Parameters.Add(new ParameterItem
                    {
                        Name = "NumberOfLeaves",
                        Value = 31,
                        DisplayText = "NumberOfLeaves (int)"
                    });
                    Parameters.Add(new ParameterItem
                    {
                        Name = "LearningRate",
                        Value = 0.1,
                        DisplayText = "LearningRate (double)"
                    });
                }
            }
            catch (Exception)
            {
                // If we can't load parameters, that's okay, just continue
            }
        }

        private void AddParameter()
        {
            var dialogViewModel = new ParameterDialogViewModel(_selectedAlgorithm, _currentModelType);
            var dialog = _dialogService.ShowDialog<ParameterDialog>(dialogViewModel);

            if (dialog?.DialogResult == true)
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

                Parameters.Add(new ParameterItem
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
                    Parameters.Add(new ParameterItem
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
        private object? _parameterValue;
        private readonly string _algorithmName;
        private readonly ModelType _modelType;

        public ParameterDialogViewModel(string algorithmName, ModelType modelType)
        {
            _algorithmName = algorithmName;
            _modelType = modelType;
        }

        public string ParameterName
        {
            get => _parameterName;
            set => SetProperty(ref _parameterName, value);
        }

        public object? ParameterValue
        {
            get => _parameterValue;
            set => SetProperty(ref _parameterValue, value);
        }

        public string ParameterValueString
        {
            get => _parameterValue?.ToString() ?? "";
            set
            {
                // Try to parse the value based on common types
                if (int.TryParse(value, out int intValue))
                    ParameterValue = intValue;
                else if (double.TryParse(value, out double doubleValue))
                    ParameterValue = doubleValue;
                else if (bool.TryParse(value, out bool boolValue))
                    ParameterValue = boolValue;
                else
                    ParameterValue = value;
            }
        }
    }
}