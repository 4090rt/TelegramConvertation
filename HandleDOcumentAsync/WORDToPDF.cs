
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Spire.Doc;

namespace TelegramConvertorBots.HandleDOcumentAsync
{
    public class WORDToPDF
    {
        public async Task<string> DOCXConverttoPDF(string filepath)
        {
            try
            {
                string pdfFilePath = Path.ChangeExtension(filepath, ".pdf");

                Spire.Doc.Document document = new Spire.Doc.Document();

                if (!File.Exists(filepath))
                {
                    Console.WriteLine($"❌ Файл не найден: {filepath}");
                    return "Файл не найден";
                }

                document.LoadFromFile(filepath);

                await Task.Run(() => document.SaveToFile(pdfFilePath, Spire.Doc.FileFormat.PDF));

                return pdfFilePath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка конвертации: {ex.Message}");
                return null;
            }
        }
    }
}
