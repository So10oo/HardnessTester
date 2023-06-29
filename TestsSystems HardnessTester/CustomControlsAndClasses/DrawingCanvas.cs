using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace TestsSystems_HardnessTester
{
    public class DrawingCanvas : Canvas
    {

        MeasuringShape currentMeasuringShape;
        private MeasuringShape CurrentMeasuringShape
        {
            get 
            { 
                return currentMeasuringShape;
            }
            set 
            {
                if (currentMeasuringShape != null)
                {
                    currentMeasuringShape.Stroke = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                    currentMeasuringShape.TextСolor = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                }
                currentMeasuringShape= value;             
                currentMeasuringShape.Stroke = new SolidColorBrush(Color.FromRgb(0, 255, 0));
                currentMeasuringShape.TextСolor = new SolidColorBrush(Color.FromRgb(0, 255, 0));
            }
        }
        private Point startPoint = new Point();
        private Point endPoint = new Point();
        private readonly List<MeasuringShape> shapes = new List <MeasuringShape>();

        #region Events Canvas

        private void DrawingCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (IsKeyDownScaleDC())
                return;
            else
                e.Handled = true;

            startPoint = e.GetPosition(this);
            var _shape = VisualTreeHelper.HitTest(this, startPoint);
      
            if (!(_shape.VisualHit is MeasuringShape))
            {
                //var min = Math.Min(this.ActualHeight, this.ActualHeight);
                switch (CurrentShape)
                {
                    case SelectedShape.Square:
                        {
                            CurrentMeasuringShape = CreatShape<MeasuringSquare>(startPoint);
                            break;
                        }
                    case SelectedShape.Circle:
                        {
                            CurrentMeasuringShape = CreatShape<MeasuringСircle>(startPoint);
                            break;
                        }
                    case SelectedShape.Line:
                        {
                            CurrentMeasuringShape = CreatShape<MeasuringLine>(startPoint);
 
                            break;
                        }
                    default: return;

                }
                ActionsStatus = Actions.Posting;
            }
            else
            {
                ActionsStatus = Actions.Move;
                CurrentMeasuringShape = _shape.VisualHit as MeasuringShape;
            }
        }

        private void DrawingCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (IsKeyDownScaleDC())
                return;
            else
                e.Handled = true;

            if (e.LeftButton == MouseButtonState.Pressed && CurrentMeasuringShape != null)
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
                                        (CurrentMeasuringShape as MeasuringSquare)?.Posting(diagonalLength, angle);
                                        break;
                                    }
                                case SelectedShape.Circle:
                                    {
                                        (CurrentMeasuringShape as MeasuringСircle)?.Posting(startPoint.X, startPoint.Y, diagonalLength, angle);
                                        break;
                                    }
                                case SelectedShape.Line:
                                    {
                                        (CurrentMeasuringShape as MeasuringLine)?.Posting(endPoint.X - startPoint.X, endPoint.Y - startPoint.Y);
                                        break;
                                    }
                            }
                            break;
                        }
                    case Actions.Move:
                        {
                            double deltaX = endPoint.X - startPoint.X;
                            double deltaY = endPoint.Y - startPoint.Y;
                            if (CurrentMeasuringShape is MeasuringSquare s)
                                s.Move(deltaX, deltaY);
                            else if (CurrentMeasuringShape is MeasuringLine l)
                                l.Move(startPoint.X, startPoint.Y, deltaX, deltaY);
                            else if (CurrentMeasuringShape is MeasuringСircle c)
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

            //if (CurrentMeasuringShape != null && (CurrentMeasuringShape.SizeShape < 10))//если фигура маленькая то она удаляется 
            //{
            //    this.Children.Remove(CurrentMeasuringShape);
            //    CurrentMeasuringShape = null;
            //}

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

            if (result != null && result.VisualHit is MeasuringShape s)
            {
                this.Children.Remove(s);
                shapes.Remove(s);
                currentMeasuringShape = null;
            }

        }

        private void DrawingCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ClearShapes();
        }
        #endregion

        #region piblic functions

        public void InvalidateVisualShapes()
        {
            foreach (var item in shapes)
            {
                item.InvalidateVisual();
            }
        }

        public void ClearShapes()
        {
            for (int i = this.Children.Count - 1; i >= 0; i--)
                if (this.Children[i] is MeasuringShape _shape)
                {
                    this.Children.Remove(_shape);
                    shapes.Remove(_shape);
                }
        }


        public double GetSizeShape()//(Shape shape)
        {
            if(CurrentMeasuringShape== null) return 0;
            return CurrentMeasuringShape.SizeShape;
        }

        public void PaintCircle(double R, double X, double Y)
        {
            var _circle = CreatShape<MeasuringСircle>(new Point(X - R, Y - R));
            _circle.Posting(X, Y + R, 2 * R, -90);
            CurrentMeasuringShape= _circle;
        }

        public void PaintSquare(float width, float angle, float x, float y)
        {
            throw new NotImplementedException();
        }

        public SelectedShape GetTypeShape()
        {
            if (CurrentMeasuringShape is MeasuringСircle)
                return SelectedShape.Circle;
            else if (CurrentMeasuringShape is MeasuringSquare)
                return SelectedShape.Square;
            else if (CurrentMeasuringShape is MeasuringLine)
                return SelectedShape.Line;
            else return SelectedShape.None;
        }

        #endregion

        #region private functions 
        private T CreatShape<T>(Point p) where T : MeasuringShape, new()
        {
            var size = Math.Min(this.ActualHeight, this.ActualHeight);
            var shape = new T()
            {
                TextSize = size * 0.04,
                StrokeThickness = size * 0.002,
            };
            if(shape is MeasuringLine l)
                l.EndSize = size * 0.02;
            Canvas.SetLeft(shape, p.X);
            Canvas.SetTop(shape, p.Y);
            this.Children.Add(shape);
            shapes.Add(shape);
            return shape;
        }
        private bool IsKeyDownRotation() => Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
        private bool IsKeyDownScaleDC() => Keyboard.IsKeyDown(Key.RightCtrl) || Keyboard.IsKeyDown(Key.LeftCtrl);
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

        #region конструктор
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
        #endregion
    }


}
