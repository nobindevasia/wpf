using D2G.Iris.ML.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D2G.Iris.ML.ConfigUI.WPF.ViewModels
{
    public class AutoMLSettingsViewModel : BaseViewModel
    {
        private bool _isEnabled;
        private int _maxExperimentTimeInSeconds = 30;
        private string _optimizingMetric = "Accuracy";
        private string _description = "AutoML is disabled. Traditional training will be used with the algorithm specified in Training Parameters.";

        public AutoMLSettingsViewModel()
        {
            UpdateDescription();
        }

        #region Properties

        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (SetProperty(ref _isEnabled, value))
                {
                    UpdateDescription();
                }
            }
        }

        public int MaxExperimentTimeInSeconds
        {
            get => _maxExperimentTimeInSeconds;
            set => SetProperty(ref _maxExperimentTimeInSeconds, System.Math.Max(1, System.Math.Min(3600, value)));
        }

        public string OptimizingMetric
        {
            get => _optimizingMetric;
            set => SetProperty(ref _optimizingMetric, value);
        }

        public string Description
        {
            get => _description;
            private set => SetProperty(ref _description, value);
        }

        public List<string> AvailableMetrics { get; } = new()
        {
            "Accuracy",
            "AUC",
            "F1Score",
            "MicroAccuracy",
            "MacroAccuracy",
            "RSquared",
            "MeanAbsoluteError",
            "RootMeanSquaredError"
        };
        #endregion

        private void UpdateDescription()
        {
            Description = IsEnabled ? "AutoML will automatically try multiple algorithms and find the best performing model for your data."
                : "AutoML is disabled. Traditional training will be used with the algorithm specified in Training Parameters.";
        }

        public void SetConfiguration(AutoMLConfig? config)
        {
            if (config == null)
            {
                IsEnabled = false;
                MaxExperimentTimeInSeconds = 30;
                OptimizingMetric = "Accuracy";
                return;
            }
            IsEnabled = config.Enabled;
            MaxExperimentTimeInSeconds = config.MaxExperimentTimeInSeconds;
            OptimizingMetric = config.OptimizingMetric ?? "Accuracy";
        }

        public AutoMLConfig GetConfiguration()
        {
            return new AutoMLConfig
            {
                Enabled = IsEnabled,
                MaxExperimentTimeInSeconds = MaxExperimentTimeInSeconds,
                OptimizingMetric = OptimizingMetric
            };
        }
    }
}