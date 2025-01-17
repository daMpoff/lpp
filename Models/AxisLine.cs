using LiveCharts;
using LiveCharts.Defaults;

namespace lpp.Models
{
    public class AxisLine
    {
        public string Title { get; set; }
        public ChartValues<ObservablePoint> Values { get; set; }
        public double Slope { get; set; }
        public double Intercept { get; set; }

        public AxisLine(string title, double x1, double y1, double x2, double y2)
        {
            Title = title;
            Values = new ChartValues<ObservablePoint>
        {
            new ObservablePoint(x1, y1),
            new ObservablePoint(x2, y2)
        };

            // Вычисляем коэффициенты для уравнения линии y = mx + b
            Slope = (y2 - y1) / (x2 - x1);
            Intercept = y1 - Slope * x1;
        }
    }
}
