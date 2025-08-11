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

        public string ValueString
        {
            get => _value?.ToString() ?? string.Empty;
            set
            {
                _value = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Value));
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