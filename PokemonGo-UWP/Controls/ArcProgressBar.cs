using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace PokemonGo_UWP.Controls
{
    public class ArcProgressBar : Canvas
    {
        public ArcProgressBar()
        {
            this.Loaded += OnLoaded;
        }

        #region Properties

        public static readonly DependencyProperty RadiusProperty =
            DependencyProperty.Register(nameof(Radius), typeof(int), typeof(ArcProgressBar),
                new PropertyMetadata(100));

        public static readonly DependencyProperty StrokeThicknessProperty =
            DependencyProperty.Register(nameof(StrokeThickness), typeof(int), typeof(ArcProgressBar),
                new PropertyMetadata(5));

        public static readonly DependencyProperty ValueEndCapDiameterProperty =
            DependencyProperty.Register(nameof(ValueEndCapDiameter), typeof(int), typeof(ArcProgressBar),
                new PropertyMetadata(5));

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(int), typeof(ArcProgressBar),
                new PropertyMetadata(0, OnValuePropertyChanged));

        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register(nameof(Minimum), typeof(int), typeof(ArcProgressBar),
                new PropertyMetadata(0));

        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register(nameof(Maximum), typeof(int), typeof(ArcProgressBar),
                new PropertyMetadata(100, OnMaximumPropertyChanged));

        public static readonly DependencyProperty EmptyColorProperty =
            DependencyProperty.Register(nameof(EmptyColor), typeof(Color), typeof(ArcProgressBar),
                new PropertyMetadata(Color.FromArgb(0x00, 0x00, 0x00, 0x00)));

        public static readonly DependencyProperty FilledColorProperty =
            DependencyProperty.Register(nameof(FilledColor), typeof(Color), typeof(ArcProgressBar),
                new PropertyMetadata(Color.FromArgb(0x00, 0x00, 0x00, 0x00)));

        public int Radius
        {
            get { return (int)GetValue(RadiusProperty); }
            set { SetValue(RadiusProperty, value); }
        }

        public int StrokeThickness
        {
            get { return (int)GetValue(StrokeThicknessProperty); }
            set { SetValue(StrokeThicknessProperty, value); }
        }

        public int ValueEndCapDiameter
        {
            get { return (int)GetValue(ValueEndCapDiameterProperty); }
            set { SetValue(ValueEndCapDiameterProperty, value); }
        }

        public int Value
        {
            get { return (int)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public int Minimum
        {
            get { return (int)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        public int Maximum
        {
            get { return (int)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        public Color EmptyColor
        {
            get { return (Color)GetValue(EmptyColorProperty); }
            set { SetValue(EmptyColorProperty, value); }
        }

        public Color FilledColor
        {
            get { return (Color)GetValue(FilledColorProperty); }
            set { SetValue(FilledColorProperty, value); }
        }

        #endregion

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            SetControlSize();
            Draw();
        }

        private static void OnValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as ArcProgressBar;
            control.SetControlSize();
            control.Draw();
        }

        private static void OnMaximumPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

            var control = d as ArcProgressBar;
            control.SetControlSize();
            control.Draw();
        }

        private void Draw()
        {
            Children.Clear();

            Path radialBackgroundStrip = GetCircleSegment(GetCenterPoint(), Radius, 180);
            radialBackgroundStrip.Stroke = new SolidColorBrush(EmptyColor);
            radialBackgroundStrip.StrokeStartLineCap = PenLineCap.Round;
            radialBackgroundStrip.StrokeEndLineCap = PenLineCap.Round;
            radialBackgroundStrip.StrokeThickness = StrokeThickness;

            Children.Add(radialBackgroundStrip);

            Path radialValueStrip = GetCircleSegment(GetCenterPoint(), Radius, GetAngle());
            radialValueStrip.Stroke = new SolidColorBrush(FilledColor);
            radialValueStrip.StrokeStartLineCap = PenLineCap.Round;
            radialValueStrip.StrokeThickness = StrokeThickness;

            Children.Add(radialValueStrip);

            Ellipse valueEndpoint = new Ellipse();
            valueEndpoint.Fill = new SolidColorBrush(FilledColor);
            valueEndpoint.Height = ValueEndCapDiameter;
            valueEndpoint.Width = ValueEndCapDiameter;
            Point valuePointCenter = ScaleUnitCirclePoint(GetCenterPoint(), Radius, GetAngle());
            SetLeft(valueEndpoint, valuePointCenter.X - (ValueEndCapDiameter / 2));
            SetTop(valueEndpoint, valuePointCenter.Y - (ValueEndCapDiameter / 2));

            Children.Add(valueEndpoint);
        }

        private void SetControlSize()
        {
            Width = Radius * 2 + Math.Max(StrokeThickness, ValueEndCapDiameter);
            Height = Radius + Math.Max(StrokeThickness, ValueEndCapDiameter);
        }

        private Point GetCenterPoint()
        {
            return new Point(Radius + (StrokeThickness / 2), Radius + (StrokeThickness / 2));
        }

        private double GetAngle()
        {
            double angle = (double)(Math.Min(Math.Max(Value, Minimum), Maximum) - Minimum) / (double)(Maximum - Minimum) * 180;

            if (angle >= 180)
            {
                angle = 179.999;
            }

            return angle;
        }

        private const double RADIANS = Math.PI / 180;

        private Path GetCircleSegment(Point centerPoint, double radius, double angle)
        {
            var path = new Path();
            var pathGeometry = new PathGeometry();

            var circleStart = new Point(centerPoint.X - radius, centerPoint.Y);

            var arcSegment = new ArcSegment
            {
                IsLargeArc = false,
                Point = ScaleUnitCirclePoint(centerPoint, radius, angle),
                Size = new Size(radius, radius),
                SweepDirection = SweepDirection.Clockwise,
                RotationAngle = -90
            };

            var pathFigure = new PathFigure
            {
                StartPoint = circleStart,
                IsClosed = false
            };

            pathFigure.Segments.Add(arcSegment);
            pathGeometry.Figures.Add(pathFigure);

            path.Data = pathGeometry;
            return path;
        }

        private Point ScaleUnitCirclePoint(Point origin, double radius, double angle)
        {
            return new Point(origin.X - Math.Cos(RADIANS * angle) * radius, origin.Y - Math.Sin(RADIANS * angle) * radius);
        }
    }
}
