using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SciChart.Charting.Visuals;
using D2G.Iris.ML.ConfigUI.WPF.ViewModels;

namespace D2G.Iris.ML.ConfigUI.WPF.Views
{
    public partial class VisualisationView : UserControl
    {
        public VisualisationView()
        {
            SciChartSurface.SetRuntimeLicenseKey("dkBLL7cFRf2kjrIDBv5UnMIOlzcrRi44w3/vQzJbsvsQ1s/F7BAmoG368pM381psDnVdB7vAyW2uH1Hs9A9gN/9nJIL9mQ4RHgln" +
                                                  "e+3ozSqZ+p7gF3tYGzMoDzG6noqUxpROjhpJh4gLxOt0CJxnp4ppf2EnxjIK70IuYIJHWUxvL91WPMjCoWYNAkN8V0JBNC86KRpj9G" +
                                                  "U6C7d1AQIp7YRC5pR2DhlxQILJ095mWDYiewPQ2GO1G26CMfMWxmevohKyGZkQgtzJTynusv7E7b/NGnnlKrGlvGujlDoMuRiRH9XkA" +
                                                  "fGCQm4bWnixsuHX3fIs7lS4IRKL1AWSPVheYnyPjrhnQPswbTVUBBMeXwgv22mVAfABvu1fxFWBO+WHjcC57GLWww/dp8KfI3Pa6oXy" +
                                                  "lI+KvgaXKysk/g3uc7TpwiNhNNMbHVy80/GIrYvhO3qmc0Uobdb0k9xENdLyL5gRHWgEoKmkban7f4QCsDrIoh2klh38eoE=");
            InitializeComponent();
        }

        private void HistogramBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is HistogramViewModel histogram)
            {
                if (DataContext is VisualisationViewModel viewModel)
                {
                    viewModel.SelectHistogramCommand.Execute(histogram);
                }
            }
        }
    }
}
