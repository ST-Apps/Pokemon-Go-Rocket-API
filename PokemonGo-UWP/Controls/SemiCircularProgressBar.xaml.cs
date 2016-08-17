using System;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace PokemonGo_UWP.Controls
{
    public sealed partial class SemiCircularProgressBar
    {
        #region ChangedEventHandlers

        private static void OnPercentageChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            SemiCircularProgressBar circularProgressBar = sender as SemiCircularProgressBar;
            if (circularProgressBar != null) circularProgressBar.Angle = (circularProgressBar.Percentage * 180) / 100;
        }

        private static void OnValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var circularProgress = sender as SemiCircularProgressBar;
            if (circularProgress != null)
            {
                if (circularProgress.Value == 0)
                    circularProgress.Percentage = 0;

                var currentValue = Math.Min(circularProgress.Value, circularProgress.Maximum);
                circularProgress.Percentage = (double)currentValue / circularProgress.Maximum * 100;
            }
        }

        private static void OnPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            SemiCircularProgressBar circularProgressBar = sender as SemiCircularProgressBar;
            circularProgressBar?.RenderArc();
        }

        private static void OnImageSourcePathChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            SemiCircularProgressBar circularProgressBar = sender as SemiCircularProgressBar;
            if (circularProgressBar?.ImageSourcePath != null)
            {
                circularProgressBar.InnerPathRoot.Fill = new ImageBrush
                {
                    ImageSource = circularProgressBar.ImageSourcePath
                };
            }
        }

        #endregion

        #region Properties

        private static readonly DependencyProperty PercentageProperty = DependencyProperty.Register("Percentage", typeof(double), typeof(SemiCircularProgressBar), new PropertyMetadata(65d, OnPercentageChanged));

        public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register("StrokeThickness", typeof(int), typeof(SemiCircularProgressBar), new PropertyMetadata(5));

        public static readonly DependencyProperty SegmentColorProperty = DependencyProperty.Register("SegmentColor", typeof(Brush), typeof(SemiCircularProgressBar), new PropertyMetadata(new SolidColorBrush(Colors.DeepSkyBlue)));

        public static readonly DependencyProperty DiameterProperty = DependencyProperty.Register("Diameter", typeof(int), typeof(SemiCircularProgressBar), new PropertyMetadata(25, OnPropertyChanged));

        public static readonly DependencyProperty AngleProperty = DependencyProperty.Register("Angle", typeof(double), typeof(SemiCircularProgressBar), new PropertyMetadata(120d, OnPropertyChanged));

        public static readonly DependencyProperty ValuePropery = DependencyProperty.Register("Value", typeof(int), typeof(SemiCircularProgressBar), new PropertyMetadata(0, OnValueChanged));

        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register("Maximum", typeof(int), typeof(SemiCircularProgressBar), new PropertyMetadata(100, OnValueChanged));

        public static readonly DependencyProperty InnerSegmentColorProperty = DependencyProperty.Register("InnerSegmentColor", typeof(Brush), typeof(SemiCircularProgressBar), new PropertyMetadata(Colors.Gray));

        public static readonly DependencyProperty ImageSourcePathProperty = DependencyProperty.Register("ImageSourcePath", typeof(BitmapImage), typeof(SemiCircularProgressBar), new PropertyMetadata("", OnImageSourcePathChanged));

        #endregion

        #region Values

        public int Diameter
        {
            get { return (int)GetValue(DiameterProperty) / 2 - StrokeThickness / 2; }
            set { SetValue(DiameterProperty, (value) / 2 - StrokeThickness / 2); }
        }

        public double Angle
        {
            get { return (double)GetValue(AngleProperty); }
            set { SetValue(AngleProperty, value); }
        }

        public Brush SegmentColor
        {
            get { return (Brush)GetValue(SegmentColorProperty); }
            set { SetValue(SegmentColorProperty, value); }
        }

        public int StrokeThickness
        {
            get { return (int)GetValue(StrokeThicknessProperty); }
            set { SetValue(StrokeThicknessProperty, value); }
        }

        private double Percentage
        {
            get { return (double)GetValue(PercentageProperty); }
            set { SetValue(PercentageProperty, value); }
        }

        public int Value
        {
            get { return Convert.ToInt32(GetValue(ValuePropery)); }
            set { SetValue(ValuePropery, value); }
        }

        public int Maximum
        {
            get { return (int)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        public Brush InnerSegmentColor
        {
            get { return (Brush)GetValue(InnerSegmentColorProperty); }
            set { SetValue(InnerSegmentColorProperty, value); }
        }

        public BitmapImage ImageSourcePath
        {
            get { return (BitmapImage)GetValue(ImageSourcePathProperty); }
            set { SetValue(ImageSourcePathProperty, value); }
        }
        #endregion

        public SemiCircularProgressBar()
        {
            InitializeComponent();
            Angle = (Percentage * 180) / 100;

            RenderArc();
        }

        public void RenderArc()
        {
            RenderInnerArc();
            Point startPoint = new Point(Diameter, 0);
            Point endPoint = ComputeCartesianCoordinate(Angle, Diameter);
            endPoint.X += Diameter;
            endPoint.Y += Diameter;

            PathRoot.Width = Diameter * 2 + PathRoot.StrokeThickness;
            PathRoot.Height = Diameter * 2 + PathRoot.StrokeThickness;
            PathRoot.RenderTransform = new CompositeTransform
            {
                TranslateX = PathRoot.StrokeThickness / 2,
                TranslateY = PathRoot.StrokeThickness / 2
            };

            bool largeArc = Angle > 180.0;

            Size outerArcSize = new Size(Diameter, Diameter);

            PathFigure.StartPoint = startPoint;

            if (startPoint.X == Math.Round(endPoint.X) && startPoint.Y == Math.Round(endPoint.Y))
                endPoint.X -= 0.01;

            ArcSegment.Point = endPoint;
            ArcSegment.Size = outerArcSize;
            ArcSegment.IsLargeArc = largeArc;
        }

        public void RenderInnerArc()
        {
            Point startPoint = new Point(Diameter, 0);
            Point endPoint = ComputeCartesianCoordinate(180, Diameter);
            endPoint.X += Diameter;
            endPoint.Y += Diameter;

            InnerPathRoot.Width = Diameter * 2 + PathRoot.StrokeThickness;
            InnerPathRoot.Height = Diameter * 2 + PathRoot.StrokeThickness;
            InnerPathRoot.RenderTransform = new CompositeTransform
            {
                TranslateX = InnerPathRoot.StrokeThickness / 2,
                TranslateY = InnerPathRoot.StrokeThickness / 2
            };

            Size outerArcSize = new Size(Diameter, Diameter);

            InnerPathFigure.StartPoint = startPoint;

            if (startPoint.X == Math.Round(endPoint.X) && startPoint.Y == Math.Round(endPoint.Y))
                endPoint.X -= 0.01;

            InnerArcSegment.Point = endPoint;
            InnerArcSegment.Size = outerArcSize;
            InnerArcSegment.IsLargeArc = true;
        }

        private Point ComputeCartesianCoordinate(double angle, double diameter)
        {
            double angleRad = (Math.PI / 180.0) * (angle - 90);

            double x = diameter * Math.Cos(angleRad);
            double y = diameter * Math.Sin(angleRad);

            return new Point(x, y);
        }
    }
}
