using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;


namespace TestsSystems_HardnessTester
{
    public abstract class MeasuringShape : Shape
    {
        #region свойства зависимости

        public static readonly DependencyProperty ShowLengthProperty = DependencyProperty.Register("ShowLength", typeof(bool), typeof(MeasuringShape),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public bool ShowLength
        {
            get
            {
                return (bool)GetValue(ShowLengthProperty);
            }
            set
            {
                SetValue(ShowLengthProperty, value);
            }
        }

        public static readonly DependencyProperty TextСolorProperty = DependencyProperty.Register("TextСolor", typeof(SolidColorBrush), typeof(MeasuringShape),
          new FrameworkPropertyMetadata(Brushes.Red, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public SolidColorBrush TextСolor
        {
            get
            {
                return (SolidColorBrush)GetValue(TextСolorProperty);
            }
            set
            {
                SetValue(TextСolorProperty, value);
            }
        }

        public static readonly DependencyProperty TextSizeProperty = DependencyProperty.Register("TextSize", typeof(double), typeof(MeasuringShape),
          new FrameworkPropertyMetadata(15.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public double TextSize
        {
            get
            {
                return (double)GetValue(TextSizeProperty);
            }
            set
            {
                SetValue(TextSizeProperty, value);
            }
        }

        public static readonly DependencyProperty TransformGroupShapeProperty = DependencyProperty.Register("TransformGroupShape", typeof(TransformGroup), typeof(MeasuringShape),
      new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public TransformGroup TransformGroupShape
        {
            get
            {
                return (TransformGroup)GetValue(TransformGroupShapeProperty);
            }
            set
            {
                SetValue(TransformGroupShapeProperty, value);
            }
        }

        #endregion

        #region abstract and virtual Metod and Features

        internal abstract double SizeShape { get; }

        internal abstract void Scale(double k);

        internal abstract void Rotation(double angle);

        internal abstract void Posting(params double[] p);

        internal abstract Point GetCenter();


        internal virtual void Move(params double[] p)
        {
            translateTransformPostingShape.X += p[0];
            translateTransformPostingShape.Y += p[1];
        }

        protected virtual Point GetPointTextOrigin(Point centerShape,double textWidth, double textHeight)
        {
            return new Point(centerShape.X - textWidth / 2.0, centerShape.Y - textHeight/2.0);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (ShowLength)
            {
                var length = SizeMeasuringShape;
                string ei = CoefficientPxtomm == 1 ? "px" : "mm";
                var formattedText = new FormattedText(
                    $"{length:F3}" + ei,
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Times New Roman"),
                    TextSize,
                    TextСolor, 1.0);
                var textWidth = formattedText.Width;
                var textHeight = formattedText.Height;
                var midPoint = GetCenter();

                // Для расчета положения текста нужно учесть его размеры   
               var textOrigin = GetPointTextOrigin(midPoint, textWidth, textHeight);
               drawingContext.DrawText(formattedText, textOrigin);
            }
        }
        #endregion

        #region static Metod and Features

        public static double fontSize = 50;
        public static double strokeThickness = 2;
        public static double multiplierStrokeThickness = 1;
        public static SolidColorBrush colorBrush = new SolidColorBrush(Colors.Red);
        private static double сoefficientPxtomm = 1;

       // TransformGroup transformGroupShape = new TransformGroup();

        internal static double CoefficientPxtomm
        {
            get => сoefficientPxtomm;
            set
            {
                сoefficientPxtomm = value;
            }
        }

        //internal static void AllTextUpdate(Canvas c)
        //{
        //    foreach (var s in c.Children)
        //        if (s is MeasuringShape mS)
        //            mS.TextUpdate();

        //}
        #endregion

        #region internal Features

        internal double SizeMeasuringShape => SizeShape * CoefficientPxtomm;

        internal RotateTransform rotateTransformCenter;
        internal ScaleTransform scaleTransformShape;
        internal RotateTransform rotateTransformPostingShape;
        internal TranslateTransform translateTransformPostingShape = new TranslateTransform();
      

        #endregion

        #region Constructor
        public MeasuringShape()
        {
            this.Stroke = colorBrush;
            //this.StrokeThickness = strokeThickness * multiplierStrokeThickness;
            this.Fill = Brushes.Transparent;
            this.RenderTransform = translateTransformPostingShape;
        }

        #endregion

        #region Private Metods


        #endregion

    }

    public class MeasuringLine : MeasuringShape
    {
        #region свойства зависимости

        public static readonly DependencyProperty X1Property = DependencyProperty.Register("X1", typeof(double), typeof(MeasuringLine),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty Y1Property = DependencyProperty.Register("Y1", typeof(double), typeof(MeasuringLine),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty X2Property = DependencyProperty.Register("X2", typeof(double), typeof(MeasuringLine),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty Y2Property = DependencyProperty.Register("Y2", typeof(double), typeof(MeasuringLine),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty EndSizeProperty = DependencyProperty.Register("EndSiz", typeof(Double), typeof(MeasuringLine),
            new FrameworkPropertyMetadata(10.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        [TypeConverter(typeof(LengthConverter))]
        public double X1
        {
            get
            {
                return (double)GetValue(X1Property);
            }
            set
            {
                SetValue(X1Property, value);
            }
        }

        [TypeConverter(typeof(LengthConverter))]
        public double Y1
        {
            get
            {
                return (double)GetValue(Y1Property);
            }
            set
            {
                SetValue(Y1Property, value);
            }
        }

        [TypeConverter(typeof(LengthConverter))]
        public double X2
        {
            get
            {
                return (double)GetValue(X2Property);
            }
            set
            {
                SetValue(X2Property, value);
            }
        }

        [TypeConverter(typeof(LengthConverter))]
        public double Y2
        {
            get
            {
                return (double)GetValue(Y2Property);
            }
            set
            {
                SetValue(Y2Property, value);
            }
        }

        [TypeConverter(typeof(LengthConverter))]
        public double EndSize
        {
            get { return (double)this.GetValue(EndSizeProperty); }
            set { this.SetValue(EndSizeProperty, value); }
        }

        #endregion

        #region переопределённые свойства и методы 

        protected override Geometry DefiningGeometry
        {
            get
            {
                Point p1 = new Point(X1, Y1);
                Point p2 = new Point(X2, Y2);

                double dx = p2.X - p1.X;
                double dy = p2.Y - p1.Y;
                double length = Math.Sqrt(dx * dx + dy * dy);
                double ux = dx / length;
                double uy = dy / length;

                double vx1 = -uy;
                double vy1 = ux;
                double vx2 = uy;
                double vy2 = -ux;

                Point p3 = new Point(p1.X + EndSize * vx1, p1.Y + EndSize * vy1);
                Point p4 = new Point(p2.X + EndSize * vx1, p2.Y + EndSize * vy1);
                Point p5 = new Point(p1.X + EndSize * vx2, p1.Y + EndSize * vy2);
                Point p6 = new Point(p2.X + EndSize * vx2, p2.Y + EndSize * vy2);

                List<PathSegment> segments = new List<PathSegment>() {
                new LineSegment(p1, false),
                new LineSegment(p2, true),
                new LineSegment(p4, false),
                new LineSegment(p6, true),
                new LineSegment(p3, false),
                new LineSegment(p5, true)};

                List<PathFigure> figures = new List<PathFigure>(1);
                PathFigure pf = new PathFigure(p1, segments, false)
                {
                    IsFilled = false
                };
                figures.Add(pf);
                Geometry g = new PathGeometry(figures, FillRule.EvenOdd, null);


                return g;
            }
        }

        internal override double SizeShape
        {
            get
            {
                double dx = X2 - X1;
                double dy = Y2 - Y1;
                return Math.Sqrt(dx * dx + dy * dy);
            }
        }

        internal override Point GetCenter()
        {
            return new Point((X1 + X2) / 2.0, (Y1 + Y2) / 2.0);
        }

        protected override Point GetPointTextOrigin(Point centerShape, double textWidth, double textHeight)
        {
            return new Point(centerShape.X - textWidth / 2.0, centerShape.Y );
        }

        internal override void Posting(params double[] p)//конечная точка
        {
            X2 = p[0];
            Y2 = p[1];
        }

        internal override void Move(params double[] p)//координаты начальной точки(x,y)
                                                      //приражения по координатам(dx,dy)
        {
            double share = 0.2;
            var sizeLine = SizeShape;
            var h = sizeLine * share;
            p[0] -= Canvas.GetLeft(this);
            p[1] -= Canvas.GetTop(this);
            if (X1 + h > p[0] && Y1 + h > p[1] && X1 - h < p[0] && Y1 - h < p[1])
            {
                X1 += p[2];
                Y1 += p[3];
            }
            else if (X2 + h > p[0] && Y2 + h > p[1] && X2 - h < p[0] && Y2 - h < p[1])
            {
                X2 += p[2];
                Y2 += p[3];
            }

        }

        internal override void Rotation(double angle)
        {
            return;
        }

        internal override void Scale(double k)
        {
            return;
        }

        #endregion

        #region конструктор
        public MeasuringLine() : base()
        {
            translateTransformPostingShape = new TranslateTransform();
            scaleTransformShape = new ScaleTransform();
            this.RenderTransform = translateTransformPostingShape;
            //EndSize = multiplierEndSize * endSize;
 
        }
        #endregion

    }

    public class MeasuringSquare : MeasuringShape
    {
        #region свойства зависимости 
        public static readonly DependencyProperty SideProperty = DependencyProperty.Register("Side", typeof(double), typeof(MeasuringSquare),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

        [TypeConverter(typeof(LengthConverter))]
        public double Side
        {
            get
            {
                return (double)GetValue(SideProperty);
            }
            set
            {
                SetValue(SideProperty, value);
            }
        }

        #endregion

        #region переопределённые методы и свойства 

        protected override Geometry DefiningGeometry
        {
            get
            {
                Point p1 = new Point(0, 0);
                Point p2 = new Point(0, Side);
                Point p3 = new Point(Side, Side);
                Point p4 = new Point(Side, 0);
                List<PathSegment> segments = new List<PathSegment>(6)
                {
                //new LineSegment(p1, true),
                new LineSegment(p2, true),
                new LineSegment(p3, true),
                new LineSegment(p4, true)
                };


                List<PathFigure> figures = new List<PathFigure>(1);

                PathFigure pf = new PathFigure(p1, segments, true);

                figures.Add(pf);

                Geometry g = new PathGeometry(figures, FillRule.EvenOdd, null)
                {
                    Transform = TransformGroupShape
                };
                return g;
            }
        }

        internal override double SizeShape
            => Side * Math.Sqrt(2) * scaleTransformShape.ScaleY;

        internal override Point GetCenter()
        {
            var p = new Point(Side / 2.0, Side / 2.0);
            return TransformGroupShape.Transform(p);
        }

        internal override void Posting(params double[] p)//диагональ,угол 
        {
            double sideLength = p[0] / Math.Sqrt(2);
            double angle = p[1];
            Side = sideLength;
            rotateTransformCenter.CenterX = sideLength / 2.0;
            rotateTransformCenter.CenterY = sideLength / 2.0;
            rotateTransformPostingShape.Angle = angle - 45;
        }

        internal override void Rotation(double angle)
        {
            var center = new Point(Side / 2.0, Side / 2.0);
            rotateTransformCenter.Angle += angle;
            rotateTransformCenter.CenterX = center.X;
            rotateTransformCenter.CenterY = center.Y;
        }

        internal override void Scale(double k)
        {
       
            k /= Side;
            scaleTransformShape.ScaleX += k;
            scaleTransformShape.ScaleY += k;
            var p = new Point(Side / 2.0, Side / 2.0);
            scaleTransformShape.CenterX = p.X;
            scaleTransformShape.CenterY = p.Y;

        }
        
        #endregion

        #region конструктор 
        public MeasuringSquare() : base()
        {
            //создаём трансформы, сохраняем для них сылки
            //для последующего преобразования преобразования
            TransformGroupShape = new TransformGroup();
            rotateTransformCenter = new RotateTransform();
            scaleTransformShape = new ScaleTransform();
            rotateTransformPostingShape = new RotateTransform();
            translateTransformPostingShape = new TranslateTransform();
            TransformGroupShape.Children.Add(rotateTransformCenter);
            TransformGroupShape.Children.Add(scaleTransformShape);
            TransformGroupShape.Children.Add(rotateTransformPostingShape);
            this.RenderTransform = translateTransformPostingShape;
        }
        #endregion 

    }

    public class MeasuringСircle : MeasuringShape
    {
        #region свойства зависимости 

        public static readonly DependencyProperty DiameterProperty = DependencyProperty.Register("Radius", typeof(double), typeof(MeasuringСircle),
          new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        [TypeConverter(typeof(LengthConverter))]
        internal double Diameter
        {
            get
            {
                return (double)GetValue(DiameterProperty);
            }
            set
            {
                SetValue(DiameterProperty, value);
            }
        }

        #endregion

        #region переопределённые методы и свойства 

        protected override Geometry DefiningGeometry
        {
            get
            {
                if (Diameter <= 0)
                {
                    return Geometry.Empty;
                }

                var elip = new EllipseGeometry(new Rect(0, 0, Diameter, Diameter))
                {
                    Transform = TransformGroupShape
                };
                return elip;
            }
        }

        internal override double SizeShape => Diameter * scaleTransformShape.ScaleX;

        internal override Point GetCenter()
        {
            var p = new Point(Diameter / 2.0, Diameter / 2.0);
            return TransformGroupShape.Transform(p);
        }

        internal override void Posting(params double[] p)
        {
            var delta = p[2] / 2.0 * (1.0 - 1.0 / Math.Sqrt(2));//линейное расстояние между углом
                                                                //ограничевающего квадрата и окружностью
            rotateTransformPostingShape.CenterX = delta;
            rotateTransformPostingShape.CenterY = delta;
            rotateTransformPostingShape.Angle = p[3] - 45;
            Diameter = p[2];
            Canvas.SetLeft(this, p[0] - delta);
            Canvas.SetTop(this, p[1] - delta);

        } 
         
        internal override void Rotation(double angle)
        {
            return;//throw new NotImplementedException();
        }

        internal override void Scale(double k)
        {
            k /= Diameter;
            scaleTransformShape.ScaleX += k;
            scaleTransformShape.ScaleY += k;
            var p = new Point(Diameter / 2.0, Diameter / 2.0);
            scaleTransformShape.CenterX = p.X;
            scaleTransformShape.CenterY = p.Y;
        }
        #endregion

        #region конструктор 
        public MeasuringСircle() : base()
        {
            //создаём трансформы, сохраняем для них сылки
            //для последующего преобразования преобразования
            TransformGroupShape = new TransformGroup();

            scaleTransformShape = new ScaleTransform();
            rotateTransformPostingShape = new RotateTransform();
            translateTransformPostingShape = new TranslateTransform();
            TransformGroupShape.Children.Add(scaleTransformShape);
            TransformGroupShape.Children.Add(rotateTransformPostingShape);
            //TransformGroupShape.Children.Add(translateTransformPostingShape);

            this.RenderTransform = translateTransformPostingShape;//TransformGroupShape;
        }
        #endregion
    }
}
