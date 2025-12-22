using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramConvertorBots.HandleDOcumentAsync;

namespace TelegramConvertorBots.WorkTheFiles
{
    public class DocumentDowloaded
    {
        private readonly ITelegramBotClient _botclient;

        public DocumentDowloaded(ITelegramBotClient botClient)
        { 
            _botclient = botClient;
        }

        public async Task DocumentDowloadedAsync(string format, Document documents, CancellationToken canceltoken, long chatid, ILogger logger)
        {
            var startTime = DateTime.Now;
            var fileInfo = await _botclient.GetFileAsync(documents.FileId, canceltoken);
            var filename = documents.FileName ?? "Без названия"; ;
            var filesize = documents.FileSize / 1024.0 / 1024.0;


            await _botclient.SendTextMessageAsync(
                chatId: chatid,
                text: "⏳ Скачиваю файл...",
                cancellationToken: canceltoken
            );

            string exePath = AppDomain.CurrentDomain.BaseDirectory;
            var downloadPath = Path.Combine(exePath, "Downloads");

            // Гарантированно создаем папку
            Directory.CreateDirectory(downloadPath);

            var filePath = Path.Combine(downloadPath,
              $"{DateTime.Now:yyyyMMdd_HHmmss}_{chatid}_{documents.FileName ?? "file"}");

            using (var Filestream = System.IO.File.Create(filePath))
            {
                await _botclient.DownloadFileAsync(fileInfo.FilePath, Filestream, canceltoken);
            }

            var downloadTime = DateTime.Now - startTime;
            logger?.LogInformation($"Файл скачан за {downloadTime.TotalSeconds:F2} секунд");

            if (downloadTime.TotalSeconds > 20) // Если скачивание заняло больше 20 секунд
            {
                logger?.LogWarning("Скачивание заняло слишком много времени, file_id мог истечь!");
            }

            await _botclient.SendTextMessageAsync(
                     chatId: chatid,
                     text: "⏳ Файл скачен, ожидаю конвертации",
                     cancellationToken: canceltoken
                 );

            string formalower = format.ToLower();
            if (formalower == "pdf")
            {
                PDF_WordConvert convert = new PDF_WordConvert();
                var converteredfilePDF_WORD = await convert.PDFConvertToWord(filePath);
                if (converteredfilePDF_WORD == "Ошибка конертации" || converteredfilePDF_WORD == "Файл слишком большой для конвертации")
                {
                    await _botclient.SendTextMessageAsync(
                        chatId: chatid,
                        text: $"❌ {converteredfilePDF_WORD}",
                        cancellationToken: canceltoken
                    );
                    return;
                }
                SendDocument senddocumentpdf_word = new SendDocument(_botclient, logger);
                await senddocumentpdf_word.SendDocumentToChatAsync(chatid, converteredfilePDF_WORD, canceltoken);

                TryDeleteFile(filePath);
                TryDeleteFile(converteredfilePDF_WORD);
            }

            if (formalower == "txt")
            {
                TXT_WordConvert convert = new TXT_WordConvert();
                var converteredTXT_Word = await convert.TXTConverttoWord(filePath);
                if (converteredTXT_Word == "Ошибка конертации" || converteredTXT_Word == "Файл слишком большой для конвертации")
                {
                    await _botclient.SendTextMessageAsync(
                        chatId: chatid,
                        text: $"❌ {converteredTXT_Word}",
                        cancellationToken: canceltoken
                    );
                    return;
                }
                SendDocument senddocumenttxt_word = new SendDocument(_botclient, logger);
                await senddocumenttxt_word.SendDocumentToChatAsync(chatid, converteredTXT_Word, canceltoken);


                TryDeleteFile(filePath);
                TryDeleteFile(converteredTXT_Word);
            }
        }
        private void TryDeleteFile(string filePath)
        {
            // Несколько попыток удаления с задержкой
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    System.IO.File.Delete(filePath);
                    break; // Успешно
                }
                catch (IOException) when (i < 2)
                {
                    Thread.Sleep(100 * (i + 1)); // Ждем и пробуем снова
                }
            }
        }
    }
}
