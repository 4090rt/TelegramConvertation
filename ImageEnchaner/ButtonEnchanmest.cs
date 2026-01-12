using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramConvertorBots.ImageEnchaner
{
    public class ButtonEnchanmest
    {
        private readonly ITelegramBotClient _botclient;

        public ButtonEnchanmest(ITelegramBotClient botclient)
        {
            _botclient = botclient;
        }

        public async Task ImageButtonsvariants(long chatid, CancellationToken cancellationToken)
        {
            var text = "Выберите тип улучшения";

            var buttons = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Слабое", "/Low"),
                    InlineKeyboardButton.WithCallbackData("Среднее","/Medium"),                   
                    InlineKeyboardButton.WithCallbackData("Высокое","/High")
                }
            });

            await _botclient.SendTextMessageAsync(
                chatId: chatid,
                 text: text,
                 replyMarkup: buttons,
                 cancellationToken: cancellationToken
                 );
        }
    }
}
