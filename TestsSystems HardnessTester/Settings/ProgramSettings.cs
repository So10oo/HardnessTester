using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace TestsSystems_HardnessTester
{
    [Serializable]
    public class ProgramSettings
    {
        public double CoefficientPxtomm { get; set; }
        public void Save(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    using (FileStream fs = new FileStream(path, FileMode.Create))
                    {
                        JsonSerializer.Serialize<ProgramSettings>(fs, this);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Настройки камеры не записались : " + e.Message);
            }
        }

        public ProgramSettings Read(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    using (FileStream fs = new FileStream(path, FileMode.Open))
                    {
                        return JsonSerializer.Deserialize<ProgramSettings>(fs);
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
