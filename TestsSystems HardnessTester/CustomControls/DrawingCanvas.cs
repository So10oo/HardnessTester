using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;


namespace TestsSystems_HardnessTester
{
    public class DrawingCanvas : Canvas
    {
        internal event EventHandler DrawingShape;
        internal event EventHandler MoveShape;
        internal event SizeChangedEventHandler SharpSizeChanged;
        private Shape shape;
        private Point startPoint = new Point();
        private Point endPoint = new Point();

        public DrawingCanvas() : base()
        {
            this.MouseMove += DrawingCanvas_MouseMove;
            this.MouseLeftButtonDown += DrawingCanvas_MouseLeftButtonDown;
            this.MouseLeftButtonUp += DrawingCanvas_MouseLeftButtonUp;
            this.MouseRightButtonDown += DrawingCanvas_MouseRightButtonDown;
            this.MouseWheel += DrawingCanvas_MouseWheel;
        }

        #region Events Canvas
        private void DrawingCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.RightCtrl) || Keyboard.IsKeyDown(Key.LeftCtrl))
                return;
            else
                e.Handled = true;


            startPoint = e.GetPosition(this);
            var _shape = VisualTreeHelper.HitTest(this, startPoint).VisualHit;

            if (!(_shape is Shape))
            {
                DrawingShape?.Invoke(this, new EventArgs());
                switch (CurrentShape)
                {
                    case SelectedShape.Square:
                        {
                            shape = CreatRect();
                            break;
                        }
                    case SelectedShape.Circle:
                        {
                            shape = CreatCircle();
                            break;
                        }
                    case SelectedShape.Line:
                        {
                            shape = CreatLine();
                            break;
                        }
                    default: return;

                }
                shape.SizeChanged += SharpSizeChanged;
                SetLeft(shape, startPoint.X);
                SetTop(shape, startPoint.Y);
                this.Children.Add(shape);
                ActionsStatus = Actions.Posting;
            }
            else
            {
                ActionsStatus = Actions.Move;
                MoveShape?.Invoke(this, new EventArgs());
                if (_shape is Rectangle rect)
                    shape = rect;
                else if (_shape is Ellipse elip)
                    shape = elip;
                //SharpSizeChanged(shape, null);
                SharpSizeChanged?.Invoke(shape, null);
            }
        }

        private void DrawingCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.RightCtrl) || Keyboard.IsKeyDown(Key.LeftCtrl))
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
                                        double sideLength = diagonalLength / Math.Sqrt(2);
                                        shape.Height = sideLength;
                                        shape.Width = sideLength;
                                        var transformGroup = (TransformGroup)shape.RenderTransform;
                                        ((RotateTransform)transformGroup.Children[0]).CenterX = sideLength / 2.0;
                                        ((RotateTransform)transformGroup.Children[0]).CenterY = sideLength / 2.0;
                                        ((RotateTransform)transformGroup.Children[2]).Angle = angle - 45;
                                        break;
                                    }
                                case SelectedShape.Circle:
                                    {
                                        var delta = diagonalLength / 2.0 * (1.0 - 1.0 / Math.Sqrt(2));//линейное расстояние между углом
                                                                                                      //ограничевающего квадрата и окружностью
                                        var transformGroup = (TransformGroup)shape.RenderTransform;
                                        ((RotateTransform)transformGroup.Children[1]).CenterX = delta;
                                        ((RotateTransform)transformGroup.Children[1]).CenterY = delta;
                                        ((RotateTransform)transformGroup.Children[1]).Angle = angle - 45.0;
                                        shape.Height = diagonalLength;
                                        shape.Width = diagonalLength;
                                        Canvas.SetLeft(shape, startPoint.X - delta);
                                        Canvas.SetTop(shape, startPoint.Y - delta);
                                        break;
                                    }
                                case SelectedShape.Line:
                                    {
                                        if (shape is Line line)
                                        {
                                            line.X2 = endPoint.X - startPoint.X;
                                            line.Y2 = endPoint.Y - startPoint.Y;
                                        }
                                        break;
                                    }
                            }
                            break;
                        }
                    case Actions.Move:
                        {
                            TranslateTransform transform = null;
                            double deltaX = endPoint.X - startPoint.X;
                            double deltaY = endPoint.Y - startPoint.Y;
                            if (shape is Rectangle)
                                transform = (TranslateTransform)((TransformGroup)shape.RenderTransform).Children[3];
                            else if (shape is Ellipse)
                                transform = (TranslateTransform)((TransformGroup)shape.RenderTransform).Children[2];
                            if (transform != null)
                            {
                                transform.X += deltaX;
                                transform.Y += deltaY;
                            }
                            startPoint = endPoint;
                            // SharpSizeChanged(shape, null);
                            SharpSizeChanged?.Invoke(shape, null);
                            break;
                        }
                }
            }
        }

        private void DrawingCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.RightCtrl) || Keyboard.IsKeyDown(Key.LeftCtrl))
                return;
            else
                e.Handled = true;

            Point pt = e.GetPosition(this);

            var k = increaseSize * e.Delta / Math.Abs(e.Delta);
            HitTestResult result = VisualTreeHelper.HitTest(this, pt);
            if (result.VisualHit is Rectangle rect)
            {
                TransformGroup transformGroup = (TransformGroup)rect.RenderTransform;
                var angle = ((RotateTransform)(transformGroup.Children[0])).Angle;
                Point center = new Point(rect.Width / 2.0, rect.Height / 2.0);
                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                {
                    angle += k;
                    RotateTransform rotateTransform1 = new RotateTransform(angle, center.X, center.Y);
                    transformGroup.Children[0] = rotateTransform1;
                    rect.RenderTransform = transformGroup;
                }
                else
                {
                    ScaleTransform transformScaleTransform = (ScaleTransform)transformGroup.Children[1];
                    transformScaleTransform.CenterX = center.X;
                    transformScaleTransform.CenterY = center.Y;
                    transformScaleTransform.ScaleX += k / rect.Width;
                    transformScaleTransform.ScaleY += k / rect.Width;
                    rect.StrokeThickness = strokeThickness / transformScaleTransform.ScaleX;
                    //SharpSizeChanged(rect, null);
                    SharpSizeChanged?.Invoke(rect, null);
                }
            }
            if (result.VisualHit is Ellipse elip)
            {
                if (!(Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)))
                {
                    TransformGroup transformGroup = (TransformGroup)elip.RenderTransform;
                    ScaleTransform transformScaleTransform = (ScaleTransform)transformGroup.Children[0];
                    transformScaleTransform.CenterX = elip.Width / 2.0;
                    transformScaleTransform.CenterY = elip.Width / 2.0;
                    transformScaleTransform.ScaleX += k / elip.Width;
                    transformScaleTransform.ScaleY += k / elip.Width;
                    elip.StrokeThickness = strokeThickness / transformScaleTransform.ScaleX;
                    //SharpSizeChanged(elip, null);
                    SharpSizeChanged?.Invoke(elip, null);
                }

            }

        }

        private void DrawingCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.RightCtrl) || Keyboard.IsKeyDown(Key.LeftCtrl))
                return;
            else
                e.Handled = true;

            if (shape != null && (shape.Width < 10 || Double.IsNaN(shape.Width)))
                if (!(shape is Line))
                    this.Children.Remove(shape);

            ActionsStatus = Actions.None;
        }

        private void DrawingCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.RightCtrl) || Keyboard.IsKeyDown(Key.LeftCtrl))
                return;
            else
                e.Handled = true;

            Point pt = e.GetPosition(this);
            HitTestResult result = VisualTreeHelper.HitTest(this, pt);

            if (result != null)
                this.Children.Remove(result.VisualHit as Shape);
        }

        #endregion

        #region functions
        internal void ClearShapes()
        {
            for (int i = this.Children.Count - 1; i >= 0; i--)
                if (this.Children[i] is Shape _shape)
                    this.Children.Remove(_shape);
        }

        #region дописывать
        //public void ConfigureSettingsShapes(double strokeThickness, SolidColorBrush color, double increaseSize)
        //{
        //    foreach (var _shape in this.Children)
        //    {
        //        if (_shape is Shape s)
        //            var (sT, c, iS) = GetShapeSettings(s);
        //        //надо  дописывать 
        //    }
        //}

        //private (double, SolidColorBrush, double) GetShapeSettings(Shape _shape)
        //{
        //    double strokeThickness = 0;
        //    SolidColorBrush color = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
        //    double increaseSize = 0;
        //    TransformGroup transformGroup;
        //    ScaleTransform transformScaleTransform;
        //    if (_shape is Rectangle rect)
        //    {
        //        transformGroup = (TransformGroup)rect.RenderTransform;
        //        transformScaleTransform = (ScaleTransform)transformGroup.Children[1];
        //        strokeThickness = transformScaleTransform.ScaleX * this.strokeThickness;
        //        color = (SolidColorBrush)rect.Stroke;
        //        increaseSize = this.increaseSize;
        //    }
        //    if (_shape is Ellipse elip)
        //    {
        //        transformGroup = (TransformGroup)elip.RenderTransform;
        //        transformScaleTransform = (ScaleTransform)transformGroup.Children[0];
        //        strokeThickness = transformScaleTransform.ScaleX * this.strokeThickness;
        //        color = (SolidColorBrush)elip.Stroke;
        //        increaseSize = this.increaseSize;
        //    }

        //    return (strokeThickness, color, increaseSize);
        //}

        #endregion

        private Ellipse CreatCircle()
        {
            var elip = new Ellipse()
            {
                Fill = Brushes.Transparent,
                Stroke = strokeShape,
                StrokeThickness = strokeThickness,
                //Stretch = Stretch.Uniform
            };
            TransformGroup transformGroup = new TransformGroup();
            ScaleTransform transform0 = new ScaleTransform();
            RotateTransform transform1 = new RotateTransform();
            TranslateTransform transform2 = new TranslateTransform();
            transformGroup.Children.Add(transform0);
            transformGroup.Children.Add(transform1);
            transformGroup.Children.Add(transform2);
            elip.RenderTransform = transformGroup;
            return elip;
        }

        private Rectangle CreatRect()
        {
            var shape = new Rectangle()
            {
                Fill = Brushes.Transparent,
                Stroke = strokeShape,
                StrokeThickness = strokeThickness,
                //Stretch = Stretch.Uniform
            };
            TransformGroup transformGroup = new TransformGroup();
            RotateTransform transform0 = new RotateTransform();
            ScaleTransform transform1 = new ScaleTransform();
            RotateTransform transform2 = new RotateTransform();
            TranslateTransform transform3 = new TranslateTransform();
            transformGroup.Children.Add(transform0);
            transformGroup.Children.Add(transform1);
            transformGroup.Children.Add(transform2);
            transformGroup.Children.Add(transform3);
            shape.RenderTransform = transformGroup;
            return shape;
        }

        private Line CreatLine()
        {
            var shape = new Line()
            {
                Fill = Brushes.Transparent,
                Stroke = strokeShape,
                StrokeThickness = strokeThickness,
                X1 = 0,
                Y1 = 0,

            };
            return shape;
        }

        public double GetSizeShape()//(Shape shape)
        {
            double transformScale = 1;
            double width = 0;

            if (shape == null)
                throw new Exception("Нет отрисованной калиброванчной фигуры");

            if (shape is Rectangle rect)
            {
                transformScale = ((ScaleTransform)(((TransformGroup)rect.RenderTransform).Children[1])).ScaleX;
                width = rect.Width;
            }
            else if (shape is Ellipse elip)
            {
                transformScale = ((ScaleTransform)(((TransformGroup)elip.RenderTransform).Children[0])).ScaleX;
                width = elip.Width;
            }
            else if (shape is Line line)
            {
                transformScale = 1;
                width = Math.Sqrt((line.X2 - line.X1) * (line.X2 - line.X1) + (line.Y2 - line.Y1) * (line.Y2 - line.Y1));
            }

            return width * transformScale;
        }


        public void PaintCircle(double R, double X, double Y)
        {
            var _circle = CreatCircle();
            SetLeft(_circle, X - R);
            SetTop(_circle, Y - R);
            _circle.Width = 2 * R;
            _circle.Height = 2 * R;
            this.Children.Add(_circle);
        }
        internal void PaintSquare(float size, float angle, float X, float Y)
        {
            var _square = CreatRect();
            _square.Width = size;
            _square.Height = size;
            SetLeft(_square, X - size / 2.0);
            SetTop(_square, Y - size / 2.0);
            RotateTransform rt = (RotateTransform)(((TransformGroup)_square.RenderTransform).Children[0]);
            rt.Angle = angle ;
            rt.CenterX = size / 2.0;
            rt.CenterY = size / 2.0;
            //((RotateTransform)(((TransformGroup)_square.RenderTransform).Children[0])).Angle = angle;

            this.Children.Add(_square);
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
        }

        #endregion

        #region Shape settings
        readonly double strokeThickness = 2;//0.4
        readonly SolidColorBrush strokeShape = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
        readonly double increaseSize = 0.7;//0.2

        #endregion
    }
}
