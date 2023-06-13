﻿using Emgu.CV;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Text.Json;
using System.IO;
using static Emgu.CV.Dai.OpenVino;
using System.Linq;

namespace TestsSystems_HardnessTester
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //настройки камеры
        readonly CaptureSettingsValue captureSettingsValue = new CaptureSettingsValue();
        //настройки программы 
        readonly ProgramSettings programSettings = new ProgramSettings();
        //Серия испытаний-тестирование 
        readonly Testing testing = new Testing();



        public MainWindow()
        {
            InitializeComponent();
            //this.WindowState = WindowState.Maximized;



            #region считываем настройки 
            string path = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "Settings", "ProgramSettings.json");
            if (programSettings.Read(path) is ProgramSettings ps)
                programSettings = ps;
            testing.CoefficientPxtomm = programSettings.CoefficientPxtomm;

            path = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "Settings", "CaptureSettings.json");
            if (captureSettingsValue.Read(path) is CaptureSettingsValue cs)
                captureSettingsValue = cs;
          
            if (captureSettingsValue != null && programSettings != null)
                MessageBox.Show("Разрешение камеры :" + captureSettingsValue.Width.ToString() + "x" + captureSettingsValue.Height.ToString() +
                    "\nКоэфициент пересчёта : " + programSettings.CoefficientPxtomm.ToString("N6"));

            #endregion



            //добавляем обработчик к изменению размеры фигуры
            drawingСanvas.SharpSizeChanged += DrawingСanvas_SharpSizeChanged;

        }

        private void DrawingСanvas_SharpSizeChanged(object sender, SizeChangedEventArgs e)
        {
            txtBrinelSize.Text = "d = " + (drawingСanvas.GetSizeShape() * testing.CoefficientPxtomm).ToString("N5") + " mm";
            txtVikkersSize.Text = txtBrinelSize.Text;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            //логика связанная с настройками
            //string jsonString = JsonSerializer.Serialize<ProgramSettings>(programSettings);
            string path = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "Settings", "ProgramSettings.json");
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                if (programSettings != null)
                    JsonSerializer.Serialize<ProgramSettings>(fs, programSettings);
            }

            //логика связанная с камерой 
            capture?.Dispose();

            e.Cancel = false;
        }


        #region Выбор фигуры
        private void TabControl_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) => SetupTypeShape();

        private void LineСalibration_Checked(object sender, RoutedEventArgs e) => SetupTypeShape();

        private void CirclesСalibration_Checked(object sender, RoutedEventArgs e) => SetupTypeShape();

        private void SetupTypeShape()
        {
            if (tabItemVikkers.IsSelected)
                drawingСanvas.CurrentShape = DrawingCanvas.SelectedShape.Square;
            if (tabItemBrinel.IsSelected)
                drawingСanvas.CurrentShape = DrawingCanvas.SelectedShape.Circle;
            if (tabItemСalibration != null && tabItemСalibration.IsSelected)
            {
                if (lineСalibration.IsChecked == true)
                    drawingСanvas.CurrentShape = DrawingCanvas.SelectedShape.Line;
                if (circlesСalibration.IsChecked == true)
                    drawingСanvas.CurrentShape = DrawingCanvas.SelectedShape.Circle;
            }
        }

        #endregion 


        void SetCaptureСontainer(double Width, double Height)
        {
            if (drawingСanvas.Width != Width || drawingСanvas.Height != Height || sreenImage.Height != Height || sreenImage.Width != Width)
            {
                drawingСanvas.Width = Width;
                drawingСanvas.Height = Height;
                sreenImage.Width = Width;
                sreenImage.Height = Height;
            }
        }

        private void TextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            var textBox = (TextBox)(sender);
            if (!(Char.IsDigit(e.Text, 0) || (e.Text == ".")
               && (!textBox.Text.Contains(".")
               && textBox.Text.Length != 0)))
            {
                e.Handled = true;
            }
        }

        private void TestWHwindow_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(mainWin.Width.ToString() + "   " + mainWin.Height.ToString());
        }


        #region Кнопки на панеле 

        private void BtmOpenImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog()
            {
                Filter = "Image Files(*.jpg; *.jpeg; *.gif; *.bmp)|*.jpg; *.jpeg; *.gif; *.bmp"
            };

            if (ofd.ShowDialog() == true)
            {
                VideoStop();//останавливаем видео если оно запущено
                drawingСanvas.ClearShapes();//удаляем фигуры если они есть на панели 
                for (int i = drawingСanvas.Children.Count - 1; i >= 0; i--)
                    if (drawingСanvas.Children[i] is Shape _shape)
                        drawingСanvas.Children.Remove(_shape);

                Bitmap bitmap = new Bitmap(ofd.FileName);

                //drawingСanvas.Width = bitmap.Width;
                //drawingСanvas.Height = bitmap.Height;
                //sreenImage.Width = bitmap.Width;
                //sreenImage.Height = bitmap.Height;
                SetCaptureСontainer(bitmap.Width, bitmap.Height);

                sreenImage.Source = ImageConverter.Bitmap2BitmappImage(bitmap);
            }
        }

        private void BtmFindShape_Click(object sender, RoutedEventArgs e)
        {
            VideoStop();//выключить видео если оно воспроизводится 
            if (sreenImage.Source == null)
                return;


            Emgu.CV.Mat mat = ImageConverter.BitmapImage2Bitmap((BitmapImage)sreenImage.Source).ToMat();
            Emgu.CV.CvInvoke.CvtColor(mat, mat, Emgu.CV.CvEnum.ColorConversion.Bgr2Gray);
            #region поиск фигуры и отрисовка
            try
            {
                if (tabItemVikkers.IsSelected)
                {
                    var r = FindShape.FindSqr(mat);
                    drawingСanvas.PaintSquare(r.Size.Width, r.Angle, r.Center.X, r.Center.Y);

                }
                else if (tabItemBrinel.IsSelected)
                {
                    var c = FindShape.FindCirl(mat);
                    drawingСanvas.PaintCircle(c.Radius, c.Center.X, c.Center.Y);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            #endregion
        }

        private void BtmSetupСalibration_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var a = double.Parse(textBoxСalibration.Text);
                var b = drawingСanvas.GetSizeShape();
                testing.CoefficientPxtomm = a / b;
                if (programSettings != null)
                    programSettings.CoefficientPxtomm = testing.CoefficientPxtomm;
                MessageBox.Show("коэфициент пересчета равен " + testing.CoefficientPxtomm.ToString("N6"));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }



        private void BtmSaveImg_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();

            if (sfd.ShowDialog() == true)
            {
                if (sfd.FileName.Substring(sfd.FileName.Length - 4) != ".bmp")
                    sfd.FileName += ".bmp";
                ImageConverter.BitmapSource2Bitmap(border.GetImgBorder()).Save(sfd.FileName, ImageFormat.Bmp);
            }
        }

        private void BtmClearShape_Click(object sender, RoutedEventArgs e)
        {
            drawingСanvas.ClearShapes();
        }

        private void BtmCaptureSettings_Click(object sender, RoutedEventArgs e)
        {
            if (capture == null)
                return;

            WindowCaptureSettings windowCaptureSettings =
                new WindowCaptureSettings(capture, drawingСanvas, sreenImage, captureSettingsValue);
            windowCaptureSettings.ShowDialog();
            //чтобы окончательно остановить процесс(почему-то после закрытия главного окна программа не выходит из отладки)
            windowCaptureSettings.Close();
        }

        private void BtmAddBrineltest_Click(object sender, RoutedEventArgs e)
        {
            if (drawingСanvas.GetTypeShape() != DrawingCanvas.SelectedShape.Circle)
                return;
            try
            {
                Test test = new Test()
                {
                    SizeTest = drawingСanvas.GetSizeShape() * testing.CoefficientPxtomm,
                    Image = ImageConverter.BitmapSource2Bitmap(border.GetImgBorder()),
                    DateTimeTest = DateTime.Now,
                    SnapshotNumber = testing.GetCountTests() + 1,
                    TypeofTest = Test.TypeOfTest.Brinel,
                };
                StackPanelBrinel.Children.Add(test.CheckBoxTest);
                testing.AddTest(test);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void BtmDeleteBrinelTests_Click(object sender, RoutedEventArgs e)
        {
            StackPanelBrinel.Children.Clear();
            var tests = testing.GetTests();
            for (int i = tests.Count - 1; i >= 0; i--)
            {
                var item = tests[i];
                if (item.TypeofTest == Test.TypeOfTest.Brinel)
                    tests.Remove(item);
            }
        }


        private void BtmDeleteVikkersTests_Click(object sender, RoutedEventArgs e)
        {
            StackPanelVikkers.Children.Clear();
            var tests = testing.GetTests();
            for (int i = tests.Count - 1; i >= 0; i--)
            {
                var item = tests[i];
                if (item.TypeofTest == Test.TypeOfTest.Vikkers)
                    tests.Remove(item);
            }
        }

        private void BtmAddVikkerstest_Click(object sender, RoutedEventArgs e)
        {
            if (drawingСanvas.GetTypeShape() != DrawingCanvas.SelectedShape.Square)
                return;
            try
            {
                Test test = new Test()
                {
                    SizeTest = drawingСanvas.GetSizeShape() * testing.CoefficientPxtomm,
                    Image = ImageConverter.BitmapSource2Bitmap(border.GetImgBorder()),
                    DateTimeTest = DateTime.Now,
                    SnapshotNumber = testing.GetCountTests() + 1,
                    TypeofTest = Test.TypeOfTest.Vikkers,
                };
                StackPanelVikkers.Children.Add(test.CheckBoxTest);
                testing.AddTest(test);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void BtmCreatReport_Click(object sender, RoutedEventArgs e)
        {
            testing.CreateProtocol();
        }

        #endregion

    }

}