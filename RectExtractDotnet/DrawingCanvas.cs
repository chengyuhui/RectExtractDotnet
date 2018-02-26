using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using CvPoint = OpenCvSharp.Point;
using System.Reactive.Linq;

namespace RectExtractDotnet
{
    public class DrawingCanvas : Canvas, INotifyPropertyChanged
    {
        enum DrawType
        {
            Target = 1,
            Exclude = 2,
        }
        private Mat image;
        private MatOfInt marker;
        private Image image_elem;

        private Rectangle result_rect_elem;
        private Rect? result;

        private Polyline line;
        private DrawType draw_type = DrawType.Target;
        private bool processing = false;

        private static SolidColorBrush TARGET = new SolidColorBrush(Color.FromRgb(255, 0, 0)); // Red
        private static SolidColorBrush EXCLUDE = new SolidColorBrush(Color.FromRgb(0, 255, 0)); // Green

        private static int THICKNESS = 4;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }


        public DrawingCanvas()
        {
            image_elem = new Image();
            result_rect_elem = new Rectangle
            {
                StrokeDashArray = new DoubleCollection(new double[] { 1 }),
                Stroke = TARGET,
                StrokeThickness = 1
            };

            Children.Add(result_rect_elem);
            Children.Add(image_elem);

            var up = Observable.FromEventPattern<MouseEventArgs>(this, "MouseUp");
            var leave = Observable.FromEventPattern<MouseEventArgs>(this, "MouseLeave");
            var merged = up.Merge(leave);
            merged
                .ObserveOnDispatcher().Where(e => OnDrawEnd())
                .Sample(TimeSpan.FromSeconds(2))
                .ObserveOnDispatcher().Subscribe(e => RunWatershed());
        }

        public void Reset()
        {
            Children.Clear();
            Children.Add(image_elem);
            Children.Add(result_rect_elem);
            result_rect_elem.Width = 0;
            result_rect_elem.Height = 0;
            marker?.SetTo(0);
            marker = new MatOfInt(image.Size(), 0);
            Result = null;
        }

        public Mat Image
        {
            get => image; set
            {
                image?.Dispose();
                image = value;
                Height = image.Height;
                Width = image.Width;
                image_elem.Source = image.ToBitmapSource();

                Reset();
            }
        }

        public bool Processing
        {
            get => processing; set
            {
                processing = value;
                OnPropertyChanged("Processing");
            }
        }

        public Rect? Result
        {
            get => result; set
            {
                result = value;
                if (result.HasValue)
                {
                    var r = result.Value;
                    SetLeft(result_rect_elem, r.Left);
                    SetTop(result_rect_elem, r.Top);
                    result_rect_elem.Width = r.Width;
                    result_rect_elem.Height = r.Height;
                    OnPropertyChanged("Result");
                }
            }
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            line = new Polyline();

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                line.Stroke = TARGET;
                draw_type = DrawType.Target;
            }
            else if (e.RightButton == MouseButtonState.Pressed)
            {
                line.Stroke = EXCLUDE;
                draw_type = DrawType.Exclude;
            }

            line.StrokeThickness = THICKNESS;
            Children.Add(line);
            line.Points.Add(e.GetPosition(this));
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            line?.Points.Add(e.GetPosition(this));
            base.OnMouseMove(e);
        }

        async void RunWatershed()
        {
            Processing = true;
            Result = await BackgroundWorker.WatershedOn(image, marker);
            Processing = false;
        }

        protected bool OnDrawEnd()
        {
            if (line == null)
            {
                return false;
            }
            var points = line.Points;
            var pairs = points.Zip(points.Skip(1), (a, b) => Tuple.Create((int)a.X, (int)a.Y, (int)b.X, (int)b.Y));

            line = null;

            foreach (var pair in pairs)
            {
                marker.Line(pair.Item1, pair.Item2, pair.Item3, pair.Item4, (int)draw_type, THICKNESS);
            }
            return true;
        }

        public Mat Crop() => Result.HasValue ? new Mat(image, Result.Value) : null;
    }
}
