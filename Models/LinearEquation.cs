namespace lpp.Models
{
    public enum Sign
    {
        LessEqually,
        MoreEqually,
        Equally
    }

    public class LinearEquation
    {
        public double A1 { get; set; }
        public double A2 { get; set; }
        public double X1 { get; set; }
        public double X2 { get; set; }
        public Point PointOne { get; set; }
        public Point PointTwo { get; set; }

        public double B { get; set; }
        public Sign Sign { get; set; }
        public LineSlopeCoefficient LineSlopeCoefficient { get; set; }

        public LinearEquation(double a1, double a2, double b, Sign sign)
        {
            A1 = a1;
            A2 = a2;
            B = b;
            Sign = sign;
            FindPoint();
            LineSlopeCoefficient = new LineSlopeCoefficient(PointOne, PointTwo);
        }
        private void FindPoint()
        {
            // Проверка на вертикальные и горизонтальные линии
            if (A1 != 0)
            {
                X1 = B / A1;
                PointOne = new Point(0, X1); // Точка пересечения с осью Y
            }
            else
            {
                PointOne = null; // Прямая параллельна оси Y
            }

            if (A2 != 0)
            {
                X2 = B / A2;
                PointTwo = new Point(X2, 0); // Точка пересечения с осью X
            }
            else
            {
                PointTwo = null; // Прямая параллельна оси X
            }
        }
        public static Point FindIntersection(LinearEquation eq1, LinearEquation eq2)
        {
            // Определитель
            double determinant = eq1.A1 * eq2.A2 - eq2.A1 * eq1.A2;

            if (determinant == 0)
            {
                return null;
            }

            // Решение системы уравнений методом Крамера
            double x = (eq1.B * eq2.A2 - eq2.B * eq1.A2) / determinant;
            double y = (eq1.A1 * eq2.B - eq2.A1 * eq1.B) / determinant;

            return new Point(y, x);
        }
    }
}
