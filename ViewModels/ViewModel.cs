﻿using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Events;
using LiveCharts.Wpf;
using lpp.Commands;
using lpp.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media;

namespace lpp.ViewModels
{
    public class ViewModel : INotifyPropertyChanged
    {
        private Axis axisX;
        private Axis axisY;
        private SeriesCollection seriesCollection;
        private TargetFunction targetFunction;
        public RelayCommand<RangeChangedEventArgs> RangeChangedCommand { get; set; }

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

        public TargetFunction TargetFunction
        {
            get => targetFunction;
            set
            {
                targetFunction = value;
                OnPropertyChanged(nameof(TargetFunction));
            }
        }

        /// <summary>
        /// Конструктор ViewModel.
        /// </summary>
        public ViewModel()
        {
            Initialize();
            // Список уравнений
            List<LinearEquation> equations = new List<LinearEquation>
            {
                new LinearEquation(4, 9, 36, Sign.LessEqually),
                new LinearEquation(2, 1, 11, Sign.LessEqually),
            };
            // Ограничения
            List<LineConstraint> constraints = new List<LineConstraint>
            {
                new LineConstraint(null, 5, Sign.LessEqually)
            };
            // Целевая функция
            TargetFunction = new TargetFunction(3, 4, Target.Max);

            // Инициализация команды для обработки события изменения диапазона
            RangeChangedCommand = new RelayCommand<RangeChangedEventArgs>
            {
                ExecuteDelegate = e =>
                {
                    SeriesCollection.Clear();

                    // Добавляем осевые линии
                    AddAxisLines();

                    // Добавляем ограничения
                    AddConstraints(constraints);

                    // Добавляем пересечения прямых
                    AddIntersections(equations);

                    // Добавляем уравнения
                    AddEquationLines(equations);

                    // Добавляем целевую функцию
                    AddTargetFunction();

                    // Добавляем точки пересечений
                    AddIntersectionWithConstraint(equations, constraints);

                    // Добавляем точку 0,0 на оси координат
                    AddZeroPoint();
                }
            };
        }

        /// <summary>
        /// Метод для добавления точки пересечения осей координатной плоскости.
        /// </summary>
        private void AddZeroPoint()
        {
            SeriesCollection.Add(CreateScatterSeries($"Точка пересечения осей", new Point(0, 0), Brushes.Black));
        }

        /// <summary>
        /// Метод для добавления вектора на плоскость.
        /// </summary>
        private void AddArrow()
        {
            // Задаем начальную и конечную точки стрелки
            Point start = new Point(0, 0);
            Point end = new Point(TargetFunction.C2, TargetFunction.C1);

            // Создание линии стрелки
            LineSeries arrowLine = new LineSeries
            {
                Title = null, // Удаляем заголовок линии, чтобы она не отображалась, при графическом отображении
                Values = new ChartValues<ObservablePoint>
                {
                    new ObservablePoint(start.X, start.Y),
                    new ObservablePoint(end.X, end.Y)
                },
                Stroke = Brushes.Black, // Цвет линии
                Fill = Brushes.Transparent,
                LineSmoothness = 0, // Прямая линия
                PointGeometrySize = 0
            };

            // Вычисляем направление для концов стрелки
            double arrowLength = 0.5; // Длина концов стрелки
            double
                angle = Math.Atan2(end.Y - start.Y,
                    end.X - start.X); // Угол наклона линии, тангенс которого равен отношению двух указанных чисел.

            // Создание концов стрелки
            Point arrowEnd1 = new Point(
                end.X - arrowLength * Math.Cos(angle - Math.PI / 6), // -30 градусов
                end.Y - arrowLength * Math.Sin(angle - Math.PI / 6)
            );

            Point arrowEnd2 = new Point(
                end.X - arrowLength * Math.Cos(angle + Math.PI / 6), // +30 градусов
                end.Y - arrowLength * Math.Sin(angle + Math.PI / 6)
            );

            // Линия для одного конца стрелки
            LineSeries arrowHeadLine1 = new LineSeries
            {
                Title = null,
                Values = new ChartValues<ObservablePoint>
                {
                    new ObservablePoint(end.X, end.Y),
                    new ObservablePoint(arrowEnd1.X, arrowEnd1.Y)
                },
                Stroke = Brushes.Black,
                Fill = Brushes.Transparent,
                LineSmoothness = 0,
                PointGeometrySize = 0
            };

            // Линия для другого конца стрелки
            LineSeries arrowHeadLine2 = new LineSeries
            {
                Title = null,
                Values = new ChartValues<ObservablePoint>
                {
                    new ObservablePoint(end.X, end.Y),
                    new ObservablePoint(arrowEnd2.X, arrowEnd2.Y)
                },
                Stroke = Brushes.Black,
                Fill = Brushes.Transparent,
                LineSmoothness = 0,
                PointGeometrySize = 0
            };

            // Добавляем линии стрелки в коллекцию
            SeriesCollection.Add(arrowLine);
            SeriesCollection.Add(arrowHeadLine1);
            SeriesCollection.Add(arrowHeadLine2);
        }

        /// <summary>
        /// Метод для добавления целевой функции.
        /// </summary>
        private void AddTargetFunction()
        {
            // Начальная и конечная точки целевой функции
            Point start = new Point(0, 0);
            Point end = new Point(TargetFunction.C2, TargetFunction.C1);

            // Создание линии для целевой функции
            LineSeries targetFunctionLine = new LineSeries
            {
                Title = "Целевая функция",
                Values = new ChartValues<ObservablePoint>
                {
                    new ObservablePoint(start.X, start.Y),
                    new ObservablePoint(end.X, end.Y)
                },
                Stroke = Brushes.Black, // Цвет линии
                Fill = Brushes.Transparent, // Оставляем заливку прозрачной
                LineSmoothness = 0, // Прямая линия
                PointGeometrySize = 0,
                StrokeDashArray = new DoubleCollection { 3, 2 } // Пунктирная линия
            };

            // Создание вертикального перпендикуляра (из точки end на ось X)
            LineSeries verticalLine = new LineSeries
            {
                Title = "Перпендикуляр на ось X",
                Values = new ChartValues<ObservablePoint>
                {
                    new ObservablePoint(end.X, end.Y),
                    new ObservablePoint(end.X, 0) // Проекция на ось X
                },
                Stroke = Brushes.Gray, // Цвет линии
                Fill = Brushes.Transparent,
                LineSmoothness = 0, // Прямая линия
                PointGeometrySize = 0,
                StrokeDashArray = new DoubleCollection { 2, 2 } // Пунктирная линия
            };

            // Создание горизонтального перпендикуляра (из точки end на ось Y)
            LineSeries horizontalLine = new LineSeries
            {
                Title = "Перпендикуляр на ось Y",
                Values = new ChartValues<ObservablePoint>
                {
                    new ObservablePoint(end.X, end.Y),
                    new ObservablePoint(0, end.Y) // Проекция на ось Y
                },
                Stroke = Brushes.Gray, // Цвет линии
                Fill = Brushes.Transparent,
                LineSmoothness = 0, // Прямая линия
                PointGeometrySize = 0,
                StrokeDashArray = new DoubleCollection { 2, 2 } // Пунктирная линия
            };

            // Добавляем линии целевой функции и перпендикуляры в коллекцию
            SeriesCollection.Add(targetFunctionLine);
            SeriesCollection.Add(verticalLine);
            SeriesCollection.Add(horizontalLine);
            AddArrow();
        }

        /// <summary>
        /// Метод для добавления осевых линий в SeriesCollection.
        /// </summary>
        private void AddAxisLines()
        {
            AxisLine yAxis = new AxisLine("Y", 0, AxisY.MinValue, 0, AxisY.MaxValue);
            AxisLine xAxis = new AxisLine("X", AxisX.MinValue, 0, AxisX.MaxValue, 0);

            SeriesCollection.Add(CreateLineSeries(yAxis));
            SeriesCollection.Add(CreateLineSeries(xAxis));
        }

        /// <summary>
        /// Метод для добавления точек пересечения прямых в SeriesCollection.
        /// </summary>
        /// <param name="equations">Список уравнений прямых.</param>
        private void AddIntersections(List<LinearEquation> equations)
        {
            for (int i = 0; i < equations.Count; i++)
            {
                for (int j = i + 1; j < equations.Count; j++)
                {
                    Point intersection = LinearEquation.FindIntersection(equations[i], equations[j]);

                    if (intersection != null && !double.IsNaN(intersection.X) && !double.IsNaN(intersection.Y))
                    {
                        SeriesCollection.Add(CreateScatterSeries($"Пересечение прямой {i + 1} и {j + 1}", intersection,
                            Brushes.Red));
                    }
                }

                // Пересечения с осями
                AddAxisIntersections(equations[i], i);
            }
        }

        /// <summary>
        /// Метод для добавления точек пересечения уравнений с ограничениями.
        /// </summary>
        /// <param name="equations">Список уравнений.</param>
        /// <param name="constraints">Список ограничений.</param>
        private void AddIntersectionWithConstraint(List<LinearEquation> equations, List<LineConstraint> constraints)
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
                        if (intersection != null && !double.IsNaN(intersection.X) && !double.IsNaN(intersection.Y))
                        {
                            SeriesCollection.Add(CreateScatterSeries(
                                $"Пересечение прямой {i + 1} и ограничения {j + 1}", intersection, Brushes.Black));
                            SeriesCollection.Add(CreateScatterSeries($"Пересечение ограничения {j + 1} c осью",
                                new Point(0, (double)constraints[j].FixedY), Brushes.Black));
                        }
                    }
                    else
                    {
                        double y = (double)((equations[i].B - (equations[i].A2 * constraints[j].FixedX)) / equations[i].A1);
                        intersection = new Point((double)constraints[j].FixedX, y);
                        if (intersection != null && !double.IsNaN(intersection.X) && !double.IsNaN(intersection.Y))
                        {
                            SeriesCollection.Add(CreateScatterSeries(
                                $"Пересечение прямой {i + 1} и ограничения {j + 1}", intersection, Brushes.Black));
                            SeriesCollection.Add(CreateScatterSeries($"Пересечение ограничения {j + 1} c осью",
                                new Point((double)constraints[j].FixedX, 0), Brushes.Black));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Метод для добавления пересечений с осями.
        /// </summary>
        /// <param name="equation">Уравнение прямой.</param>
        /// <param name="index">Индекс прямой.</param>
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

        /// <summary>
        /// Метод для добавления уравнений в SeriesCollection.
        /// </summary>
        /// <param name="equations">Список уравнений.</param>
        private void AddEquationLines(List<LinearEquation> equations)
        {
            foreach (var equation in equations)
            {
                ChartValues<ObservablePoint> borderPoints = FindBorderPoints(AxisX.MinValue, AxisX.MaxValue,
                    equation.LineSlopeCoefficient.M, equation.LineSlopeCoefficient.B);
                SeriesCollection.Add(CreateLineSeries($"Прямая {equations.IndexOf(equation) + 1}", borderPoints));
            }
        }

        /// <summary>
        /// Метод для добавления ограничений в SeriesCollection.
        /// </summary>
        /// <param name="constraints">Список ограничений.</param>
        private void AddConstraints(List<LineConstraint> constraints)
        {
            foreach (var constraint in constraints)
            {
                if (constraint.FixedY.HasValue)
                {
                    ChartValues<ObservablePoint> borderPoints =
                        FindBorderPoints(AxisX.MinValue, AxisX.MaxValue, 0, (double)constraint.FixedY);

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

        /// <summary>
        /// Метод для создания LineSeries на основе объекта AxisLine.
        /// </summary>
        /// <param name="axisLine">Объект AxisLine, содержащий данные для создания LineSeries.</param>
        /// <returns>Объект LineSeries.</returns>
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

        /// <summary>
        /// Перегруженный метод для создания LineSeries на основе заголовка и коллекции точек.
        /// </summary>
        /// <param name="title">Заголовок серии.</param>
        /// <param name="values">Коллекция точек для серии.</param>
        /// <returns>Объект LineSeries.</returns>
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

        /// <summary>
        /// Метод для создания ScatterSeries с заданным заголовком, точкой пересечения и цветом.
        /// </summary>
        /// <param name="title">Заголовок серии.</param>
        /// <param name="intersection">Точка пересечения.</param>
        /// <param name="color">Цвет серии.</param>
        /// <returns>Объект ScatterSeries.</returns>
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

        /// <summary>
        /// Метод инициализации графика и установки размеров клеток.
        /// </summary>
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

        /// <summary>
        /// Метод для нахождения граничных точек на прямой линии.
        /// </summary>
        /// <param name="startX">Начальная координата X.</param>
        /// <param name="endX">Конечная координата X.</param>
        /// <param name="m">Наклон линии (угловой коэффициент).</param>
        /// <param name="b">Точка пересечения линии с осью Y (свободный член).</param>
        /// <returns>Коллекция граничных точек.</returns>
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