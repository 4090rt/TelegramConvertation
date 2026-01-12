using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramConvertorBots.ImageEnchaner
{
    public class ImageEnchanerButton
    {
        private readonly ITelegramBotClient _botclient;

        public ImageEnchanerButton(ITelegramBotClient botclient)
        {
            _botclient = botclient;
        }

        public async Task ImageButtonsvariants(long chatid, CancellationToken cancellationToken)
        {
            var text = "Выберите действие";

            var Buttons = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Сжать изображение", "/Compression"),
                    InlineKeyboardButton.WithCallbackData("Улучшить качество изображения", "/Enchanner")
                }
            });

            await _botclient.SendTextMessageAsync(
               chatId: chatid,
                text: text,
                replyMarkup: Buttons,
                cancellationToken: cancellationToken
                );
        }
    }
}
