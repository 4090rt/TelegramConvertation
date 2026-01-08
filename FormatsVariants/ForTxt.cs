using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramConvertorBots.FormatsVariants
{
    public class ForTxt
    {
        private readonly ITelegramBotClient _botclient;
        public ForTxt(ITelegramBotClient botClient)
        {
            _botclient = botClient;
        }

        public async Task ButtonsPdf(long chatId, CancellationToken cancellationToken)
        {
            var text = "Выберите формат";

            var keyboard = new InlineKeyboardMarkup(new[]
            {
        new[]
        {
            InlineKeyboardButton.WithCallbackData("Word", "/txt"),
        }
    });

            await _botclient.SendTextMessageAsync(
                chatId: chatId,
                text: text,
                replyMarkup: keyboard,
                cancellationToken: cancellationToken);
        }
    }
}
