using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramConvertorBots.HandleDOcumentAsync
{
    public class PDFToTxt
    {
        public async Task<string> PDFConverttoTXT(string filepath)
        {
            try
            {
                string txtpath = Path.ChangeExtension(filepath, ".txt");

                if (!File.Exists(filepath))
                {
                    Console.WriteLine($"❌ Файл не найден: {filepath}");
                    return "Файл не найден";
                }

                FileInfo fileInfo = new FileInfo(filepath);

                if (fileInfo.Length > 10 * 1024 * 1024) // 10MB лимит для PDF
                {
                    Console.WriteLine($"❌ Файл слишком большой: {fileInfo.Length / 1024 / 1024} MB");
                    return "Файл слишком большой для конвертации";
                }

                using (Spire.Pdf.PdfDocument document = new Spire.Pdf.PdfDocument())
                { 
                    document.LoadFromFile(filepath);

                    await Task.Run(() => document.SaveToFile(txtpath));

                    return txtpath;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка конвертации: {ex.Message}");
                return null;
            }
        }
    }
}
