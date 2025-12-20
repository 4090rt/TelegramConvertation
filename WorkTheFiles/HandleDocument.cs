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
       private readonly Document _documents;
        public readonly Microsoft.Extensions.Logging.ILogger _logger;
        private readonly ITelegramBotClient _botclient;

        public HandleDocument(ITelegramBotClient botClient, Microsoft.Extensions.Logging.ILogger logger)
        { 
            _botclient = botClient;
            _logger = logger;
        }
       public async Task HandleDocumentAsyncmethod(Telegram.Bot.Types.Message message, string format, long chatId, CancellationToken cancellationToken)
       {
            if (message.Document != null)
            {
                try
                {
                    var filename = _documents.FileName ?? "Без названия"; ;
                    var filesize = _documents.FileSize / 1024.0 / 1024.0;
                    var type = _documents.MimeType ?? "Неизвестный тип";


                    _logger.LogInformation($"Получен файл:{filename} Размер: {filesize} Тип: {type}");

                    DocumentDowloaded documentDowloaded = new DocumentDowloaded(_botclient);
                    await documentDowloaded.DocumentDowloadedAsync(_documents, cancellationToken, chatId);
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
