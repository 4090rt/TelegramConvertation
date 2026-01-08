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
    public class ForWord
    {
        private readonly ITelegramBotClient _botClient;

        public ForWord(ITelegramBotClient botClient)
        {
            _botClient = botClient;
        }

        public async Task ButtonsWord(long chatId, CancellationToken cancellationToken)
        {
            var text = "Выберите формат";

            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("PDF","/pdf"),
                    InlineKeyboardButton.WithCallbackData("txt", "/txt")
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
