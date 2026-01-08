using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramConvertorBots.Commands
{
    public class FormatsList
    {
        private readonly ITelegramBotClient _botClient;

        public FormatsList(ITelegramBotClient botClient)
        { 
            _botClient = botClient;
        }
        public async Task SendFormatsListAsync(long chatId, CancellationToken cancellationToken)
        {
            var formatsText =
                "📊 <b>Поддерживаемые форматы конвертации:</b>\n\n" +
                "🔸 <b>Из PDF в:</b>\n" +
                "   • DOCX (Word документ)\n" +
                "   • HTML (Браузерная страница)\n" +
                "🔸 <b>Из DOCX в:</b>\n" +
                "   • PDF (документ)\n" +
                "   • TXT (текстовый файл)\n\n" +
                "🔸 <b>Из txt  в:</b>\n" +
                "   • DOCX (WORD документ)\n\n" +
                "💡 <b>Сжатие изображения JPG, PNG</b>\n" +
                "💡 <b>Как использовать конвертацию:</b>\n" +
                "1. Отправьте файл или используйте /convert, так же перед отправкой,\n" +
                "можете ввести почту и файл отправится туда /sendmail, \n" +
                "2. Выберите целевой формат (например: docx)\n" +
                "3. Получите результат\n" +
                 "💡 <b>Как использовать сжатие:</b>\n" +
                 "1. Отправьте файл или используйте /compression, или просто перетащите фото,\n" +
                 "2. Выберите уровень сжатия (например: Средний)\n" +
                 "3. Получите результат";
            var keyboard = new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton[] { "🚀 Начать конвертацию (/convert)" }
            })
            {
                ResizeKeyboard = true,
                OneTimeKeyboard = true
            };

            await _botClient.SendTextMessageAsync(
            chatId: chatId,
            text: formatsText,
            parseMode: ParseMode.Html,
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);
        }
    }
}
