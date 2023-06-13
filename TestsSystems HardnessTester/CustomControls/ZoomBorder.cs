using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TestsSystems_HardnessTester
{
    public class ZoomBorder : Border
    {
        private UIElement child = null;
        private Point origin;
        private Point start;

        public BitmapSource GetImgBorder()
        {
            //var st = _border.ScaleXY;
            var border = (System.Windows.Controls.Border)this;

            // Создаем новый RenderTargetBitmap, используя размеры Border
            RenderTargetBitmap rtb =
                new RenderTargetBitmap((int)(border.ActualWidth/*/st.Item1*/), (int)(border.ActualHeight/*/st.Item2*/), 96, 96, PixelFormats.Pbgra32);

            // Рендерим содержимое Border в RenderTargetBitmap
            rtb.Render(border);

            // Создаем новый BitmapEncoder
            PngBitmapEncoder encoder = new PngBitmapEncoder();

            // Добавляем RenderTargetBitmap в BitmapEncoder
            encoder.Frames.Add(BitmapFrame.Create(rtb));

            // Получаем BitmapSource из кадра BitmapFrame
            BitmapSource bitmap = encoder.Frames[0];

            return bitmap;
        }

        private TranslateTransform GetTranslateTransform(UIElement element)
        {
            return (TranslateTransform)((TransformGroup)element.RenderTransform)
              .Children.First(tr => tr is TranslateTransform);
        }

        private ScaleTransform GetScaleTransform(UIElement element)
        {
            return (ScaleTransform)((TransformGroup)element.RenderTransform)
              .Children.First(tr => tr is ScaleTransform);
        }

        public override UIElement Child
        {
            get { return base.Child; }
            set
            {
                if (value != null && value != this.Child)
                    this.Initialize(value);
                base.Child = value;
            }
        }

        public void Initialize(UIElement element)
        {
            this.child = element;
            if (child != null)
            {
                TransformGroup group = new TransformGroup();
                ScaleTransform st = new ScaleTransform();
                group.Children.Add(st);
                TranslateTransform tt = new TranslateTransform();
                group.Children.Add(tt);
                child.RenderTransform = group;
                child.RenderTransformOrigin = new Point(0.0, 0.0);
                this.MouseWheel += Child_MouseWheel;
                this.MouseLeftButtonDown += Child_MouseLeftButtonDown;
                this.MouseLeftButtonUp += Child_MouseLeftButtonUp;
                this.MouseMove += Child_MouseMove;
                this.PreviewMouseRightButtonDown += new MouseButtonEventHandler(
                  Child_PreviewMouseRightButtonDown);
            }
        }

        public void Reset()
        {
            if ((Keyboard.IsKeyDown(Key.RightCtrl) || Keyboard.IsKeyDown(Key.LeftCtrl)) && child != null)
            {
                // reset zoom
                var st = GetScaleTransform(child);
                st.ScaleX = 1.0;
                st.ScaleY = 1.0;

                // reset pan
                var tt = GetTranslateTransform(child);
                tt.X = 0.0;
                tt.Y = 0.0;
            }
        }

        #region Child Events

        private void Child_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if ((Keyboard.IsKeyDown(Key.RightCtrl) || Keyboard.IsKeyDown(Key.LeftCtrl)) && child != null)
            {
                var st = GetScaleTransform(child);
                var tt = GetTranslateTransform(child);

                double zoom = e.Delta > 0 ? .2 : -.2;
                if (!(e.Delta > 0) && (st.ScaleX <= 1 || st.ScaleY <= 1))
                    return;

                Point relative = e.GetPosition(child);
                double absoluteX;
                double absoluteY;

                absoluteX = relative.X * st.ScaleX + tt.X;
                absoluteY = relative.Y * st.ScaleY + tt.Y;

                st.ScaleX += zoom;
                st.ScaleY += zoom;

                //tt.X = absoluteX - relative.X * st.ScaleX;
                //tt.Y = absoluteY - relative.Y * st.ScaleY;

                var ttX = absoluteX - relative.X * st.ScaleX;
                var ttY = absoluteY - relative.Y * st.ScaleY;

                #region Моя правка
                var w = ((FrameworkElement)child).Width;
                var h = ((FrameworkElement)child).Height;
                var ttXMax = w - w * st.ScaleX;
                var ttYMax = h - h * st.ScaleY;

                if (ttX > 0)
                    ttX = 0;
                else if (ttX < ttXMax)
                    ttX = ttXMax;

                if (ttY > 0)
                    ttY = 0;
                else if (ttY < ttYMax)
                    ttY = ttYMax;
                #endregion

                tt.X = ttX;
                tt.Y = ttY;
            }
        }

        private void Child_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if ((Keyboard.IsKeyDown(Key.RightCtrl) || Keyboard.IsKeyDown(Key.LeftCtrl)) && child != null)
            {
                var tt = GetTranslateTransform(child);
                start = e.GetPosition(this);
                origin = new Point(tt.X, tt.Y);
                this.Cursor = Cursors.Hand;
                child.CaptureMouse();
            }
        }

        private void Child_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (child != null)
            {
                child.ReleaseMouseCapture();
                this.Cursor = Cursors.Arrow;
            }
        }

        void Child_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Reset();
        }

        private void Child_MouseMove(object sender, MouseEventArgs e)
        {
            if (child != null)
            {
                if (child.IsMouseCaptured)
                {
                    var tt = GetTranslateTransform(child);
                    Vector v = start - e.GetPosition(this);

                    //tt.X = origin.X - v.X;
                    //tt.Y = origin.Y - v.Y;

                    #region Моя правка
                    var st = GetScaleTransform(child);
                    var ttX = origin.X - v.X;
                    var ttY = origin.Y - v.Y;
                    var w = ((Canvas)child).Width;
                    var h = ((Canvas)child).Height;

                    if (ttX < 0 && ttX > (w - w * st.ScaleX))
                        tt.X = ttX;
                    if (ttY < 0 && ttY > (h - h * st.ScaleY))
                        tt.Y = ttY;
                    #endregion
                }
            }
        }

        #endregion
    }
}
