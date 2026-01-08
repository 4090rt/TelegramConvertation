using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramConvertorBots.CompressionRatios
{
    public class Compressionretes
    {
        private readonly ITelegramBotClient _botclient;

        public Compressionretes(ITelegramBotClient botclient)
        {
            _botclient = botclient;
        }

        public async Task ButtonsImage(long chatId, CancellationToken cancellationToken)
        {
            var text = "Выберите степень сжатия";

            var keyboards = new InlineKeyboardMarkup(new[]
            {
            new[]
            {
                  InlineKeyboardButton.WithCallbackData("Слабое", "/Lite"),
                  InlineKeyboardButton.WithCallbackData("Среднее", "/Middle"),
                  InlineKeyboardButton.WithCallbackData("Сильное", "/Stronger"),
            }
            });

            await _botclient.SendTextMessageAsync(
                chatId: chatId,
                text: text,
                replyMarkup: keyboards,
                cancellationToken: cancellationToken
                );
        }
    }
}
