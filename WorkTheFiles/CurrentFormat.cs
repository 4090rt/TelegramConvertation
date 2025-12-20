using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using TelegramConvertorBots.Models;

namespace TelegramConvertorBots.WorkTheFiles
{
    public class CurrentFormat
    {
        private readonly ITelegramBotClient _botClient;
        private readonly Telegram.Bot.Types.Message _message;
        public readonly Microsoft.Extensions.Logging.ILogger _logger;
        public CurrentFormat(ITelegramBotClient botClient)
        { 
            _botClient = botClient;
        }

        public async Task ProcessFormatSelectionAsync(long chatId, CancellationToken cancellationToken,UserSession session)
        {
           
            string format = _message.Text;
            format = format.ToLower().Trim();
            if (format.StartsWith("."))
            {
                format = format.Substring(1);
            }

            var supportsformat = new[] { "docx", "txt", "pdf", "html" };
            if (!supportsformat.Contains(format))
            {
                await _botClient.SendTextMessageAsync(
                   chatId: chatId,
                   text: $"❌ Формат '{format}' не поддерживается.\n\n" +
                        "Доступные форматы: docx, pdf, jpg, png, txt\n\n" +
                        "Введите формат еще раз:",
                   cancellationToken: cancellationToken);
                return;
            }
            session.CurrentFormat = format;
            session.state = UserState.Processing;

            await _botClient.SendTextMessageAsync(
                 chatId: chatId,
                 text: $"🔄 Начинаю конвертацию в {format.ToUpper()}...\n\n" +
                      "Это может занять некоторое время.",
                 cancellationToken: cancellationToken);
            HandleDocument document = new HandleDocument(_botClient, _logger);
            await document.HandleDocumentAsyncmethod(_message, format, chatId, cancellationToken);

            session.state = UserState.Idle;

            await _botClient.SendTextMessageAsync(
                chatId: chatId,
                text: $"✅ Конвертация завершена!\n\n" +
                     "В реальной версии здесь будет файл для скачивания.\n" +
                     "Пока это демо-версия.",
                cancellationToken: cancellationToken);
        }
    }
}
