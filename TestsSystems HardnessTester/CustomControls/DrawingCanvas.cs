using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;
using System.Collections.Generic;
using System.ComponentModel;

namespace TestsSystems_HardnessTester
{
    public class DrawingCanvas : Canvas
    {
        private MeasuringShape shape;
        private Point startPoint = new Point();
        private Point endPoint = new Point();

        public MeasuringShape GetShape() { return shape; }

        public DrawingCanvas() : base()
        {
            this.MouseMove += DrawingCanvas_MouseMove;
            this.MouseLeftButtonDown += DrawingCanvas_MouseLeftButtonDown;
            this.MouseLeftButtonUp += DrawingCanvas_MouseLeftButtonUp;
            this.MouseRightButtonDown += DrawingCanvas_MouseRightButtonDown;
            this.MouseWheel += DrawingCanvas_MouseWheel;

            //тестируется
            this.SizeChanged += DrawingCanvas_SizeChanged;
        }

        private void DrawingCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var minL = Math.Min(e.NewSize.Width, e.NewSize.Height);
            var sT = minL * 0.002;//0.2% от минимального
                                  //линейного размера canvas
            var eS = minL * 0.02;//2% от минимального
                                 //линейного размера canvas
            var fS = minL * 0.04;
            MeasuringShape.strokeThickness = sT;
            MeasuringShape.fontSize = fS;
            MeasuringLine.endSize = eS;
            foreach (var item in this.Children)
            {
                if (item is MeasuringShape shape)
                {
                    shape.StrokeThickness = sT * MeasuringShape.multiplierStrokeThickness / shape.scaleTransformShape.ScaleX;
                    shape.tb.FontSize = fS;
                }

                if (item is MeasuringLine line)
                    line.EndSize = eS * MeasuringLine.multiplierEndSize;
            }
        }

        #region Events Canvas

        private void DrawingCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (IsKeyDownScaleDC())
                return;
            else
                e.Handled = true;

            startPoint = e.GetPosition(this);
            var _shape = VisualTreeHelper.HitTest(this, startPoint).VisualHit;

            if (!(_shape is MeasuringShape))
            {
                switch (CurrentShape)
                {
                    case SelectedShape.Square:
                        {
                            shape = CreatRect(this, startPoint);
                            break;
                        }
                    case SelectedShape.Circle:
                        {
                            shape = CreatCircle(startPoint);
                            break;
                        }
                    case SelectedShape.Line:
                        {
                            shape = CreatLine(this, startPoint);
                            break;
                        }
                    default: return;

                }

                //shape.SizeChanged += SharpSizeChanged;

                ActionsStatus = Actions.Posting;
            }
            else
            {
                ActionsStatus = Actions.Move;
                shape = _shape as MeasuringShape;
            }
        }

        private void DrawingCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (IsKeyDownScaleDC())
                return;
            else
                e.Handled = true;

            if (e.LeftButton == MouseButtonState.Pressed && shape != null)
            {
                endPoint = e.GetPosition(this);
                switch (ActionsStatus)
                {
                    case Actions.Posting:
                        {
                            double diagonalLength = Math.Sqrt(Math.Pow(endPoint.X - startPoint.X, 2) + Math.Pow(endPoint.Y - startPoint.Y, 2));
                            double angle = Math.Atan2(endPoint.Y - startPoint.Y, endPoint.X - startPoint.X) * 180 / Math.PI;
                            switch (CurrentShape)
                            {
                                case SelectedShape.Square:
                                    {
                                        (shape as MeasuringSquare)?.Posting(diagonalLength, angle);
                                        break;
                                    }
                                case SelectedShape.Circle:
                                    {
                                        (shape as MeasuringСircle)?.Posting(startPoint.X, startPoint.Y, diagonalLength, angle);
                                        break;
                                    }
                                case SelectedShape.Line:
                                    {
                                        (shape as MeasuringLine)?.Posting(endPoint.X - startPoint.X, endPoint.Y - startPoint.Y);
                                        break;
                                    }
                            }
                            break;
                        }
                    case Actions.Move:
                        {
                            double deltaX = endPoint.X - startPoint.X;
                            double deltaY = endPoint.Y - startPoint.Y;
                            if (shape is MeasuringSquare s)
                                s.Move(deltaX, deltaY);
                            else if (shape is MeasuringLine l)
                                l.Move(startPoint.X, startPoint.Y, deltaX, deltaY);
                            else if (shape is MeasuringСircle c)
                                c.Move(deltaX, deltaY);

                            startPoint = endPoint;

                            // SharpSizeChanged(shape, null);
                            break;
                        }
                }
            }
        }

        private void DrawingCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (IsKeyDownScaleDC())
                return;
            else
                e.Handled = true;

            Point pt = e.GetPosition(this);

            var k = increaseSize * e.Delta / Math.Abs(e.Delta);
            HitTestResult result = VisualTreeHelper.HitTest(this, pt);
            if (result.VisualHit is MeasuringShape shape)
            {
                if (IsKeyDownRotation())
                    shape.Rotation(k);
                else
                    shape.Scale(k);
            }


        }

        private void DrawingCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (IsKeyDownScaleDC())
                return;
            else
                e.Handled = true;

            if (shape != null && (shape.SizeShape < 10))//если фигура маленькая то она удаляется 
            {
                this.Children.Remove(shape);
                shape = null;
            }

            ActionsStatus = Actions.None;
        }

        private void DrawingCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (IsKeyDownScaleDC())
                return;
            else
                e.Handled = true;

            Point pt = e.GetPosition(this);
            HitTestResult result = VisualTreeHelper.HitTest(this, pt);

            if (result != null && result.VisualHit is Shape s)
            {
                this.Children.Remove(s);
                shape = null;
            }
            
        }

        #endregion

        #region functions
        public void ClearShapes()
        {
            for (int i = this.Children.Count - 1; i >= 0; i--)
                if (this.Children[i] is Shape _shape)
                    this.Children.Remove(_shape);
        }

        private MeasuringСircle CreatCircle(Point p)
        {
            var elip = new MeasuringСircle(this, p);
            return elip;
        }

        private MeasuringSquare CreatRect(Canvas canvas, Point p)
        {
            var shape = new MeasuringSquare(canvas, p);
            return shape;
        }

        private MeasuringLine CreatLine(Canvas canvas, Point p)
        {
            var shape = new MeasuringLine(canvas, p);
            return shape;
        }

        public double GetSizeShape()//(Shape shape)
        {
            if(shape== null) return 0;
            return shape.SizeShape;
        }

        public void PaintCircle(double R, double X, double Y)
        {
            var _circle = CreatCircle(new Point(X + R, X + Y));
            SetLeft(_circle, X - R);
            SetTop(_circle, Y - R);
            _circle.Diameter = 2 * R;
            this.Children.Add(_circle);
        }

        private bool IsKeyDownRotation() => Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
        private bool IsKeyDownScaleDC() => Keyboard.IsKeyDown(Key.RightCtrl) || Keyboard.IsKeyDown(Key.LeftCtrl);

        internal void PaintSquare(float width, float angle, float x, float y)
        {
            throw new NotImplementedException();
        }

        internal SelectedShape GetTypeShape()
        {
            if (shape is MeasuringСircle)
                return SelectedShape.Circle;
            else if (shape is MeasuringSquare)
                return SelectedShape.Square;
            else if (shape is MeasuringLine)
                return SelectedShape.Line;
            else return SelectedShape.None;
        }

        #endregion

        #region Enum Canvas

        private Actions _actions;
        public Actions ActionsStatus { get => _actions; set => _actions = value; }
        public enum Actions
        {
            Posting,//Размещение
            Move,//Перемещение 
            Turn,//Вращение
            None
        }

        private SelectedShape _currentShape;
        public SelectedShape CurrentShape { get => _currentShape; set => _currentShape = value; }
        public enum SelectedShape
        {
            Circle,//окружность
            Square,//квадрат
            Line,
            None,
        }

        #endregion

        #region Shape settings

        internal double increaseSize = 0.7;//0.2

        #endregion
    }

    public abstract class MeasuringShape : Shape
    {

        internal static double CoefficientPxtomm
        {
            get => сoefficientPxtomm;
            set
            {
                сoefficientPxtomm = value;
            }
        }
        internal double SizeMeasuringShape => SizeShape * CoefficientPxtomm;

        internal abstract double SizeShape { get; }

        internal abstract void Posting(params double[] p);
        internal virtual void Move(params double[] p)
        {
            translateTransformPostingShape.X += p[0];
            translateTransformPostingShape.Y += p[1];
        }

        internal abstract void Scale(double k);

        internal abstract void Rotation(double angle);

        internal abstract Point GetCenter();

        internal virtual void TextUpdate()
        {
            string ei = CoefficientPxtomm == 1 ? "px" : "mm";
            tb.Text = SizeMeasuringShape.ToString("N3") + ei;
        }

        internal static void AllTextUpdate(Canvas c)
        {
            foreach (var s in c.Children)
                if (s is MeasuringShape mS)
                    mS.TextUpdate();

        }

        internal TextBlock tb = new TextBlock();
        internal Canvas panel;
        internal RotateTransform rotateTransformCenter;
        internal ScaleTransform scaleTransformShape;
        internal RotateTransform rotateTransformPostingShape;
        internal TranslateTransform translateTransformPostingShape;
        internal TranslateTransform translateTransformPostingTB;

        #region настройки линии
        public static double fontSize = 50;
        public static double strokeThickness = 2;
        public static double multiplierStrokeThickness = 1;
        public static SolidColorBrush colorBrush = new SolidColorBrush(Colors.Red);
        private static double сoefficientPxtomm = 1;

        #endregion

        public MeasuringShape(Canvas panel)
        {
            this.panel = panel;
            Unloaded += Shape_Unloaded;
            tb.MouseRightButtonDown += (s, e) =>
            {
                panel.Children.Remove(this);//как же это ужасно..
                var shape = (panel as DrawingCanvas).GetShape();
                shape = null;
            };
            this.Stroke = colorBrush;
            this.StrokeThickness = strokeThickness * multiplierStrokeThickness;
            this.Fill = Brushes.Transparent;
            tb.Foreground = colorBrush;
            tb.FontSize = fontSize;
            panel?.Children.Add(tb);
            panel?.Children.Add(this);
        }


        private void Shape_Unloaded(object sender, RoutedEventArgs e)
        {
            panel?.Children.Remove(tb);//удаляем textbloc
        }


        #region INotifyPropertyChanged
        //public event PropertyChangedEventHandler PropertyChanged;
        //public void OnPropertyChanged([CallerMemberName] string prop = "")
        //{
        //    if (PropertyChanged != null)
        //        PropertyChanged(this, new PropertyChangedEventArgs(prop));
        //}
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

        public static double multiplierEndSize = 1;

        public static double endSize = 1;
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


        internal override void Scale(double k)
        {
            return;
        }

        internal override Point GetCenter()
        {
            return new Point((X1 + X2) / 2.0, (Y1 + Y2) / 2.0);
        }

        internal override void Rotation(double angle)
        {
            return;
        }


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

                #region Текст 
                Canvas.SetLeft(tb, Canvas.GetLeft(this) + (p1.X + p2.X) / 2 - tb.ActualWidth / 2.0);
                Canvas.SetTop(tb, Canvas.GetTop(this) + (p1.Y + p2.Y) / 2);

                //string ei = СoefficientPxtomm == 1 ? "px" : "mm";
                //tb.Text = SizeMeasuringShape.ToString("N3") + ei;
                TextUpdate();
                #endregion

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



        public MeasuringLine(Canvas panel, Point p) : base(panel)
        {
            translateTransformPostingShape = new TranslateTransform();
            scaleTransformShape = new ScaleTransform();
            this.RenderTransform = translateTransformPostingShape;
            tb.RenderTransform = translateTransformPostingShape;
            EndSize = multiplierEndSize * endSize;
            Canvas.SetLeft(this, p.X);
            Canvas.SetTop(this, p.Y);
            Canvas.SetLeft(tb, p.X);
            Canvas.SetTop(tb, p.Y);
        }


    }

    public class MeasuringSquare : MeasuringShape
    {
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

                Geometry g = new PathGeometry(figures, FillRule.EvenOdd, null);

                return g;
            }
        }


        internal override double SizeShape
            => Side * Math.Sqrt(2) * scaleTransformShape.ScaleY;

        internal override void Posting(params double[] p)//диагональ,угол ,
                                                         //из какой точки рисовать
        {
            double sideLength = p[0] / Math.Sqrt(2);
            double angle = p[1];
            Side = sideLength;
            rotateTransformCenter.CenterX = sideLength / 2.0;
            rotateTransformCenter.CenterY = sideLength / 2.0;
            rotateTransformPostingShape.Angle = angle - 45;
            translateTransformPostingTB.X = p[0] / 2.0 * Math.Cos(angle * Math.PI / 180.0) - tb.ActualWidth / 2.0;
            translateTransformPostingTB.Y = p[0] / 2.0 * Math.Sin(angle * Math.PI / 180.0) - tb.ActualHeight / 2.0;

            TextUpdate();
        }

        internal override void Scale(double k)
        {
            k /= Side;

            scaleTransformShape.ScaleX += k;
            scaleTransformShape.ScaleY += k;
            StrokeThickness = strokeThickness * multiplierStrokeThickness / scaleTransformShape.ScaleX;
            var p = GetCenter();
            scaleTransformShape.CenterX = p.X;
            scaleTransformShape.CenterY = p.Y;

            TextUpdate();
        }

        internal override Point GetCenter()
        {
            return new Point(Side / 2.0, Side / 2.0);
        }

        internal override void Rotation(double angle)
        {
            var center = GetCenter();
            rotateTransformCenter.Angle += angle;
            rotateTransformCenter.CenterX = center.X;
            rotateTransformCenter.CenterY = center.Y;
        }

        public MeasuringSquare(Canvas panel, Point p) : base(panel)
        {
            //создаём трансформы, сохраняем для них сылки
            //для последующего преобразования преобразования
            TransformGroup transformGroupShape = new TransformGroup();
            rotateTransformCenter = new RotateTransform();
            scaleTransformShape = new ScaleTransform();
            rotateTransformPostingShape = new RotateTransform();
            translateTransformPostingShape = new TranslateTransform();
            transformGroupShape.Children.Add(rotateTransformCenter);
            transformGroupShape.Children.Add(scaleTransformShape);
            transformGroupShape.Children.Add(rotateTransformPostingShape);
            transformGroupShape.Children.Add(translateTransformPostingShape);
            this.RenderTransform = transformGroupShape;

            TransformGroup transformGroupTB = new TransformGroup();
            translateTransformPostingTB = new TranslateTransform();
            transformGroupTB.Children.Add(translateTransformPostingShape);
            transformGroupTB.Children.Add(translateTransformPostingTB);
            tb.RenderTransform = transformGroupTB;

            Canvas.SetLeft(this, p.X);
            Canvas.SetTop(this, p.Y);
            Canvas.SetLeft(tb, p.X);
            Canvas.SetTop(tb, p.Y);

        }
    }

    public class MeasuringСircle : MeasuringShape
    {

        public static readonly DependencyProperty DiameterProperty = DependencyProperty.Register("Radius", typeof(double), typeof(MeasuringСircle),
          new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public MeasuringСircle(Canvas panel, Point p) : base(panel)
        {
            //создаём трансформы, сохраняем для них сылки
            //для последующего преобразования преобразования
            TransformGroup transformGroupShape = new TransformGroup();
            scaleTransformShape = new ScaleTransform();
            rotateTransformPostingShape = new RotateTransform();
            translateTransformPostingShape = new TranslateTransform();
            transformGroupShape.Children.Add(scaleTransformShape);
            transformGroupShape.Children.Add(rotateTransformPostingShape);
            transformGroupShape.Children.Add(translateTransformPostingShape);
            this.RenderTransform = transformGroupShape;


            TransformGroup transformGroupTB = new TransformGroup();
            translateTransformPostingTB = new TranslateTransform();
            transformGroupTB.Children.Add(translateTransformPostingShape);
            transformGroupTB.Children.Add(translateTransformPostingTB);
            tb.RenderTransform = transformGroupTB;

            Canvas.SetLeft(this, p.X);
            Canvas.SetTop(this, p.Y);
            Canvas.SetLeft(tb, p.X);
            Canvas.SetTop(tb, p.Y);
        }

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


        protected override Geometry DefiningGeometry
        {
            get
            {
                if (Diameter <= 0)
                {
                    return Geometry.Empty;
                }

                return new EllipseGeometry(new Rect(0, 0, Diameter, Diameter));
            }
        }

        internal override double SizeShape => Diameter * scaleTransformShape.ScaleX;

        internal override Point GetCenter()
        {
            return new Point(Diameter / 2.0, Diameter / 2.0);
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

            //Canvas.SetLeft(tb, p[0] - delta);
            //Canvas.SetTop(tb, p[1] - delta);
            translateTransformPostingTB.X = p[2] / 2.0 * Math.Cos(p[3] * Math.PI / 180.0) - tb.ActualWidth / 2.0;
            translateTransformPostingTB.Y = p[2] / 2.0 * Math.Sin(p[3] * Math.PI / 180.0) - tb.ActualHeight / 2.0;

            TextUpdate();
        }

        internal override void Rotation(double angle)
        {
            throw new NotImplementedException();
        }

        internal override void Scale(double k)
        {
            k /= Diameter;

            scaleTransformShape.ScaleX += k;
            scaleTransformShape.ScaleY += k;

            StrokeThickness = strokeThickness * multiplierStrokeThickness / scaleTransformShape.ScaleX;

            var p = GetCenter();
            scaleTransformShape.CenterX = p.X;
            scaleTransformShape.CenterY = p.Y;

            TextUpdate();
            //k = k / Side;

            //scaleTransformShape.ScaleX += k;
            //scaleTransformShape.ScaleY += k;
            //StrokeThickness = strokeThickness / scaleTransformShape.ScaleX;
            //var p = GetCenter();
            //scaleTransformShape.CenterX = p.X;
            //scaleTransformShape.CenterY = p.Y;

            //TextUpdate();
        }
    }
}
