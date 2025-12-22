using Microsoft.Extensions.Logging;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramConvertorBots.WorkTheFiles
{
    public class HandleDocument
    {
        public readonly Microsoft.Extensions.Logging.ILogger _logger;
        private readonly ITelegramBotClient _botclient;

        public HandleDocument(ITelegramBotClient botClient, Microsoft.Extensions.Logging.ILogger logger)
        { 
            _botclient = botClient;
            _logger = logger;
        }
       public async Task HandleDocumentAsyncmethod(Document documant, Telegram.Bot.Types.Message message, string format, long chatId, CancellationToken cancellationToken)
       {
            if (documant != null)
            {
                try
                {
                    var filename = documant.FileName ?? "Без названия"; ;
                    var filesize = documant.FileSize / 1024.0 / 1024.0;
                    var type = documant.MimeType ?? "Неизвестный тип";


                    _logger.LogInformation($"Получен файл:{filename} Размер: {filesize} Тип: {type}");

                    DocumentDowloaded documentDowloaded = new DocumentDowloaded(_botclient);
                    await documentDowloaded.DocumentDowloadedAsync(format, documant, cancellationToken, chatId, _logger);
                }
                catch (Exception ex)
                {
                    await _botclient.SendTextMessageAsync(
                    chatId: chatId,
                    text: $"❌ Ошибка при обработке файла: {ex.Message}",
                    cancellationToken: cancellationToken);
                }
            }
            else
            {
                await _botclient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "Отправьте файл (документ) или текстовую команду",
                        cancellationToken: cancellationToken
                    );
            }
       }
    }
}
