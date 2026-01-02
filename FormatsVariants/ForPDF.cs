using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramConvertorBots.FormatsVariants
{
    public class ForPDF
    {
        private readonly ITelegramBotClient _botClient;
        public readonly Dictionary<long, Models.UserSession> _usersession;
        public ForPDF(ITelegramBotClient botClient)
        {
            _usersession = new Dictionary<long, Models.UserSession>();
            _botClient = botClient;
        }

        public async Task ButtonsPdf(long chatId, CancellationToken cancellationToken)
        {
            var text = "Выберите формат";

            var keyboard = new InlineKeyboardMarkup(new[]
            {
        new[]
        {
            InlineKeyboardButton.WithCallbackData("Word", "/word"),
            InlineKeyboardButton.WithCallbackData("txt", "/txt"),
            InlineKeyboardButton.WithCallbackData("HTML", "/HTML")
        }
    });

            await _botClient.SendTextMessageAsync(
                chatId: chatId,
                text: text,
                replyMarkup: keyboard,
                cancellationToken: cancellationToken);
        }
    }
}
