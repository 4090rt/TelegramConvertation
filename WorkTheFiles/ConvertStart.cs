using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramConvertorBots.Models;

namespace TelegramConvertorBots.WorkTheFiles
{
    public class ConvertStart
    {
        private readonly ITelegramBotClient _botClient;
        private readonly Dictionary<long, Models.UserSession> _userSession;
        private readonly Document _document;
        private readonly ILogger _logger;
        public ConvertStart(ITelegramBotClient botClient, Dictionary<long, Models.UserSession> userSession, Document document, ILogger logger) 
        {
            _userSession = userSession;
            _botClient = botClient;
            _document = document;
            _logger = logger;
        }

        public async Task HadleUserInputAsync(long chatId, CancellationToken cancellationToken)
        {
            if (!_userSession.TryGetValue(chatId, out var session))
            {
                await _botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Используйте /convert для начала конвертации",
                    cancellationToken: cancellationToken
                    );
                return;
            }

            switch (session.state)
            {
                case UserState.WaitingForFile:
                    _userSession[chatId].CurrentFormat = null;
                    CurrentFormat currentFormat = new CurrentFormat(_botClient, _logger, _userSession);
                    await currentFormat.ProcessFormatSelectionAsync(chatId, cancellationToken,session, _document);
                    break;

                default:
                    await _botClient.SendTextMessageAsync(
                      chatId: chatId,
                      text: "Я не понимаю. Используйте /help для списка команд",
                      cancellationToken: cancellationToken);
                    break;
            }

        }
    }
}
