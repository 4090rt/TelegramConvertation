using Microsoft.Extensions.Logging;
using NLog;
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
using TelegramConvertorBots.Models;

namespace TelegramConvertorBots.WorkTheFiles
{
    public class SendDocument
    {
        private readonly ITelegramBotClient _botClient;
        private readonly Dictionary<long, Models.UserSession> _userSession;

        private readonly Microsoft.Extensions.Logging.ILogger _logger;

        public SendDocument(ITelegramBotClient botClient, Microsoft.Extensions.Logging.ILogger logger, Dictionary<long, Models.UserSession> userSession)
        {
            _botClient = botClient;
            _logger = logger;
            _userSession = userSession;
        }

        public async Task SendDocumentToChatAsync(long chatId, string filePath, CancellationToken cancellationToken)
        {
            using (var filestream = System.IO.File.OpenRead(filePath))
            {
                var session = _userSession[chatId];
   
                var filename = System.IO.Path.GetFileName(filePath);
                if (session.Email != null)
                {
                    await _botClient.SendDocumentAsync(
                    chatId: chatId,
                    document: filestream,
                    caption: $"✅ Успешно! Ваш конвертированный файл, также файл отправлен на почту {session.Email}",
                    cancellationToken: cancellationToken
                );
                }
                else
                {
                    await _botClient.SendDocumentAsync(
                    chatId: chatId,
                    document: filestream,
                    caption: "✅ Успешно! Ваш конвертированный файл, почта для отправки не указана",
                    cancellationToken: cancellationToken
                );
                }
            }
        }
    }
}

