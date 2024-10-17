namespace lpp.Models
{
    public class LineConstraint
    {
        public double? FixedX { get; set; } // Ограничение по X
        public double? FixedY { get; set; } // Ограничение по Y
        public Sign Sign { get; set; }
        public LineConstraint(double? fixedX = null, double? fixedY = null, Sign sign = Sign.Equally)
        {
            FixedX = fixedX;
            FixedY = fixedY;
            Sign = sign;
        }
    }
}