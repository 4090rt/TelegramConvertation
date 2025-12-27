using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramConvertorBots.Models;

namespace TelegramConvertorBots.Commands
{
    public class SendMail
    {
        private readonly ITelegramBotClient _botClient;
        public readonly Dictionary<long, Models.UserSession> _usersession;
        public SendMail(ITelegramBotClient botClient)
        {
            _usersession = new Dictionary<long, Models.UserSession>();
            _botClient = botClient;
        }
        public async Task SendMails(long chatId, CancellationToken cancellationToken)
        {
            var text = "Этот раздел поозволит вам отправить конвертированный файл прямо на почту!";

            var keyboard = new InlineKeyboardMarkup(new[]
            {
            new[]
             {
            InlineKeyboardButton.WithCallbackData(
                "🚀 Начать конвертацию c отправкой на почту",
                "/sendmail")
             }
              });
            if (!_usersession.ContainsKey(chatId))
            {
                _usersession[chatId] = new Models.UserSession { ChatId = chatId };
            }
            var session = _usersession[chatId];
            session.state = Models.UserState.WaitingForFile;
            session.LastActivity = DateTime.UtcNow;
            await _botClient.SendTextMessageAsync(
            chatId: chatId,
            text: text,
            parseMode: ParseMode.Html,
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);
        }
    }
}
