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
using TelegramConvertorBots.CompressionRatios;

namespace TelegramConvertorBots.WorkTheFiles
{
    public class DowloadImage
    {
        private readonly ITelegramBotClient _botclient;
        private readonly Dictionary<long, Models.UserSession> _userSession;
        public readonly Microsoft.Extensions.Logging.ILogger _logger;
        private readonly CommandHandler.CommandHandlerr _commandHandlerr;
        public DowloadImage(ITelegramBotClient botClient, Dictionary<long, Models.UserSession> userSession, Microsoft.Extensions.Logging.ILogger logger)
        {
            _botclient = botClient;
            _userSession = userSession;
            _logger = logger;
        }

        public async Task ImageDowloadedAsync(Message message, CancellationToken canceltoken, long chatid, ILogger logger)
        {
                        await _botclient.SendTextMessageAsync(
                chatId: chatid,
                text: "⏳ Начинаю скачивание...",
                cancellationToken: canceltoken
            );

            var startTime = DateTime.Now;
            var session = _userSession[chatid];

            var photos = message.Photo;
            var largestPhoto = photos.Last();

            var fileinfo = await _botclient.GetFileAsync(largestPhoto.FileId,canceltoken);


            string exepath = AppDomain.CurrentDomain.BaseDirectory;
            string apppath = Path.Combine(exepath, "Downloads");
            Directory.CreateDirectory(apppath);

            string filename = Path.Combine(apppath, $"{DateTime.Now:yyyyMMdd_HHmmss}_{chatid}_photo.jpg");

            using (var filestream = System.IO.File.Create(filename))
            {
                await _botclient.DownloadFileAsync(fileinfo.FilePath,filestream,canceltoken);
            }
            var downloadTime = DateTime.Now - startTime;
            logger?.LogInformation($"Файл скачан за {downloadTime.TotalSeconds:F2} секунд");

            if (downloadTime.TotalSeconds > 20)
            {
                logger?.LogWarning("Скачивание заняло слишком много времени, file_id мог истечь!");
            }

            await _botclient.SendTextMessageAsync(
                    chatId: chatid,
                    text: "✅ Файл скачен",
                    cancellationToken: canceltoken
                );
            Compressionretes compressionretes = new Compressionretes(_botclient);
            await compressionretes.ButtonsImage(chatid,canceltoken);

            session.CurrentFilePath = filename;
        }
    }
}
