using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using D2G.Iris.ML.ConfigUI.WPF.Services;
using D2G.Iris.ML.ConfigUI.WPF.ViewModels;

namespace D2G.Iris.ML.ConfigUI.WPF
{
    public partial class App : Application
    {
        private ServiceProvider? _serviceProvider;

        protected override void OnStartup(StartupEventArgs e)
        {
            var services = new ServiceCollection();

            // Register Services
            services.AddSingleton<IConfigurationService, ConfigurationService>();
            services.AddSingleton<IDatabaseSchemaLoader, DatabaseSchemaLoader>();
            services.AddSingleton<IDialogService, DialogService>();

            // Register ViewModels
            services.AddTransient<MainWindowViewModel>();
            services.AddTransient<GeneralSettingsViewModel>();
            services.AddTransient<DatabaseSettingsViewModel>();
            services.AddTransient<InputFieldsViewModel>();
            services.AddTransient<TrainingParametersViewModel>();
            services.AddTransient<DataBalancingViewModel>();
            services.AddTransient<FeatureEngineeringViewModel>();
            services.AddTransient<AutoMLSettingsViewModel>();
            services.AddTransient<TrainingLogsViewModel>();

            // Build the service provider
            _serviceProvider = services.BuildServiceProvider();

            // Create and show the main window
            var mainWindowViewModel = _serviceProvider.GetRequiredService<MainWindowViewModel>();
            var mainWindow = new MainWindow(mainWindowViewModel);

            mainWindow.Show();

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _serviceProvider?.Dispose();
            base.OnExit(e);
        }
    }
}