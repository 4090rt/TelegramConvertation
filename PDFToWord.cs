using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramConvertorBots.HandleDOcumentAsync
{
    public class PDFToWord
    {
        public async Task<string> PDFConverttoDocx(string filepath)
        {
            try
            {
                string pdffile = Path.ChangeExtension(filepath, ".doc");

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

                using (Spire.Pdf.PdfDocument documentS = new Spire.Pdf.PdfDocument())
                {

                    documentS.LoadFromFile(filepath);

                    await Task.Run(() => documentS.SaveToFile(pdffile, Spire.Pdf.FileFormat.DOCX));

                    return pdffile;
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
