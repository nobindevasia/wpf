using System;
using System.Collections.Generic;
using System.Linq;
using D2G.Iris.ML.Core.Enums;
using D2G.Iris.ML.Utils;

namespace D2G.Iris.ML.ConfigUI.WPF.ViewModels
{
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