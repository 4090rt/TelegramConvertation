using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;

namespace TelegramConvertorBots.CompressionsImages
{
    public class CompressionsImagesSend
    {
        private readonly ITelegramBotClient _botClient;
        private readonly Dictionary<long, Models.UserSession> _userSession;
        private readonly Microsoft.Extensions.Logging.ILogger _logger;

        public CompressionsImagesSend(ITelegramBotClient botClient, Microsoft.Extensions.Logging.ILogger logger, Dictionary<long, Models.UserSession> userSession)
        { 
            _botClient = botClient; 
            _userSession = userSession;
            _logger = logger;
        }

        public async Task SendImageToChatAsync(long chatId, string filePath, CancellationToken cancellationToken)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    _logger?.LogError($"Файл не найден: {filePath}");
                    await _botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "❌ Ошибка: файл не найден",
                        cancellationToken: cancellationToken
                    );
                    return;
                }

                FileInfo fileinfo = new FileInfo(filePath);
                long fileSizeKB = fileinfo.Length / 1024;

                string pathname = Path.GetFileName(filePath);
                _logger.LogInformation($"Отправка файла: {pathname} в чат {chatId}");

                using (var filestram = File.OpenRead(filePath))
                {
                    var session = _userSession[chatId];
                    var inputFile = new Telegram.Bot.Types.InputFiles.InputOnlineFile(filestram, pathname);

                    await _botClient.SendPhotoAsync(
                    chatId: chatId,
                    photo: inputFile,
                    caption: $"✅ Успешно! Ваш сжатый файл, теперь его размер {fileSizeKB} KB",
                    cancellationToken: cancellationToken);
                }
            }
            catch (FileNotFoundException ex)
            {
                _logger?.LogError(ex, $"Файл не найден: {filePath}");
                await _botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "❌ Файл не найден после конвертации",
                    cancellationToken: cancellationToken
                );
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, $"Ошибка отправки файла {filePath}");
                await _botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: $"❌ Ошибка отправки файла: {ex.Message}",
                    cancellationToken: cancellationToken
                );
            }
        }
    }
}
