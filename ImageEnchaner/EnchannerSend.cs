using Aspose.Pdf.Operators;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;

namespace TelegramConvertorBots.ImageEnchaner
{

    public class EnchannerSend
    {
        private readonly ITelegramBotClient _botClient;
        private readonly Dictionary<long, Models.UserSession> _userSession;
        private readonly Microsoft.Extensions.Logging.ILogger _logger;

        public EnchannerSend(ITelegramBotClient botClient, Microsoft.Extensions.Logging.ILogger logger, Dictionary<long, Models.UserSession> userSession)
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
                    _logger.LogError("Файл по пути не найден");

                    await _botClient.SendTextMessageAsync(
                     chatId: chatId,
                     text: "Кажется, что файл потерялся",
                     cancellationToken: cancellationToken
                     );
                }
                    FileInfo fileInfo = new FileInfo(filePath);
                    string filename = fileInfo.Name;  
                    long filesize = fileInfo.Length / 1024;

                    _logger.LogInformation($"Отправка файла {filename} в чат {chatId}");

                    using (var fileread = System.IO.File.OpenRead(filePath))
                    {
                        var session = _userSession[chatId];
                        var inpuntfiles = new Telegram.Bot.Types.InputFiles.InputOnlineFile(fileread, filename);

                        await _botClient.SendPhotoAsync(
                         chatId: chatId,
                         photo: inpuntfiles,
                         caption: $"✅ Успешно! Теперь размер вашего изображения {filesize} KB",
                         cancellationToken: cancellationToken);
                    }
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogError("Файл не найден" + ex.Message);

                await _botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Что-то пошло не так",
                    cancellationToken: cancellationToken
                    );
            }
            catch (Exception ex)
            {
                _logger.LogError("Исключение" + ex.Message);

                await _botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Что-то пошло не так",
                    cancellationToken: cancellationToken
                    );
            }
        }
    }
}
