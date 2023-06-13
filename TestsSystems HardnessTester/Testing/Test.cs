using System;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;

namespace TestsSystems_HardnessTester
{
    public class Test
    {
        private double sizeTest = 0.0;

        public Test()
        {
            DateTimeTest = DateTime.Now;
            CheckBoxTest = new CheckBox();
            CheckBoxTest.Style = (System.Windows.Style)Application.Current.Resources["CheckBoxStyle"];
            CheckBoxTest.VerticalAlignment = VerticalAlignment.Center;
            //CheckBox.HorizontalAlignment = HorizontalAlignment.Center;
            CheckBoxTest.Margin = new Thickness(5);
            CheckBoxTest.IsChecked = true;

        }
        public CheckBox CheckBoxTest { get; set; }
        public Bitmap Image { get; set; }
        public uint SnapshotNumber { get; set; } = 1;//норме отпечатка
        public string SampleName { get; set; } = "test";//имя образца 
        public double HardnessTest { get; set; } = 0.0;//твёрдость
        public double SizeTest
        {
            get => sizeTest;
            set
            {
                CheckBoxTest.Content = value.ToString("N4");
                sizeTest = value;
            }
        }
        public DateTime DateTimeTest { get; set; }
        public string Disassembly { get; set; } = "норм"; //разбраковка
        public string DataTameNow() => DateTimeTest.ToString("dd.MM.yyyy hh:mm:ss");
        public TypeOfTest TypeofTest { get; set; }
        public enum TypeOfTest
        {
            Brinel,
            Vikkers,
            СircleСalibration,
            LineCalibration
        }
    }
}
