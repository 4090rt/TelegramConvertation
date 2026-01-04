using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace TelegramConvertorBots.Commands
{
    public class Startofcompression
    {
        private readonly ITelegramBotClient _botClient;
        public Startofcompression(ITelegramBotClient botclient)
        { 
            _botClient = botclient;
        }

        public async Task SendStartofcompression(long chatId, CancellationToken cancellationToken)
        {
            string text = "Вы можете уменьшить размер изображения, просто отправьте изображение в чат";

            await _botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: text,
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken);
        }
    }
}
