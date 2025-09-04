using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SciChart.Charting.Visuals;
using D2G.Iris.ML.ConfigUI.WPF.ViewModels;
using System.Collections.ObjectModel;
using SciChart.Charting.Model.ChartSeries;

namespace D2G.Iris.ML.ConfigUI.WPF.Views
{
    public partial class VisualisationView : UserControl
    {
        public VisualisationView()
        {
            SciChartSurface.SetRuntimeLicenseKey(
                "dkBLL7cFRf2kjrIDBv5UnMIOlzcrRi44w3/vQzJbsvsQ1s/F7BAmoG368pM381psDnVdB7vAyW2uH1Hs9A9gN/9nJIL9mQ4RHgln" +
                "e+3ozSqZ+p7gF3tYGzMoDzG6noqUxpROjhpJh4gLxOt0CJxnp4ppf2EnxjIK70IuYIJHWUxvL91WPMjCoWYNAkN8V0JBNC86KRpj9G" +
                "U6C7d1AQIp7YRC5pR2DhlxQILJ095mWDYiewPQ2GO1G26CMfMWxmevohKyGZkQgtzJTynusv7E7b/NGnnlKrGlvGujlDoMuRiRH9XkA" +
                "fGCQm4bWnixsuHX3fIs7lS4IRKL1AWSPVheYnyPjrhnQPsw" +
                "bTVUBBMeXwgv22mVAfABvu1fxFWBO+WHjcC57GLWww/dp8KfI3Pa6oXy" +
                "lI+KvgaXKysk/g3uc7TpwiNhNNMbHVy80/GIrYvhO3qmc0Uobdb0k9xENdLyL5gRHWgEoKmkban7f4QCsDrIoh2klh38eoE="
            );
            
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }
        
        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is VisualisationViewModel viewModel)
            {
                Console.WriteLine("VisualisationView DataContext set");
                
                // Subscribe to ChartSeries changes
                if (viewModel.ChartSeries != null)
                {
                    UpdateChartSeries(viewModel.ChartSeries);
                    viewModel.ChartSeries.CollectionChanged += OnChartSeriesChanged;
                }
            }
        }
        
        private void OnChartSeriesChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            Console.WriteLine($"ChartSeries collection changed: Action={e.Action}");
            
            if (sender is ObservableCollection<IRenderableSeriesViewModel> chartSeries)
            {
                UpdateChartSeries(chartSeries);
            }
        }
        
        private void UpdateChartSeries(ObservableCollection<IRenderableSeriesViewModel> chartSeries)
        {
            Console.WriteLine($"Updating chart with {chartSeries.Count} series");
            
            // Clear existing series
            ChartSurface.RenderableSeries.Clear();
            
            // Add all series from the ViewModel
            foreach (var seriesViewModel in chartSeries)
            {
                // Handle line series
                if (seriesViewModel is SciChart.Charting.Model.ChartSeries.LineRenderableSeriesViewModel lineViewModel)
                {
                    var lineSeries = new SciChart.Charting.Visuals.RenderableSeries.FastLineRenderableSeries
                    {
                        DataSeries = lineViewModel.DataSeries,
                        StrokeThickness = lineViewModel.StrokeThickness,
                        Stroke = lineViewModel.Stroke ?? System.Windows.Media.Colors.Blue
                    };
                    ChartSurface.RenderableSeries.Add(lineSeries);
                    Console.WriteLine($"Added line series to chart surface");
                }
            }
            
            Console.WriteLine($"Chart surface now has {ChartSurface.RenderableSeries.Count} series");
        }
    }
}