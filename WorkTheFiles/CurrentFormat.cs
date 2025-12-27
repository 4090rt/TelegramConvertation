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
using TelegramConvertorBots.Models;

namespace TelegramConvertorBots.WorkTheFiles
{
    public class CurrentFormat
    {
        private readonly ITelegramBotClient _botClient;
        private readonly Telegram.Bot.Types.Message _message;
        public readonly Microsoft.Extensions.Logging.ILogger _logger;
        private readonly Dictionary<long, Models.UserSession> _userSession;
        public CurrentFormat(ITelegramBotClient botClient, ILogger logger, Dictionary<long, Models.UserSession> userSession)
        { 
            _botClient = botClient;
            _userSession = userSession;
            _logger = logger;
        }

        public async Task ProcessFormatSelectionAsync(long chatId, CancellationToken cancellationToken,UserSession session, Document document)
        {
            if (document != null && !string.IsNullOrEmpty(document.FileName))
            {
                string filename = document.FileName;
                string exttation = Path.GetExtension(filename);

                string format = Path.GetExtension(filename).Trim('.') ?? "";
                format = format.ToLower();

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
                     text: $"🔄 Начинаю конвертацию файла {filename}, формата {format.ToUpper()}...\n\n" +
                          "Это может занять некоторое время.",
                     cancellationToken: cancellationToken);


                HandleDocument documents = new HandleDocument(_botClient, _logger, _userSession);
                await documents.HandleDocumentAsyncmethod(document, _message, format, chatId, cancellationToken);
                _logger.LogInformation($"передан документ {filename}");
                session.state = UserState.Idle;
            }
        }
    }
}
