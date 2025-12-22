using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramConvertorBots.HandleDOcumentAsync
{
    public class TXT_WordConvert
    {
        public async Task<string> TXTConverttoWord(string filepath)
        {
            try
            {
                // собираем информацию о файле
                FileInfo fileinfo = new FileInfo(filepath);
                long filesize = fileinfo.Length;

                Console.WriteLine($"=== КОНВЕРТАЦИЯ ===");
                Console.WriteLine($"Входной файл: {filepath}");
                Console.WriteLine($"Размер TXT: {filesize} байт ({filesize / 1024.0 / 1024.0:F2} МБ)");

                // пересохранили файл как ворд
                string pathext = Path.ChangeExtension(filepath,"docx");
                // прочитали весь текст с указанием кодировки
                string text = System.IO.File.ReadAllText(filepath, Encoding.UTF8);
                // создание нового объекта document
                using (WordprocessingDocument wordDocument =
                    WordprocessingDocument.Create(pathext, DocumentFormat.OpenXml.WordprocessingDocumentType.Document))
                {
                    // добавление главной части документ
                    MainDocumentPart mainpart = wordDocument.AddMainDocumentPart();
                    mainpart.Document = new DocumentFormat.OpenXml.Wordprocessing.Document();
                    // тело документа - в нем все сего содержимое
                    DocumentFormat.OpenXml.Wordprocessing.Body body = mainpart.Document.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Body());
                    // разделяем на параграфы с помощью переводов строк
                    string[] paragraphs = text.Split(new[] { "\r\n\r\n", "\n\n" }, StringSplitOptions.RemoveEmptyEntries);
                    // проходим по всем абзацам
                    foreach (var paragraphstext in paragraphs)
                    {
                        // если существует то создаем параграф в нем Run  - тестовое поле и в него добавляем текст New Text(paragraphstext)
                        if (!string.IsNullOrWhiteSpace(paragraphstext))
                        {
                            DocumentFormat.OpenXml.Wordprocessing.Paragraph paragraph = body.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Paragraph());
                            DocumentFormat.OpenXml.Wordprocessing.Run run = paragraph.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Run());
                            run.AppendChild(new Text(paragraphstext));
                        }
                    }
                    // сохраняем
                    mainpart.Document.Save();
                }
                Console.WriteLine($"✅ DOCX создан: {pathext}");
                return pathext;

            }
            catch (Exception ex)
            {
                Console.WriteLine("Не удалось конвертировать файл word " + ex.Message);
                return "Ошибка конертации";
            }
        }
    }
}
