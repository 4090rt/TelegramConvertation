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
    public class ConvertStart
    {
        private readonly ITelegramBotClient _botClient;
        private readonly Dictionary<long, Models.UserSession> _userSession;
        public ConvertStart(ITelegramBotClient botClient) 
        {
            _userSession = new  Dictionary<long, Models.UserSession>();
            _botClient = botClient;
        }

        public async Task HadleUserInputAsync( long chatId, CancellationToken cancellationToken)
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
                case UserState.WaitingForFormat:
                    CurrentFormat currentFormat = new CurrentFormat(_botClient);
                    await currentFormat.ProcessFormatSelectionAsync(chatId, cancellationToken,session);
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
