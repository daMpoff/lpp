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

            // Инициализация команды для обработки события изменения диапазона
            RangeChangedCommand = new RelayCommand<RangeChangedEventArgs>
            {
                ExecuteDelegate = e =>
                {
                    SeriesCollection.Clear();

                    // Список уравнений
                    List<LinearEquation> equations = new List<LinearEquation>
                    {
                        new LinearEquation(4, 9, 36, Sign.LessEqually),
                        new LinearEquation(2, 1, 11, Sign.LessEqually),
                    };
                    List<LineConstraint> constraints = new List<LineConstraint>
                    {
                        new LineConstraint(null,5,Sign.LessEqually)//y<5
                    };

                    // Добавляем осевые линии
                    AddAxisLines();

                    AddConstraints(constraints);

                    // Добавляем пересечения прямых
                    AddIntersections(equations);

                    // Добавляем уравнения
                    AddEquationLines(equations);

                    AddIntersecionWithConstraint(equations, constraints);

                }
            };
        }
        // Метод для добавления осевых линий в SeriesCollection
        private void AddAxisLines()
        {
            AxisLine yAxis = new AxisLine("Y", 0, AxisY.MinValue, 0, AxisY.MaxValue);
            AxisLine xAxis = new AxisLine("X", AxisX.MinValue, 0, AxisX.MaxValue, 0);

            SeriesCollection.Add(CreateLineSeries(yAxis));
            SeriesCollection.Add(CreateLineSeries(xAxis));
        }

        // Метод для добавления точек пересечения прямых в SeriesCollection
        private void AddIntersections(List<LinearEquation> equations)
        {
            for (int i = 0; i < equations.Count; i++)
            {
                for (int j = i + 1; j < equations.Count; j++)
                {
                    Point intersection = LinearEquation.FindIntersection(equations[i], equations[j]);

                    if (intersection != null && !double.IsNaN(intersection.X) && !double.IsNaN(intersection.Y))
                    {
                        SeriesCollection.Add(CreateScatterSeries($"Пересечение прямой {i + 1} и {j + 1}", intersection, Brushes.Red));
                    }
                }

                // Пересечения с осями
                AddAxisIntersections(equations[i], i);
            }
        }
        private void AddIntersecionWithConstraint(List<LinearEquation> equations, List<LineConstraint> constraints)
        {
            for (int i = 0; i < equations.Count; i++)
            {
                for (int j = 0; j < constraints.Count; j++)
                {
                    Point intersection;
                    if (constraints[j].FixedX == null)
                    {
                        double x = (double)((equations[i].B - (equations[i].A1 * constraints[j].FixedY)) / equations[i].A2);
                        intersection = new Point(x, (double)constraints[j].FixedY);
                    }
                    else
                    {
                        double y = (double)((equations[i].B - (equations[i].A2 * constraints[j].FixedX)) / equations[i].A1);
                        intersection = new Point((double)constraints[j].FixedX, y);
                    }
                    if (intersection != null && !double.IsNaN(intersection.X) && !double.IsNaN(intersection.Y))
                    {
                        SeriesCollection.Add(CreateScatterSeries($"Пересечение прямой {i + 1} и ограничения {j + 1}", intersection, Brushes.Black));
                    }
                }
            }
        }
        // Метод для добавления пересечений с осями
        private void AddAxisIntersections(LinearEquation equation, int index)
        {
            Point intersectionWithXAxis = new Point(equation.X2, 0);
            Point intersectionWithYAxis = new Point(0, equation.X1);

            if (!double.IsNaN(intersectionWithXAxis.X) && !double.IsNaN(intersectionWithXAxis.Y))
            {
                SeriesCollection.Add(new ScatterSeries
                {
                    Title = $"Пересечения с осями (Прямая {index + 1})",
                    Values = new ChartValues<ObservablePoint>
                    {
                        new ObservablePoint(intersectionWithXAxis.X, intersectionWithXAxis.Y),
                        new ObservablePoint(intersectionWithYAxis.X, intersectionWithYAxis.Y),
                    },
                    Stroke = Brushes.Black,
                    Fill = Brushes.Black,
                });
            }
        }

        // Метод для добавления уравнений в SeriesCollection
        private void AddEquationLines(List<LinearEquation> equations)
        {
            foreach (var equation in equations)
            {
                ChartValues<ObservablePoint> borderPoints = FindBorderPoints(AxisX.MinValue, AxisX.MaxValue, equation.LineSlopeCoefficient.M, equation.LineSlopeCoefficient.B);
                SeriesCollection.Add(CreateLineSeries($"Прямая {equations.IndexOf(equation) + 1}", borderPoints));
            }
        }
        // Метод для добавления ограничений в SeriesCollection
        private void AddConstraints(List<LineConstraint> constraints)
        {
            foreach (var constraint in constraints)
            {
                if (constraint.FixedY.HasValue)
                {
                    ChartValues<ObservablePoint> borderPoints = FindBorderPoints(AxisX.MinValue, AxisX.MaxValue, 0, (double)constraint.FixedY);

                    LineSeries constraintLine = new LineSeries
                    {
                        Title = $"Ограничение Y {constraint.FixedY}",
                        Values = borderPoints,
                        Stroke = Brushes.Gray,
                        LineSmoothness = 0,
                        PointGeometrySize = 0,
                        StrokeDashArray = new DoubleCollection { 2, 2 } // Пунктирная линия
                    };

                    SeriesCollection.Add(constraintLine);
                }
                else if (constraint.FixedX.HasValue)
                {
                    // Ограничение по X
                    ScatterSeries constraintLine = new ScatterSeries
                    {
                        Title = $"Ограничение X {constraint.FixedX}",
                        Values = new ChartValues<ObservablePoint>
                {
                    new ObservablePoint((double)constraint.FixedX, AxisY.MinValue),
                    new ObservablePoint((double)constraint.FixedX, AxisY.MaxValue)
                },
                        Stroke = Brushes.Gray,
                        StrokeDashArray = new DoubleCollection { 2, 2 } // Пунктирная линия
                    };

                    SeriesCollection.Add(constraintLine);
                }
            }
        }

        // Метод для создания LineSeries
        private LineSeries CreateLineSeries(AxisLine axisLine)
        {
            return new LineSeries
            {
                Title = axisLine.Title,
                Values = axisLine.Values,
                LineSmoothness = 0,
                PointGeometrySize = 0
            };
        }

        // Перегруженный метод для создания LineSeries для уравнений
        private LineSeries CreateLineSeries(string title, ChartValues<ObservablePoint> values)
        {
            return new LineSeries
            {
                Title = title,
                Values = values,
                LineSmoothness = 0,
                PointGeometrySize = 0
            };
        }

        // Метод для создания ScatterSeries
        private ScatterSeries CreateScatterSeries(string title, Point intersection, Brush color)
        {
            return new ScatterSeries
            {
                Title = title,
                Values = new ChartValues<ObservablePoint> { new ObservablePoint(intersection.X, intersection.Y) },
                Stroke = color,
                Fill = color,
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