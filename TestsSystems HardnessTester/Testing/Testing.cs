using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Xceed.Document.NET;
using Xceed.Words.NET;

namespace TestsSystems_HardnessTester
{
    internal class Testing
    {
        readonly private List<Test> tests = new List<Test>();
        public double CoefficientPxtomm { get; set; } = 1.0; //коэффициент пересчёта
        public uint ProtocolNumber { get; set; } = 1;
        private string TestingMethod { get; set; } = "Бринеля";//метод тестирования
        public string MetalGrade { get; set; } = "Test(040913)";//марка металла
        public string Gost { get; set; } = "гост,ту";//Гост Ту
        public string Melting { get; set; } = "№777";//плавка
        public string Load { get; set; } = "1000кгс";//нагрузка
        public string LoadApplicationTime { get; set; } = "10 с";//время приложения нагрузки 
        public string MinimumDisassembly { get; set; } = "100 HB";//минимум разбраковки
        public string MaximumDisassembly { get; set; } = "101 HB";//максимум разбраковки 
        public string Indenter { get; set; } = "Шарик 10 мм";//индентор
        public string NameHardnessTester { get; set; } = "Нет имени";
        public DateTime HardnessЕesterVerificationTime { get; set; } = new DateTime(2022, 9, 9);//дата поверки твердомера
        public string NameVisualizer { get; set; } = "Нет имени";
        public DateTime VisualizerVerificationTime { get; set; } = new DateTime(2022, 9, 9);//дата поверки визуализатора

        public string TesterName { get; set; } = "Иванов";

        public void AddTest(Test test)
        {
            tests.Add(test);
        }

        public List<Test> GetTests()
        {
            return tests;
        }
        public uint GetCountTests()
        {
            return (uint)tests.Count;
        }

        public void CreateProtocol()
        {
            string filePath = "export.docx";
            DocX document = DocX.Create(filePath);
            document.MarginTop = 24;//???

            Paragraph p1 = document.InsertParagraph();
            p1.Alignment = Alignment.center;
            p1.Append("ООО «Тестсистемы»").FontSize(12);
            p1.AppendLine("ПРОТОКОЛ ИСПЫТАНИЯ № " + ProtocolNumber.ToString()).FontSize(16).Bold();
            p1.AppendLine("");
            p1.AppendLine("Измерение твердости по методу " + TestingMethod).FontSize(12);

            var dateTime1 = HardnessЕesterVerificationTime.ToString("dd.MM.yyyy");
            var dateTime2 = VisualizerVerificationTime.ToString("dd.MM.yyyy");
            p1.AppendLine("Испытательное оборудование: " + NameHardnessTester + " зав. №1 (Дата поверки: " + dateTime1 + "),").FontSize(12);
            p1.AppendLine(NameVisualizer + " №2  (Дата поверки: " + dateTime2 + ").").FontSize(12);
            p1.AppendLine("");

            #region 1-я таблица  
            // создаём таблицу с 4 строками и 2 столбцами
            Table table1 = document.AddTable(4, 2);
            table1.Alignment = Alignment.center;
            table1.Design = TableDesign.None;
            table1.Rows[0].Cells[0].Paragraphs[0].Append("Марка металла:\t" + MetalGrade).FontSize(12);
            table1.Rows[0].Cells[1].Paragraphs[0].Append("Время приложения нагрузки:\t" + LoadApplicationTime).FontSize(12);
            table1.Rows[1].Cells[0].Paragraphs[0].Append("ГОСТ, ТУ:\t" + Gost).FontSize(12);
            table1.Rows[1].Cells[1].Paragraphs[0].Append("Минимум разбраковки:\t" + MinimumDisassembly).FontSize(12);
            table1.Rows[2].Cells[0].Paragraphs[0].Append("Плавка:\t" + Melting).FontSize(12);
            table1.Rows[2].Cells[1].Paragraphs[0].Append("Максимум разбраковки:\t" + MaximumDisassembly).FontSize(12);
            table1.Rows[3].Cells[0].Paragraphs[0].Append("Нагрузка:\t" + Load).FontSize(12);
            table1.Rows[3].Cells[1].Paragraphs[0].Append("Индентор:\t" + Indenter).FontSize(12);
            p1.InsertTableAfterSelf(table1);
            p1.AppendLine("");
            #endregion

            Paragraph p2 = document.InsertParagraph();
            p2.Alignment = Alignment.left;
            p2.AppendLine("Результаты серии испытаний:").FontSize(12);

            #region 2-я таблица
            var numberTests = 2;
            int columnCount = tests.Count;
            Table table2 = document.AddTable(1 + numberTests, columnCount);
            table2.SetWidthsPercentage(new[] { 12.9f, 7.4f, 35f, 15f, 15, 12.9f }, 470);


            table2.Rows[0].Cells[0].Paragraphs[0].Append("Образец").FontSize(12).Alignment = Alignment.center;
            table2.Rows[0].Cells[1].Paragraphs[0].Append("№").FontSize(12).Alignment = Alignment.center;
            table2.Rows[0].Cells[2].Paragraphs[0].Append("Дата").FontSize(12).Alignment = Alignment.center;
            table2.Rows[0].Cells[3].Paragraphs[0].Append("Твердость,HB").FontSize(12).Alignment = Alignment.center;
            table2.Rows[0].Cells[4].Paragraphs[0].Append("Диагональ,мм").FontSize(12).Alignment = Alignment.center;
            table2.Rows[0].Cells[5].Paragraphs[0].Append("Разборка").FontSize(12).Alignment = Alignment.center;

            #region надо будет менять (это тест)

            for (int i = 0; i < tests.Count; i++)
            {
                Test _test = tests[i];
                table2.Rows[i].Cells[0].Paragraphs[0].Append(_test.SampleName).FontSize(12).Alignment = Alignment.center;
                table2.Rows[i].Cells[1].Paragraphs[0].Append(_test.SnapshotNumber.ToString()).FontSize(12).Alignment = Alignment.center;
                table2.Rows[i].Cells[2].Paragraphs[0].Append(_test.DataTameNow()).FontSize(12).Alignment = Alignment.center;
                table2.Rows[i].Cells[3].Paragraphs[0].Append(_test.HardnessTest.ToString("N1")).FontSize(12).Alignment = Alignment.center;
                table2.Rows[i].Cells[4].Paragraphs[0].Append(_test.SizeTest.ToString("N1")).FontSize(12).Alignment = Alignment.center;
                table2.Rows[i].Cells[5].Paragraphs[0].Append(_test.Disassembly).FontSize(12).Alignment = Alignment.center;
            }

            #endregion

            p2.AppendLine("");
            p2.InsertTableAfterSelf(table2);
            #endregion


            Paragraph p3 = document.InsertParagraph();
            p3.Alignment = Alignment.center;

            #region 3-я таблица
            Table table3 = document.AddTable(3, 2);
            table3.Alignment = Alignment.center;
            table3.Design = TableDesign.None;
            table3.Rows[0].Cells[0].Paragraphs[0].Append("Минимальная твердость:\t 100,90 HB").FontSize(12);
            table3.Rows[0].Cells[1].Paragraphs[0].Append("Стандартное отклонение:\t 0,90").FontSize(12);
            table3.Rows[1].Cells[0].Paragraphs[0].Append("Максимальная твердость:\t 101,90 HB").FontSize(12);
            table3.Rows[1].Cells[1].Paragraphs[0].Append("Доверительный интервал:\t 1,90").FontSize(12);
            table3.Rows[2].Cells[0].Paragraphs[0].Append("Средняя твердость:\t\t 105,90").FontSize(12);
            p3.InsertTableAfterSelf(table3);
            #endregion

            document.InsertParagraph().AppendLine("Изображения отпечатков:").FontSize(12).Alignment = Alignment.left;

            Paragraph p4 = document.InsertParagraph();
            p4.Alignment = Alignment.center;

            #region 4-я таблица

            int row = tests.Count % 2 == 1 ? tests.Count / 2 + 1 : tests.Count / 2;
            Table table4 = document.AddTable(row, 2);
            table4.Alignment = Alignment.center;
            table4.Design = TableDesign.None;


            int indexTabl4 = 0;
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    if (indexTabl4 == tests.Count) continue;
                    Test _test = tests[indexTabl4];
                    Bitmap bitmap = _test.Image;
                    MemoryStream memoryStream = new MemoryStream();
                    bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                    var image = document.AddImage(memoryStream);
                    var picture = image.CreatePicture();
                    picture.Height = 200 * picture.Height / picture.Width;
                    picture.Width = 200;
                    table4.Rows[i].Cells[j].Paragraphs[0].AppendPicture(picture);
                    table4.Rows[i].Cells[j].Paragraphs[0].AppendLine("Образец " + _test.SampleName + ". Отпечаток № " + _test.SnapshotNumber).FontSize(12).Alignment = Alignment.center;
                    indexTabl4++;
                }
                table4.Rows[i].Cells[0].Paragraphs[0].AppendLine("");
            }
            p4.InsertTableAfterSelf(table4);

            #endregion

            #region подписи снизу
            Table tableF = document.AddTable(1, 2);
            tableF.Alignment = Alignment.center;
            tableF.Design = TableDesign.None;
            tableF.Rows[0].Cells[0].Paragraphs[0].Append("Испытания проводил: " + TesterName).FontSize(12).Alignment = Alignment.left;
            tableF.Rows[0].Cells[1].Paragraphs[0].Append("Дата: " + DateTime.Now.ToString("dd.MM.yyyy")).FontSize(12).Alignment = Alignment.right;
            document.AddFooters();
            Footer footer_default = document.Footers.Odd;
            footer_default.InsertParagraph().InsertTableAfterSelf(tableF);
            #endregion

            SaveFileDialog svg = new SaveFileDialog();
            if (svg.ShowDialog() == true)
            {
                document.SaveAs(svg.FileName + ".docx");
                System.Windows.MessageBox.Show("Файл сохранен");
            }
        }

    }
}
