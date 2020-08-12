using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Threading;

namespace PathFinding
{
    public class Spot
    {
        public double Size { get; set; }
        public double X 
        {
            get
            {
                UIElement container = VisualTreeHelper.GetParent(Ellipse) as UIElement;
                Point r = Ellipse.TranslatePoint(new Point(0, 0), container);
                return r.X+(Size/2);
            }
        }
        public double Y {
            get
            {
                UIElement container = VisualTreeHelper.GetParent(Ellipse) as UIElement;
                Point r = Ellipse.TranslatePoint(new Point(0, 0), container);
                return r.Y+(Size/2);
            }
        }
        public Ellipse Ellipse { get; set; }
        public List<Road> Roads { get; set; } = new List<Road>();
        public double Distance { get; set; }
        public bool SptSet { get; set; }
    }
}
