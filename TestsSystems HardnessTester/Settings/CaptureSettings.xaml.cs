using Emgu.CV;
using Emgu.CV.CvEnum;
using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace TestsSystems_HardnessTester
{
    /// <summary>
    /// Логика взаимодействия для CaptureSettings.xaml
    /// </summary>
    public partial class WindowCaptureSettings : Window
    {
        readonly private VideoCapture capture;
        readonly private DrawingCanvas drawingCanvas;
        readonly private Image image;
        readonly private CaptureSettingsValue captureSettingsValue;

        public WindowCaptureSettings(VideoCapture capture, DrawingCanvas drawingCanvas, Image image, CaptureSettingsValue captureSettingsValue)
        {

            this.capture = capture;
            this.drawingCanvas = drawingCanvas;
            this.image = image;
            this.captureSettingsValue = captureSettingsValue;
            InitializeComponent();

            #region считываем настройки камеры 

            #region тестируем
            string path = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "Settings", "CaptureSettings.json");
            this.captureSettingsValue = captureSettingsValue.Read(path);
            SldBrightness.Value = captureSettingsValue.Brightness;
            SldContrast.Value = captureSettingsValue.Contrast;
            SlrSaturation.Value = captureSettingsValue.Saturation;
            SlrSharpness.Value = captureSettingsValue.Sharpness;
            SlrGamma.Value = captureSettingsValue.Gamma;
            #endregion




            foreach (var item in comboBoxResolution.Items)
            {
                if (item is TextBlock textBlock)
                    if (textBlock.Text == (capture.Width.ToString() + "x" + capture.Height.ToString()))
                    {
                        comboBoxResolution.SelectedItem = item;
                        break;
                    }
            }


            #endregion

        }


        private void SldBrightness_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var slider = (Slider)sender;
            double value = slider.Value;
            capture.Set(Emgu.CV.CvEnum.CapProp.Brightness, value);
            captureSettingsValue.Brightness = value;
        }

        private void SldContrast_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var slider = (Slider)sender;
            double value = slider.Value;
            capture.Set(Emgu.CV.CvEnum.CapProp.Contrast, value);
            captureSettingsValue.Contrast = value;
        }

        private void SlrSharpness_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var slider = (Slider)sender;
            double value = slider.Value;
            capture.Set(Emgu.CV.CvEnum.CapProp.Sharpness, value);
            captureSettingsValue.Sharpness = value;
        }

        private void SlrSaturation_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var slider = (Slider)sender;
            double value = slider.Value;
            capture.Set(Emgu.CV.CvEnum.CapProp.Saturation, value);
            captureSettingsValue.Saturation = value;
        }

        private void SlrGamma_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var slider = (Slider)sender;
            double value = slider.Value;

            if (value < 0.375)
                slider.Value = 0.35;
            else if (value < 0.425)
                slider.Value = 0.4;
            else if (value < 0.6)
                slider.Value = 0.45;
            else if (value < 0.635)
                slider.Value = 0.57;
            else if (value < 0.85)
                slider.Value = 0.7;
            else
                slider.Value = 1.0;

            capture.Set(Emgu.CV.CvEnum.CapProp.Gamma, value);
            captureSettingsValue.Gamma = value;
        }

        private void SlrGain_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (gainAuto) return;
            var slider = (Slider)sender;
            double value = slider.Value;
            capture.Set(Emgu.CV.CvEnum.CapProp.Gain, value);
            captureSettingsValue.Gain = value;
        }

        private void SlrExposure_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (exposureAuto) return;
            var slider = (Slider)sender;
            double value = slider.Value;
            capture.Set(Emgu.CV.CvEnum.CapProp.Exposure, value);
            captureSettingsValue.Exposure = value;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string wh = ((TextBlock)((ComboBox)sender).SelectedItem).Text;
            string[] wh2 = wh.Split('x');
            int w = int.Parse(wh2[0]);
            int h = int.Parse(wh2[1]);
            if (w != capture.Width || h != capture.Height)
            {
                capture.Set(CapProp.FrameWidth, w);
                capture.Set(CapProp.FrameHeight, h);
                captureSettingsValue.SetSize(capture.Width, capture.Height);
                image.Width = capture.Width; image.Height = capture.Height;
                drawingCanvas.Width = capture.Width; drawingCanvas.Height = capture.Height;
            }
        }

        private bool gainAuto = new bool();
        private void CBGain_Changed(object sender, RoutedEventArgs e)
        {
            var checkBox = (CheckBox)sender;
            if ((bool)checkBox.IsChecked)
            {
                capture.Set(CapProp.Gain, -10);
                gainAuto = true;
            }
            else
            {
                capture.Set(CapProp.Autofocus, 0);
                capture.Set(CapProp.Gain, SlrGain.Value);
                gainAuto = false;
            }
        }

        private bool exposureAuto = new bool();
        private void CBExposure_Changed(object sender, RoutedEventArgs e)
        {
            var checkBox = (CheckBox)sender;
            if ((bool)checkBox.IsChecked)
            {
                capture.Set(CapProp.AutoExposure, 1);
                exposureAuto = true;
            }
            else
            {
                capture.Set(CapProp.AutoExposure, 0);
                capture.Set(CapProp.Gain, SlrExposure.Value);
                exposureAuto = false;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            string path = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "Settings", "CaptureSettings.json");
            captureSettingsValue.Save(path);
        }
    }

    [Serializable]
    public class CaptureSettingsValue
    {
        public double Brightness { get; set; }
        public double Contrast { get; set; }
        public double Sharpness { get; set; }
        public double Saturation { get; set; }
        public double Gamma { get; set; }

        public double Gain { get; set; }
        public double Exposure { get; set; }
        public double GainAuto { get; set; }
        public double ExposureAuto { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public void SetSize(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public void Save(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    using (FileStream fs = new FileStream(path, FileMode.Create))
                    {
                        JsonSerializer.Serialize<CaptureSettingsValue>(fs, this);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Настройки камеры не записались : " + e.Message);
            }
        }

        public CaptureSettingsValue Read(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    using (FileStream fs = new FileStream(path, FileMode.Open))
                    {
                        return JsonSerializer.Deserialize<CaptureSettingsValue>(fs);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Настройки камеры не считались : " + e.Message);
            }
            return null;
        }
    }
}
