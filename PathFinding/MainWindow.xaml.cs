using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PathFinding
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<Spot> Spots = new List<Spot>();
        double size = 30;
        Spot selectedSpot = null;
        Spot targetSpot = null;
        BackgroundWorker worker = null;
        bool work = false;
        public MainWindow()
        {
            InitializeComponent();
        }
        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
            {
                if (e.OriginalSource.GetType() == typeof(Ellipse)) TargetEllipse((Ellipse)e.OriginalSource);
                else UntargetEllipse();
            }
            else
            {
                if (e.OriginalSource.GetType() == typeof(Ellipse))
                {
                    MarkEllipse((Ellipse)e.OriginalSource);
                }
                else if (e.OriginalSource.GetType() != typeof(Line))
                {
                    DrawEllipse((Canvas)sender);
                }
                else
                {
                    UnmarkElipse();
                }
            }
        }
        private void UntargetEllipse()
        {
            if (targetSpot != null)
            {
                targetSpot.Ellipse.Fill = Brushes.DarkGray;
                targetSpot = null;
            }
        }
        private void TargetEllipse(Ellipse e)
        {
            if (targetSpot != null)
            {
                foreach (Road r in targetSpot.Roads)
                {
                    r.Line.Stroke = Brushes.Black;
                    r.Line.StrokeThickness = 1;
                }
            }
            UntargetEllipse();
            targetSpot = Spots.First(v => v.Ellipse == e);
            targetSpot.Ellipse.Fill = Brushes.Red;
            work = false;
        }
        private void UnmarkElipse()
        {
            if (selectedSpot == null) return;
            selectedSpot.Ellipse.Stroke = Brushes.Transparent;
            selectedSpot = null;
        }
        private void MarkEllipse(Ellipse e)
        {
            Spot s = Spots.Find(v => v.Ellipse.GetHashCode() == e.GetHashCode());
            if (selectedSpot != null)
            {
                DrawLine(s, selectedSpot);
                UnmarkElipse();
                return;
            }
            else if (selectedSpot != null && selectedSpot.Ellipse == e)
            {
                UnmarkElipse();
                return;
            }
            selectedSpot = s;
            selectedSpot.Ellipse.Stroke = Brushes.Black;
        }
        private void DrawEllipse(Canvas c)
        {
            if (selectedSpot != null)
            {
                selectedSpot.Ellipse.Stroke = Brushes.Transparent;
            }
            selectedSpot = null;
            Point mp = Mouse.GetPosition(c);
            Ellipse ellipse = new Ellipse();
            ellipse.Height = size;
            ellipse.Width = size;
            ellipse.Fill = Brushes.DarkGray;
            Canvas.SetLeft(ellipse, mp.X - (size / 2));
            Canvas.SetTop(ellipse, mp.Y - (size / 2));
            c.Children.Add(ellipse);
            Spots.Add(new Spot() { Ellipse = ellipse, Size = size });
            SortChildren(ref c);
        }
        private void DrawLine(Spot e, Spot s)
        {
            UIElement container = VisualTreeHelper.GetParent(e.Ellipse) as UIElement;
            Point r = e.Ellipse.TranslatePoint(new Point(0, 0), container);
            Line line = new Line();
            line.X1 = r.X + (size / 2);
            line.Y1 = r.Y + (size / 2);

            container = VisualTreeHelper.GetParent(s.Ellipse) as UIElement;
            r = s.Ellipse.TranslatePoint(new Point(0, 0), container);
            line.X2 = r.X + (size / 2);
            line.Y2 = r.Y + (size / 2);

            line.StrokeThickness = 1;
            line.Stroke = Brushes.Black;

            Canvas c = e.Ellipse.Parent as Canvas;
            foreach (var i in c.Children)
            {
                if (i.GetType() == typeof(Line))
                {
                    Line t = i as Line;
                    if (t.X1 == line.X1 &&
                        t.Y1 == line.Y1 &&
                        t.X2 == line.X2 &&
                        t.Y2 == line.Y2) return;
                    if (t.X1 == line.X2 &&
                        t.Y1 == line.Y2 &&
                        t.X2 == line.X1 &&
                        t.Y2 == line.Y1) return;
                }
            }

            e.Roads.Add(new Road() { Line = line });
            e.Roads[e.Roads.Count - 1].CalcLength();


            s.Roads.Add(new Road() { Line = line });
            s.Roads[s.Roads.Count - 1].CalcLength();

            c.Children.Add(line);
            SortChildren(ref c);
        }
        private void SortChildren(ref Canvas p)
        {
            List<UIElement> list = new List<UIElement>();
            foreach (UIElement child in p.Children) if (child.GetType() == typeof(Line)) list.Add(child);
            foreach (UIElement child in p.Children) if (child.GetType() == typeof(Ellipse)) list.Add(child);
            p.Children.Clear();
            foreach (UIElement child in list) p.Children.Add(child);
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ThickenLines();
        }
        private void ThickenLines()
        {
            if (targetSpot != null)
            {
                work = true;
                SetupWorker();
                if (AllLineThick(targetSpot)) foreach (Road r in targetSpot.Roads) r.Line.StrokeThickness = 1;
                else foreach (Road r in targetSpot.Roads) r.Line.StrokeThickness = 3;
                if (!worker.IsBusy) worker.RunWorkerAsync();
            }
        }
        private void AlternateColors(object sender, DoWorkEventArgs e)
        {
            while (work)
            {
                foreach (Road r in targetSpot.Roads)
                {
                    r.Line.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate ()
                    {
                        r.Line.Stroke = Brushes.Red;
                    }));
                    Thread.Sleep(500);
                    if (!work) return;
                    r.Line.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate ()
                    {
                        r.Line.Stroke = Brushes.Black;
                    }));
                }
            }
        }
        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                return;
            }
            else
            {
                return;
            }
        }
        private bool AllLineThick(Spot s)
        {
            foreach (Road r in s.Roads)
            {
                if (r.Line.StrokeThickness != 3) return false;
            }
            return true;
        }
        private void SetupWorker()
        {
            worker = new BackgroundWorker();
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += new DoWorkEventHandler(AlternateColors);
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
        }
        private void Path_Click(object sender, RoutedEventArgs e)
        {
            if (targetSpot != null && selectedSpot != null)
            {
                List<Spot> path = new List<Spot>(Dijkstra(Spots));
                foreach (Spot s in path)
                {
                    s.Ellipse.Fill = Brushes.Green;
                }
            }
        }
        private Spot FindNodeAtEnd(Spot s, Line l)
        {
            double x, y;
            if (s.X == l.X1)
            {
                x = l.X2;
                y = l.Y2;
            }
            else
            {
                x = l.X1;
                y = l.Y1;
            }
            return Spots.Find(v => v.X == x && v.Y == y);
        }
        private List<Spot> Dijkstra(List<Spot> s)
        {
            List<Spot> path = new List<Spot>();
            path.Add(selectedSpot);

            Spot currentSpot = selectedSpot;
            while (true)
            {
                List<Spot> allNeighbors = new List<Spot>();
                foreach (Road road in currentSpot.Roads)
                {
                    allNeighbors.Add(FindNodeAtEnd(currentSpot, road.Line));
                }

                IEnumerable<Spot> neighbors = from neighbor in allNeighbors
                                              where !path.Contains(neighbor)
                                              select neighbor;
                foreach (Spot temp in neighbors) if (temp == null) break;
                if (neighbors.Count() == 0) break;
                if (neighbors.Contains(targetSpot))
                {
                    path.Add(targetSpot);
                    break;
                }

                Spot nearestSpot = FindNearestSpot(neighbors, targetSpot);
                path.Add(nearestSpot);
                currentSpot = nearestSpot;
            }
            return path;
        }
        public Spot FindNearestSpot(IEnumerable<Spot> spots, Spot targetSpot)
        {
            double shortestDistance = -1;
            Spot nearestSpot = null;

            int index = 0;
            foreach (Spot spot in spots)
            {
                double distance = FindDistance(spot, targetSpot);
                if (index == 0)
                {
                    shortestDistance = distance;
                    nearestSpot = spot;
                }
                else if (shortestDistance > distance)
                {
                    shortestDistance = distance;
                    nearestSpot = spot;
                }
                index += 1;
            }

            return (nearestSpot);
        }
        public static double FindDistance(Spot spot1, Spot spot2)
        {
            double x1 = spot1.X, y1 = spot1.Y;
            double x2 = spot2.X, y2 = spot2.Y;

            double distance = Math.Sqrt(Math.Pow(x2 - x1, 2.0) + Math.Pow(y2 - y1, 2.0));
            return (distance);
        }
        private void ClearAll_Click(object sender, RoutedEventArgs e)
        {
            targetSpot = null;
            selectedSpot = null;
            Spots.Clear();
            Button b = sender as Button;
            StackPanel s = b.Parent as StackPanel;
            Grid g = s.Parent as Grid;
            Canvas c = null;
            foreach (UIElement uie in g.Children)
            {
                if (uie.GetType() == typeof(Canvas))
                {
                    c = uie as Canvas;
                    break;
                }
            }
            c.Children.Clear();
        }
        private void ResetAll()
        {
            foreach (Spot s in Spots)
            {
                s.Ellipse.Stroke = Brushes.Transparent;
                s.Ellipse.Fill = Brushes.DarkGray;
                foreach (Road r in s.Roads)
                {
                    r.Line.Stroke = Brushes.Black;
                    r.Line.StrokeThickness = 1;
                }
                selectedSpot = null;
                targetSpot = null;
            }
        }
        private void ClearPath_Click(object sender, RoutedEventArgs e)
        {
            ResetAll();
        }
        private void DeleteSpot_Click(object sender, RoutedEventArgs e)
        {
            if (selectedSpot != null)
            {
                Button b = sender as Button;
                StackPanel s = b.Parent as StackPanel;
                Grid g = s.Parent as Grid;
                Canvas c = null;
                foreach (UIElement uie in g.Children)
                {
                    if (uie.GetType() == typeof(Canvas))
                    {
                        c = uie as Canvas;
                        break;
                    }
                }
                foreach (Road r in selectedSpot.Roads)
                {
                    Spot otherSpot = FindNodeAtEnd(selectedSpot, r.Line);
                    int index = -1;
                    foreach(Road or in otherSpot.Roads)
                    {
                        if (or.Line.X1 == r.Line.X1 &&
                            or.Line.X2 == r.Line.X2 &&
                            or.Line.Y1 == r.Line.Y1 &&
                            or.Line.Y2 == r.Line.Y2) index = otherSpot.Roads.IndexOf(or);
                    }
                    if(index != -1) otherSpot.Roads.RemoveAt(index);
                    c.Children.Remove(r.Line);
                }
                selectedSpot.Roads.Clear();
                c.Children.Remove(selectedSpot.Ellipse);
                Spots.Remove(selectedSpot);
                selectedSpot = null;
            }
        }
    }
}
