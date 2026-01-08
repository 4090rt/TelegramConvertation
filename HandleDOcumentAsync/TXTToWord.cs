using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramConvertorBots.HandleDOcumentAsync
{
    public class TXTToWord
    {
        public async Task<string> DOCXConverttoPDF(string filepath)
        {
            try
            {
                string pdffile = Path.ChangeExtension(filepath, ".doc");

                using (Spire.Doc.Document document = new Spire.Doc.Document())
                {

                    if (!File.Exists(filepath))
                    {
                        Console.WriteLine($"❌ Файл не найден: {filepath}");
                        return "Файл не найден";
                    }

                    document.LoadFromFile(filepath);

                    await Task.Run(() => document.SaveToFile(pdffile, Spire.Doc.FileFormat.Doc));

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
