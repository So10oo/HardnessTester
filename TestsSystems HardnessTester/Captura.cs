using Emgu.CV;
using Emgu.CV.CvEnum;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TestsSystems_HardnessTester
{
    public partial class MainWindow
    {
        private VideoCapture capture;

        private void ProcessFrame(object sender, EventArgs e)
        {

            try
            {
                
                Mat frame = capture.QueryFrame();//в этот момент нельзя переподключать камеру
                Bitmap bitmap = frame.ToBitmap();
                BitmapImage btm = ImageConverter.Bitmap2BitmappImage(bitmap);
                btm.Freeze();//Ах, нет ничего лучше старого доброго расплывчатого и
                             //таинственного трюка для решения чего-то, чего никто не понимает. 
                             //–Эдвин 4 марта 2017 в 10:43 https://translated.turbopages.org/proxy_u/en-ru.ru.bff4b66b-64635e39-13818cc7-74722d776562/https/stackoverflow.com/questions/9732709/the-calling-thread-cannot-access-this-object-because-a-different-thread-owns-it#comment72321173_33917169
                DisplayImage(btm);

                frame.Dispose();
                bitmap.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private delegate void DisplayImageDelegate(BitmapImage Img);

        private void DisplayImage(BitmapImage Img)
        {
            if (!this.Dispatcher.CheckAccess())
            {
                try
                {
                    DisplayImageDelegate DI = new DisplayImageDelegate(DisplayImage);
                    this.Dispatcher.BeginInvoke(DI, new object[] { Img });
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
               //GC.Collect();
                sreenImage.Source = Img;
            }

        }

        private void BtmVideoStopStart_Click(object sender, RoutedEventArgs e)
        {
            if (capture == null || (!capture.IsOpened))
            {
                txtMessageCanvas.FontSize = Math.Min(drawingСanvas.ActualHeight, drawingСanvas.ActualWidth) * 0.04;
                txtMessageCanvas.Foreground = new SolidColorBrush(Colors.Red);
                txtMessageCanvas.BeginAnimation(TextBlock.TextProperty, Animations.CreatStrindAnimation("Камера не подключенна!", 0, 2.5));
                return;
            }

            if (CaptureInProgress)
            {
                capture.Pause();
                CaptureInProgress = false;
            }
            else
            {
                SetCaptureСontainer(capture.Width, capture.Height);
                capture.Start();
                CaptureInProgress = true;
            }

        }

        readonly string video = "Видео";
        readonly string photo = "Фото ";

        private void VideoStop()
        {
            if (CaptureInProgress)
            {
                capture.Pause();
                CaptureInProgress = false;
            }

        }
        private void VideoStart()
        {
            if (CaptureInProgress)
            {
                capture.Start();
                CaptureInProgress = true;
            }

        }

        private bool captureInProgress;
        private bool CaptureInProgress
        {
            get
            {
                return captureInProgress;
            }
            set
            {
                DrawingImage fotoDrawingImage;
                if (value)
                {   
                    fotoDrawingImage = (DrawingImage)Application.Current.Resources["FotoDrawingImage"];
                    btmVideoStopStart.Tag = fotoDrawingImage;
                    btmVideoStopStart.Content = photo;
                }
                else
                {
                    fotoDrawingImage = (DrawingImage)Application.Current.Resources["VideoDrawingImage"];
                    btmVideoStopStart.Tag = fotoDrawingImage;
                    btmVideoStopStart.Content = video;
                }
                captureInProgress = value;
            }
        }

       // Task TaskConectCamera;
        private void BtmConnectToCamer_Click(object sender, RoutedEventArgs e)
        {

            if (capture != null)
            {
                capture.ImageGrabbed -= ProcessFrame;
                capture.Dispose();
                capture = null;
            }
            capture = new VideoCapture();
            if (capture.IsOpened)
            {
                capture.Set(CapProp.FrameWidth, captureSettingsValue.Width);
                capture.Set(CapProp.FrameHeight, captureSettingsValue.Height);
                SetCaptureСontainer(capture.Width, capture.Height);
                CaptureInProgress = true;
                capture.ImageGrabbed += ProcessFrame;
                capture.Start();
            }
            else
            {
                capture.Dispose();
                capture = null;
                txtMessageCanvas.FontSize = Math.Min(drawingСanvas.ActualHeight, drawingСanvas.ActualWidth) * 0.04;
                txtMessageCanvas.Foreground = new SolidColorBrush(Colors.Red);
                txtMessageCanvas.BeginAnimation(TextBlock.TextProperty, Animations.CreatStrindAnimation("Не удалось подключиться к камере!", 0, 2.5));
            }

            #region подключение камеры в другом потоке
            //if (TaskConectCamera != null && TaskConectCamera.Status == TaskStatus.Running)
            //    return;

            //if (capture != null)
            //{
            //    capture.Dispose();
            //    capture = null;
            //}


            //TaskConectCamera = new Task(() =>
            //{
            //    var capture = new VideoCapture();
            //    //capture = captura;
            //    if (capture.IsOpened)
            //    {
            //        capture.ImageGrabbed += ProcessFrame;
            //        capture.Set(CapProp.FrameWidth, captureSettingsValue.Width);
            //        capture.Set(CapProp.FrameHeight, captureSettingsValue.Height);
            //        Dispatcher.Invoke(() =>
            //        {
            //            this.capture = capture;
            //            SetCaptureСontainer(this.capture.Width, this.capture.Height);
            //            CaptureInProgress = true;
            //            this.capture.Start();
            //        });
                     
            //    }
            //    else
            //    {
            //        Dispatcher.Invoke(() =>
            //        {
            //            txtMessageCanvas.FontSize = Math.Min(drawingСanvas.ActualHeight, drawingСanvas.ActualWidth) * 0.04;
            //            txtMessageCanvas.Foreground = new SolidColorBrush(Colors.Red);
            //            txtMessageCanvas.BeginAnimation(TextBlock.TextProperty, Animations.CreatStrindAnimation("Не удалось подключиться к камере!", 0, 2.5));
            //        });
                    
            //    }

            //});
            //TaskConectCamera.Start();
            #endregion
        }
    }
}
