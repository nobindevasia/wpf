using D2G.Iris.ML.ConfigUI.WPF.Commands;
using D2G.Iris.ML.ConfigUI.WPF.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace D2G.Iris.ML.ConfigUI.WPF.ViewModels
{
    class TrainingLogsViewModel : BaseViewModel
    {
        private string _LongText = "";
        private readonly TextWriter _originalConsoleOut;

        public TrainingLogsViewModel()
        {
            LogEntries = new ObservableCollection<LogEntry>();
            InitializeCommands();
            _originalConsoleOut = Console.Out;
            RedirectConsoleOutput();
            LogMessage("Welcome to Iris ML Configuration Tool", "Info");
            LogMessage("Use the tabs to configure your model settings and click 'Launch Training' to start training", "Info");

        }

        #region Properties

        public ObservableCollection<LogEntry> LogEntries { get; }

        public string LongText
        {
            get => _LongText;
            set => SetProperty(ref _LongText, value);
        }

        #endregion

        #region Commands

        public ICommand ClearLogsCommand { get; private set; } = null!;
        public ICommand SaveLogsCommand { get; private set; } = null!;

        #endregion
        private void InitializeCommands()
        {
            ClearLogsCommand = new RelayCommand(ClearLogs);
            SaveLogsCommand = new RelayCommand(SaveLogs);
        }

        private void RedirectionConsoleOutput()
        {
            var textWriter = new ConsoleTextWriter(this);
            Console.SetOut(textWriter);
        }

        public void LogMessage(string message, string level)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;
            Application.Current?.Dispatcher.Invoke(() =>
            {
                var logEntry = new LogEntry
                {
                    Timestamp = DateTime.Now,
                    Level = level,
                    Message = message
                };
                logEntries.Add(logEntry);

                string timestamp = logEntry.Timestamp.ToString("yyyy-MM-dd HH:mm:ss");
                string formattedMessage = $"[{timestamp}] [{level}] {message}{Environment.NewLine}";
                LogText += formattedMessage;

                while (logEntries.Count > 1000)
                {
                    logEntries.RemoveAt(0);
                }
            });
        }

        public void ClearLogs()
        {
            Application.Current?.Dispatcher.Invoke(() =>
            {
                logEntries.Clear();
                LogText = "";
            });
        }
        private void SaveLogs()
        {
            try
            {
                string filename = $"Training_Log_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                File.WriteAllText(filename, LogText);
                LogMessage($"Logs saved to:{filename}", "Info");
            }
            catch (Exception ex)
            {
                LogMessage($"Error saving logs: {ex.Message}", "Error");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)

            {
                Console.SetOut(_originalConsoleOut);
            }
            base.Dispose(disposing);
        }
    }

    public class ConsoleTextWriter : TextWriter
    {
        private readonly TrainingLogsViewModel _logsViewModel;
        public ConsoleTextWriter(TrainingLogsViewModel logsViewModel)
        { 
        _logsViewModel = logsViewModel;
        }

        public override void Write(string? value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _logsViewModel.LogMessage(value, "Console");
            }
        }

        public override void WriteLine(string? value)
        {
            if (!string.IsNullOrEmpty(value))

            {
                _logsViewModel.LogMessage(value, "Console");
            }
        }
        public override Encoding Encoding => Encoding.UTF8;
    }

}
