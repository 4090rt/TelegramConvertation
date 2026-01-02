using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramConvertorBots.CommandHandler;
using TelegramConvertorBots.FormatsVariants;
using TelegramConvertorBots.HandleDOcumentAsync;
using TelegramConvertorBots.Models;

namespace TelegramConvertorBots.WorkTheFiles
{
    public class DocumentDowloaded
    {
        private readonly ITelegramBotClient _botclient;
        private readonly Dictionary<long, Models.UserSession> _userSession;
        public readonly Microsoft.Extensions.Logging.ILogger _logger;
        private readonly CommandHandler.CommandHandlerr _commandHandlerr;
        public DocumentDowloaded(ITelegramBotClient botClient, Dictionary<long, Models.UserSession> userSession, Microsoft.Extensions.Logging.ILogger logger)
        { 
            _botclient = botClient;
            _userSession = userSession;
            _logger = logger;
        }

        public async Task DocumentDowloadedAsync(string format, Document documents, CancellationToken canceltoken, long chatid, ILogger logger)
        {
            var startTime = DateTime.Now;
            var fileInfo = await _botclient.GetFileAsync(documents.FileId, canceltoken);
            var filename = documents.FileName ?? "Без названия"; ;
            var filesize = documents.FileSize / 1024.0 / 1024.0;
            var session = _userSession[chatid];

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
            session.CurrentFilePath = filePath;

            if (format == "pdf")
            {
                ForPDF forPDF = new ForPDF(_botclient);
                await forPDF.ButtonsPdf(chatid, canceltoken);
            }
            else
            {
                await _botclient.SendTextMessageAsync(
             chatId: chatid,
             text: "Функция пока недоступна",
             cancellationToken: canceltoken
         );
            }
        }
  
    }
}
