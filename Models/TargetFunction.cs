namespace lpp.Models
{
    public class TargetFunction
    {
        public double C1 { get; set; }
        public double C2 { get; set; }
        public Target Target { get; set; }
        public TargetFunction(double c1, double c2, Target target)
        {
            C1 = c1;
            C2 = c2;
            Target = target;
        }
    }
}