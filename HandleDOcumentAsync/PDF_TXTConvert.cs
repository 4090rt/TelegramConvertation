using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramConvertorBots.HandleDOcumentAsync
{
    public class PDF_TXTConvert
    {
        public async Task<string> PDFReaderForTXT(string filepath)
        {
            try
            {
                FileInfo fileimfo = new FileInfo(filepath);
                long filesize = fileimfo.Length;

                Console.WriteLine($"=== КОНВЕРТАЦИЯ ===");
                Console.WriteLine($"Входной файл: {filepath}");
                Console.WriteLine($"Размер PDF: {filesize} байт ({filesize / 1024.0 / 1024.0:F2} МБ)");

                if (filesize < 524288000)
                {
                    using (var pdfReafer = new iText.Kernel.Pdf.PdfReader(filepath))
                    {
                        using (var pdfdoc = new iText.Kernel.Pdf.PdfDocument(pdfReafer))
                        {
                            StringBuilder builder = new StringBuilder();

                            for (int i = 1; i <= pdfdoc.GetNumberOfPages(); i++)
                            { 
                                var pahe = pdfdoc.GetPage(i);

                                var strategy = new iText.Kernel.Pdf.Canvas.Parser.Listener.SimpleTextExtractionStrategy();

                                var text = iText.Kernel.Pdf.Canvas.Parser.PdfTextExtractor.GetTextFromPage(pahe,strategy);


                                builder.AppendLine($"---Номер страницы {pahe}---");
                                builder.AppendLine(text);
                            }

                            string txtpath = Path.ChangeExtension(filepath, "txt");

                            using (StreamWriter writer = new StreamWriter(txtpath, false, new UTF8Encoding(true)))
                            {
                                await writer.WriteAsync(builder.ToString());
                            }

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
    }
}
