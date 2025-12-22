using Aspose.Pdf.Vector;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Telegram.Bot.Types;

namespace TelegramConvertorBots.HandleDOcumentAsync
{
    public class PDF_WordConvert
    {

        public async Task<string> PDFReadoForTxt(string filepath)
        {
            try
            {
                // собираем информацию о файле
                FileInfo fileinfo = new FileInfo(filepath);
                long filesize = fileinfo.Length;

                Console.WriteLine($"=== КОНВЕРТАЦИЯ ===");
                Console.WriteLine($"Входной файл: {filepath}");
                Console.WriteLine($"Размер PDF: {filesize} байт ({filesize / 1024.0 / 1024.0:F2} МБ)");
                // проверка размера
                if (filesize < 524288000)
                {
                    // создание нового объекта iText для чтения pdf
                    using (var reader = new iText.Kernel.Pdf.PdfReader(filepath))
                    {
                        // создание нового документа  с содержимым из reader
                        using (var pdgdocument = new iText.Kernel.Pdf.PdfDocument(reader))
                        {
                            // новый объект builder
                            StringBuilder builder = new StringBuilder();
                            // цикл для перебора страниц и действия с ними
                            for (int i = 1; i <= pdgdocument.GetNumberOfPages(); i++)
                            {
                                // кол-во страниц
                                var page = pdgdocument.GetPage(i);
                                //  новый объект стратегии чтения pdf  которая возвращает текст как одну строку, игнорируя разрывы строк
                                var strategy = new iText.Kernel.Pdf.Canvas.Parser.Listener.SimpleTextExtractionStrategy();
                                // применяем стратегию к нашему документу
                                var text = iText.Kernel.Pdf.Canvas.Parser.PdfTextExtractor.GetTextFromPage(page, strategy);
                                // добавляем в builder
                                builder.AppendLine($"---Номер страницы {page}---");
                                builder.AppendLine(text);
                            }
                            // пересохраняем  файл  как txt
                            string txtpath = Path.ChangeExtension(filepath, "_convertPDF-WORDtxt");
                            // читаем все этот файл из builder с указанием кодировки 
                            System.IO.File.WriteAllText(txtpath, builder.ToString(), Encoding.UTF8);

                            return txtpath;
                        }

                    }

                }
                else
                {
                    return "Файл слишком большой для конвертации";
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Не удалось конвертировать файл word " + ex.Message);
                return "Ошибка конвертации";
            }
        }

        public async Task<string> PDFConvertToWord(string filepath)
        {
            try
            {
             // получаем путь к txt фапйлу
              var textpathfile =  await PDFReadoForTxt(filepath);
              // пересохраняем как docx
              string docpath = Path.ChangeExtension(textpathfile, "docx");
              // читаем весь текст из txt с указанием кодировки
              string text  =  System.IO.File.ReadAllText(textpathfile, Encoding.UTF8);
                // в using создаем новый word документ
                using (WordprocessingDocument wordDocument =
                WordprocessingDocument.Create(docpath, DocumentFormat.OpenXml.WordprocessingDocumentType.Document))
                { 
                    // добавление главной части документ
                    MainDocumentPart mainpart = wordDocument.AddMainDocumentPart();
                    mainpart.Document = new DocumentFormat.OpenXml.Wordprocessing.Document();
                    // тело документа - в нем все сего содержимое
                    Body body = mainpart.Document.AppendChild(new Body());
                    // разделяем на параграфы с помощью переводов строк
                    string[] paragraphs = text.Split(new[] { "\r\n\r\n", "\n\n" }, StringSplitOptions.RemoveEmptyEntries);
                    // проходим по всем абзацам
                    foreach (var paragraphstext in paragraphs)
                    {
                        // если существует то создаем параграф в нем Run  - тестовое поле и в него добавляем текст New Text(paragraphstext)
                        if (!string.IsNullOrWhiteSpace(paragraphstext))
                        {
                            Paragraph paragraph = body.AppendChild(new Paragraph());
                            Run run = paragraph.AppendChild(new Run());
                            run.AppendChild(new Text(paragraphstext));
                        }
                    }
                    // сохраняем
                    mainpart.Document.Save();
                }
                Console.WriteLine($"✅ DOCX создан: {docpath}");
                return docpath;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Не удалось конвертировать файл word " + ex.Message);
                return "Ошибка конертации";
            }
        }
    }
}
