using Emgu.CV;
using Emgu.CV.CvEnum;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace TestsSystems_HardnessTester
{
    public partial class MainWindow
    {
        private VideoCapture capture;

        private void ProcessFrame(object sender, EventArgs e)
        {
            Mat frame = capture.QueryFrame();
            BitmapImage btm = ImageConverter.Bitmap2BitmappImage(frame.ToBitmap());
            btm.Freeze();//Ах, нет ничего лучше старого доброго расплывчатого и
                         //таинственного трюка для решения чего-то, чего никто не понимает. 
                         //–Эдвин 4 марта 2017 в 10:43 https://translated.turbopages.org/proxy_u/en-ru.ru.bff4b66b-64635e39-13818cc7-74722d776562/https/stackoverflow.com/questions/9732709/the-calling-thread-cannot-access-this-object-because-a-different-thread-owns-it#comment72321173_33917169
            DisplayImage(btm);


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
                sreenImage.Source = Img;

        }

        private void BtmVideoStopStart_Click(object sender, RoutedEventArgs e)
        {
            if (capture == null || (!capture.IsOpened))
                return;

            if (CaptureInProgress)
            {
                capture.Pause();
                CaptureInProgress = false;
                ((Button)sender).Content = open_mes;
            }
            else
            {
                SetCaptureСontainer(capture.Width, capture.Height);

                capture.Start();
                CaptureInProgress = true;
                ((Button)sender).Content = close_mes;
            }

        }
        readonly string open_mes = $"Режим \n  видео";
        readonly string close_mes = $"     Сделать\n  фотографию";
        private void VideoStop()
        {
            if (CaptureInProgress)
            {
                capture.Pause();
                btmVideoStopStart.Content = open_mes;
                CaptureInProgress = false;
            }

        }

        private bool CaptureInProgress { get; set; }
        private void BtmConnectToCamer_Click(object sender, RoutedEventArgs e)
        {
            if (capture != null)
            {
                capture.Dispose();
                capture = null;
            }
            capture = new VideoCapture();
            if (capture.IsOpened)
            {

                SetCaptureСontainer(capture.Width, capture.Height);
                btmVideoStopStart.Content = close_mes;
                CaptureInProgress = true;
                capture.ImageGrabbed += ProcessFrame;
                capture.Start();
            }
        }
    }
}
