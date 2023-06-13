using System;
using System.Drawing;

namespace TestsSystems_HardnessTester
{
    public class Test
    {
        public Test()
        {
            DateTimeTest = DateTime.Now;
        }
        public Bitmap Image { get; set; }
        public uint SnapshotNumber { get; set; } = 1;//норме отпечатка
        public string SampleName { get; set; } = "test";//имя образца 
        public double HardnessTest { get; set; } = 0.0;//твёрдость
        public double SizeTest { get; set; } = 0.0;//размер при тестировании 
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
