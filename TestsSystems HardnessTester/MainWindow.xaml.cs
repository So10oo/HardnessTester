﻿using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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
            MeasuringShape.CoefficientPxtomm = testing.CoefficientPxtomm;
 

            path = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "Settings", "CaptureSettings.json");
            if (captureSettingsValue.Read(path) is CaptureSettingsValue cs)
                captureSettingsValue = cs;
          
            if (captureSettingsValue != null && programSettings != null)
                MessageBox.Show("Разрешение камеры :" + captureSettingsValue.Width.ToString() + "x" + captureSettingsValue.Height.ToString() +
                    "\nКоэфициент пересчёта : " + programSettings.CoefficientPxtomm.ToString("N6"));

            #endregion

        }

        protected override void OnClosing(CancelEventArgs e)
        {
            //if(TaskConectCamera!=null)
            //    TaskConectCamera.Wait();

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

        #region вспомогательные функции 
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

        private void TxtMessageCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var temp = (TextBlock)sender;
            Canvas.SetLeft(temp, (drawingСanvas.ActualWidth - temp.ActualWidth) / 2.0);
            Canvas.SetTop(temp, (drawingСanvas.ActualHeight - temp.ActualHeight) / 2.0);
        }
        #endregion

        #region Кнопки на панеле 

        private void BtmOpenImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog()
            {
                Filter = "Image Files(*.jpg; *.jpeg; *.gif; *.bmp)|*.jpg; *.jpeg; *.gif; *.bmp"
            };

            VideoStop();//останавливаем видео если оно запущено
            if (ofd.ShowDialog() == true)
            {
                // VideoStop();//останавливаем видео если оно запущено
                drawingСanvas.ClearShapes();//удаляем фигуры если они есть на панели 
                Bitmap bitmap = new Bitmap(ofd.FileName);
                SetCaptureСontainer(bitmap.Width, bitmap.Height);
                sreenImage.Source = ImageConverter.Bitmap2BitmappImage(bitmap);
            }
            else
            {
                VideoStart();
            }
        }

        private void BtmFindShape_Click(object sender, RoutedEventArgs e)
        {
            VideoStop();//выключить видео если оно воспроизводится 
            if (sreenImage.Source == null)
            {
                txtMessageCanvas.FontSize = Math.Min(drawingСanvas.ActualHeight, drawingСanvas.ActualWidth) * 0.04;
                txtMessageCanvas.Foreground = new SolidColorBrush(Colors.Red);
                txtMessageCanvas.BeginAnimation(TextBlock.TextProperty, Animations.CreatStrindAnimation("Изображение не было найдено!", 0, 2.5));
                return;
            }

            Emgu.CV.Mat mat = ImageConverter.BitmapImage2Bitmap((BitmapImage)sreenImage.Source).ToMat();
            Emgu.CV.CvInvoke.CvtColor(mat, mat, Emgu.CV.CvEnum.ColorConversion.Bgr2Gray);
            #region поиск фигуры и отрисовка
            try
            {

                if (tabItemVikkers.IsSelected)
                {
                    var taskSquare = new Task<(RotatedRect, double)>(() =>
                    {
                        var sq1 = FindShape.FindSquare_v1(mat);
                        return sq1;
                    });
                    Task _taskSquare = taskSquare.ContinueWith(t =>
                    {
                        var r = t.Result.Item1;
                        Dispatcher.Invoke(() =>
                        {
                            drawingСanvas.PaintSquare(r.Size.Width, r.Angle, r.Center.X, r.Center.Y);
                        });
                    });
                    taskSquare.Start();

                }
                else if (tabItemBrinel.IsSelected)
                {
                    var taskCircle = new Task<(CircleF, double)>(() =>
                    {
                        var cl1 = FindShape.FindCircle_v3(mat);
                        return cl1;
                    });  
                    Task _taskCircle = taskCircle.ContinueWith(t =>
                    {
                        var c = t.Result.Item1;
                        Dispatcher.Invoke(() =>
                        {
                            drawingСanvas.PaintCircle(c.Radius, c.Center.X, c.Center.Y);
                        });
                    });
                    taskCircle.Start();
                }

                //Task taskFind = new Task(() =>
                //{
                //    #region находим окружность 
                //    var taskCircle = new Task<(CircleF, double)>(() =>
                //    {
                //        var cl1 = FindShape.FindCircle_v3(mat);
                //        return cl1;
                //    });
                //    taskCircle.Start();
                //    #endregion
                //    taskCircle.Wait();
                //    #region рисуем круг
                //    var c = taskCircle.Result.Item1;
                //    Dispatcher.Invoke(() =>
                //    {
                //        drawingСanvas.PaintCircle(c.Radius, c.Center.X, c.Center.Y);
                //    });
                //    #endregion

                //});
                //taskFind.Start();

                //if (tabItemVikkers.IsSelected)
                //{
                //    var r = FindShape.FindSqr(mat);
                //    drawingСanvas.PaintSquare(r.Size.Width, r.Angle, r.Center.X, r.Center.Y);
                //}
                //else if (tabItemBrinel.IsSelected)
                //{
                //    var c = FindShape.FindCirl2(mat);
                //    drawingСanvas.PaintCircle(c.Radius, c.Center.X, c.Center.Y);
                //}
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
                if (b == 0)
                    throw new Exception("Не найдена фигура!");

                testing.CoefficientPxtomm = a / b;

                programSettings.CoefficientPxtomm = testing.CoefficientPxtomm;
                MeasuringShape.CoefficientPxtomm = testing.CoefficientPxtomm;

                //вместо этого надо будет пройтись по всем фигурам 
                //InvalidateVisual()
                drawingСanvas.InvalidateVisualShapes();

                string path = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "Settings", "ProgramSettings.json");
                programSettings.Save(path);


                messageСalibration.Foreground = new SolidColorBrush(Colors.LightGreen);
                messageСalibration.BeginAnimation(TextBlock.TextProperty,
                    Animations.CreatStrindAnimation("Успешно! Коэфициент пересчета равен " + testing.CoefficientPxtomm.ToString("N6") + " mm/px", 0, 5));
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                messageСalibration.Foreground = new SolidColorBrush(Colors.Red);
                messageСalibration.BeginAnimation(TextBlock.TextProperty,
                    Animations.CreatStrindAnimation("Ошибка! " + ex.Message, 0, 2));
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
            //var a = this.Icon;
            drawingСanvas.ClearShapes();
        }

        private void BtmCaptureSettings_Click(object sender, RoutedEventArgs e)
        {
            if (capture == null)
            {
                txtMessageCanvas.FontSize = Math.Min(drawingСanvas.ActualHeight, drawingСanvas.ActualWidth) * 0.04;
                txtMessageCanvas.Foreground = new SolidColorBrush(Colors.Red);
                txtMessageCanvas.BeginAnimation(TextBlock.TextProperty, Animations.CreatStrindAnimation("Камера не подключенна!", 0, 2.5));
                return;
            }

            Settings windowCaptureSettings =
                new Settings(capture, drawingСanvas, sreenImage, captureSettingsValue);
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
            if (tabItemVikkers.IsSelected)
            {
                testing.TestingMethod = "Виккерса";
                testing.ei = "HB";
            }
            else if (tabItemBrinel.IsSelected)
            {
                testing.TestingMethod = "Бринеля";
                testing.ei = "HV";
            }
            testing.CreateProtocol();
        }





        #endregion

        private void textBoxСalibration_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                BtmSetupСalibration_Click(null, null);
        }
    }

}
