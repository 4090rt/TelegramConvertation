using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramConvertorBots.Filters
{
    
    public class FilterButton
    {
        private readonly ITelegramBotClient _botClient;

        public FilterButton(ITelegramBotClient botClient)
        {  
            _botClient = botClient;
        }

        public async Task FilterButtons(long chaid, CancellationToken cancellationToken)
        {
            string text = "Выберите фильтр";

            var objectt = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Карандашный набросок", "/pencil"),
                    InlineKeyboardButton.WithCallbackData("Мультяшный", "/multi"),
                    InlineKeyboardButton.WithCallbackData("Акварель", "/aquarel")
                }
            });

            await _botClient.SendTextMessageAsync(
                 text: text,
                 chatId: chaid,
                 replyMarkup: objectt,
                 cancellationToken: cancellationToken
                );
        }
    }
}
