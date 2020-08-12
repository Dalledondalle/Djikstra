using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Shapes;

namespace PathFinding
{
    public class Road
    {
        public double X1 { get { return Line.X1; } }
        public double Y1 { get { return Line.Y1; } }
        public double X2 { get { return Line.X2; } }
        public double Y2 { get { return Line.X2; } }
        public Line Line { get; set; }
        public double Length { get; set; }
        public void CalcLength()
        {
            Length = Math.Sqrt(((X2 - X1) * (X2 - X1)) + ((X2 - X1) * (X2 - X1)));
        }
    }
}
