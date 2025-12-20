using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using TelegramConvertorBots.Models;

namespace TelegramConvertorBots.Commands
{

    public class CancelCurrentOperation
    {
        private readonly ITelegramBotClient _botClient;
        public readonly Dictionary<long, Models.UserSession> _userSession;

        public CancelCurrentOperation(ITelegramBotClient botClient)
        {
            _userSession = new Dictionary<long, Models.UserSession>();
            _botClient = botClient;
        }

        public async Task CancelCurrentOperationAsync(long chatId, CancellationToken cancellationToken)
        {
            if (_userSession.ContainsKey(chatId))
            {
                _userSession[chatId].state = UserState.Idle;
                _userSession[chatId].CurrentFilePath = null;
                _userSession[chatId].CurrentFormat = null;
            }
            await _botClient.SendTextMessageAsync(
                 chatId: chatId,
                 text: "❌ Операция отменена\n\n" +
                      "Вы можете начать заново с помощью /convert",
                 cancellationToken: cancellationToken);
        }
    }
}
