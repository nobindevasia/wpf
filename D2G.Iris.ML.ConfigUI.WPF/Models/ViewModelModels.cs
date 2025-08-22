using System;
using System.Reflection;
using D2G.Iris.ML.ConfigUI.WPF.ViewModels;

namespace D2G.Iris.ML.ConfigUI.WPF.Models
{
    public class InputFieldItem : BaseViewModel
    {
        private string _name = string.Empty;
        private bool _isEnabled = true;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }
    }

    public class ParameterItem : BaseViewModel
    {
        private string _name = string.Empty;
        private object? _value;
        private string _displayText = string.Empty;
        private PropertyInfo? _property;
        private Type? _expectedType;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public object? Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }

        public string DisplayText
        {
            get => _displayText;
            set => SetProperty(ref _displayText, value);
        }

        public PropertyInfo? Property
        {
            get => _property;
            set => SetProperty(ref _property, value);
        }

        public Type? ExpectedType
        {
            get => _expectedType;
            set => SetProperty(ref _expectedType, value);
        }

        public string ValueString
        {
            get => _value?.ToString() ?? string.Empty;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    _value = null;
                }
                else
                {
                    _value = ConvertToExpectedType(value);
                }
                OnPropertyChanged();
                OnPropertyChanged(nameof(Value));
            }
        }

        private object? ConvertToExpectedType(string value)
        {
            if (_expectedType == null)
                return value; // Fallback to string if type unknown

            try
            {
                // Handle nullable types
                var targetType = Nullable.GetUnderlyingType(_expectedType) ?? _expectedType;

                if (targetType == typeof(string))
                    return value;
                else if (targetType == typeof(int))
                    return int.Parse(value);
                else if (targetType == typeof(double))
                    return double.Parse(value);
                else if (targetType == typeof(float))
                    return float.Parse(value);
                else if (targetType == typeof(decimal))
                    return decimal.Parse(value);
                else if (targetType == typeof(bool))
                    return bool.Parse(value);
                else if (targetType.IsEnum)
                    return Enum.Parse(targetType, value, true);
                else
                    return Convert.ChangeType(value, targetType);
            }
            catch
            {
                // If conversion fails, return the string value
                return value;
            }
        }
    }

    public class LogEntry : BaseViewModel
    {
        private DateTime _timestamp;
        private string _level = string.Empty;
        private string _message = string.Empty;

        public DateTime Timestamp
        {
            get => _timestamp;
            set => SetProperty(ref _timestamp, value);
        }

        public string Level
        {
            get => _level;
            set => SetProperty(ref _level, value);
        }

        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }
    }
}