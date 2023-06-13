using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;

namespace TestsSystems_HardnessTester.Style.Window
{
    partial class WindowStyle
    {
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            (sender as System.Windows.Window).StateChanged += Window_StateChanged;
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            System.Windows.Window window = (sender as System.Windows.Window);
            //Button maximizeCaptionButton = me.Template.FindName("MaxRestoreButton", me) as Button;
            if (window.Template.FindName("MaxRestoreButton", window) is Button maximizeCaptionButton)
            {
                maximizeCaptionButton.Content = window.WindowState == WindowState.Maximized ? "2" : "1";
            }
            if (window.Template.FindName("PART_Border", window) is Border border)
            {
                if (window.WindowState == WindowState.Maximized)
                    border.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#383838"));
                else
                    border.ClearValue(Border.BorderBrushProperty);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            var window = ((sender as FrameworkElement).TemplatedParent as System.Windows.Window);
            window.Close();
        }

        private void MaxRestoreButton_Click(object sender, RoutedEventArgs e)
        {
            var window = ((sender as FrameworkElement).TemplatedParent as System.Windows.Window);
            window.WindowState = (window.WindowState == WindowState.Normal) ? WindowState.Maximized : WindowState.Normal;

            //var x = window.Template.FindName("PART_Border", window) as Border;
            //if (window.WindowState == WindowState.Maximized)
            //{
            //    x.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#383838"));
            //}
            //else
            //    x.ClearValue(Border.BorderBrushProperty);
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            var window = ((sender as FrameworkElement).TemplatedParent as System.Windows.Window);
            window.WindowState = WindowState.Minimized;
        }


    }
}

