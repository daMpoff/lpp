namespace lpp.Models
{
    public class LineSlopeCoefficient
    {
        public double M { get; set; }
        public double B { get; set; }
        public LineSlopeCoefficient(Point pointOne, Point pointTwo)
        {
            if ((pointOne is null) || (pointTwo is null))
            {
                M = 0;
            }
            else
            {
                M = (pointTwo.Y - pointOne.Y) / (pointTwo.X - pointOne.X);
            }
            if (pointOne is null)
            {
                B = 0;
            }
            else
            {
                B = pointOne.Y - M * pointOne.X;
            }
        }
    }
}
