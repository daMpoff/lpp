using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Events;
using LiveCharts.Wpf;
using lpp.Commands;
using lpp.Models;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media;

namespace lpp.ViewModels
{
    public partial class ViewModel : INotifyPropertyChanged
    {
        private Axis axisX;
        private Axis axisY;
        private SeriesCollection seriesCollection;
        private CartesianChart chart;
        public RelayCommand<RangeChangedEventArgs> RangeChangedCommand { get; set; }

        public CartesianChart Chart
        {
            get { return chart; }
            set
            {
                chart = value;
                OnPropertyChanged(nameof(Chart));
            }
        }
        public SeriesCollection SeriesCollection
        {
            get => seriesCollection;
            set
            {
                seriesCollection = value;
                OnPropertyChanged(nameof(SeriesCollection));
            }
        }

        public Axis AxisX
        {
            get => axisX;
            set
            {
                axisX = value;
                OnPropertyChanged(nameof(AxisX));
            }
        }

        public Axis AxisY
        {
            get => axisY;
            set
            {
                axisY = value;
                OnPropertyChanged(nameof(AxisY));
            }
        }

        public ViewModel()
        {
            Initialize();
            RangeChangedCommand = new RelayCommand<RangeChangedEventArgs>
            {
                ExecuteDelegate = e =>
                {
                    List<LinearEquation> equations = new List<LinearEquation>
                    {
                        new LinearEquation(4, 9, 36, Sign.LessEqually),
                        new LinearEquation(2, 1, 11, Sign.LessEqually),
                    };

                    SeriesCollection.Clear();
                    AxisLine yAxis = new AxisLine("Y", 0, AxisY.MinValue, 0, AxisY.MaxValue);
                    AxisLine xAxis = new AxisLine("X", AxisX.MinValue, 0, AxisX.MaxValue, 0);
                    AxisLine yLessThan5 = new AxisLine("Y<5", AxisX.MinValue, 5, AxisX.MaxValue, 5);

                    // Добавление линий в коллекцию
                    SeriesCollection.Add(new LineSeries
                    {
                        Title = yAxis.Title,
                        Values = yAxis.Values,
                        LineSmoothness = 0,
                        PointGeometrySize = 0,
                    });
                    SeriesCollection.Add(new LineSeries
                    {
                        Title = xAxis.Title,
                        Values = xAxis.Values,
                        LineSmoothness = 0,
                        PointGeometrySize = 0,
                    });
                    SeriesCollection.Add(new LineSeries
                    {
                        Title = yLessThan5.Title,
                        Values = yLessThan5.Values,
                        LineSmoothness = 0,
                        PointGeometrySize = 0,
                    });

                    for (int i = 0; i < equations.Count; i++)
                    {
                        for (int j = i + 1; j < equations.Count; j++)
                        {
                            Point intersection = LinearEquation.FindIntersection(equations[i], equations[j]);

                            if (intersection != null && !double.IsNaN(intersection.X) && !double.IsNaN(intersection.Y))
                            {
                                SeriesCollection.Add(new ScatterSeries
                                {
                                    Title = $"Пересечение прямой {i + 1} и {j + 1}",
                                    Values = new ChartValues<ObservablePoint>
                                    {
                                        new ObservablePoint(intersection.X, intersection.Y)
                                    },
                                    Stroke = Brushes.Red,
                                    Fill = Brushes.Red,
                                });
                            }
                        }

                        Point intersectionWithXAxis = new Point(equations[i].X2, 0);
                        Point intersectionWithYAxis = new Point(0, equations[i].X1);

                        if (!double.IsNaN(intersectionWithXAxis.X) && !double.IsNaN(intersectionWithXAxis.Y))
                        {
                            SeriesCollection.Add(new ScatterSeries
                            {
                                Title = $"Пересечения с осями (Прямая {i + 1})",
                                Values = new ChartValues<ObservablePoint>
                                {
                                    new ObservablePoint(intersectionWithXAxis.X, intersectionWithXAxis.Y),
                                    new ObservablePoint(intersectionWithYAxis.X, intersectionWithYAxis.Y)
                                },
                                Stroke = Brushes.Blue,
                                Fill = Brushes.Blue,
                            });
                        }
                    }

                    foreach (var equation in equations)
                    {
                        ChartValues<ObservablePoint> borderPoints = FindBorderPoints(AxisX.MinValue, AxisX.MaxValue, equation.LineSlopeCoefficient.M, equation.LineSlopeCoefficient.B);
                        SeriesCollection.Add(new LineSeries
                        {
                            Title = $"Прямая {equations.IndexOf(equation) + 1}",
                            Values = borderPoints,
                            LineSmoothness = 0,
                            PointGeometrySize = 0
                        });
                    }
                }
            };
        }

        private void Initialize()
        {
            AxisX = new Axis
            {
                Title = "Ось X",
                MinValue = -20,
                MaxValue = 20,
                Separator = new Separator
                {
                    Step = 1
                }
            };
            AxisY = new Axis
            {
                Title = "Ось Y",
                MinValue = -20,
                MaxValue = 20,
                Separator = new Separator
                {
                    Step = 1
                },
            };
            SeriesCollection = new SeriesCollection();
        }

        private ChartValues<ObservablePoint> FindBorderPoints(double startX, double endX, double m, double b)
        {
            ChartValues<ObservablePoint> points = new ChartValues<ObservablePoint>();

            double startY = m * startX + b;
            double endY = m * endX + b;

            if (!double.IsNaN(startY) && !double.IsNaN(endY))
            {
                points.Add(new ObservablePoint(startX, startY));
                points.Add(new ObservablePoint(endX, endY));
            }

            return points;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}