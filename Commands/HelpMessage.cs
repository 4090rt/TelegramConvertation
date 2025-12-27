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
    public class HelpMessage
    {
        private readonly ITelegramBotClient _botClient;

        public HelpMessage(ITelegramBotClient botClient)
        { 
            _botClient = botClient;
        }
        public async Task SendHelpMessageAsync(long chatId, CancellationToken cancellationToken)
        {
            var helpText =
                "📖 <b>File Converter Bot - Справка</b>\n\n" +
                "🔄 <b>Конвертация файлов:</b>\n" +
                "1. Используйте /convert или просто отправьте файл\n" +
                "2. Выберите целевой формат\n" +
                "3. Получите конвертированный файл\n\n" +
                "📋 <b>Поддерживаемые форматы:</b>\n" +
                "• PDF → DOCX, JPG, PNG, TXT\n" +
                "• DOCX → PDF, TXT\n" +
                "• JPG/PNG → PDF\n" +
                "• TXT → PDF, DOCX\n\n" +
                "⚡ <b>Основные команды:</b>\n" +
                "/start - Начальное приветствие\n" +
                "/convert - Начать конвертацию\n" +
                "/formats - Список форматов\n" +
                "/status - Статус бота\n" +
                "/sendmail - Отправить файл на почту после конвертации\n" +
                "/cancel - Отмена операции\n\n" +
                "📝 <b>Ограничения:</b>\n" +
                "• Макс. размер файла: 50MB\n" +
                "• Поддерживаются документы и изображения\n" +
                "• Конвертация может занять некоторое время";

            await _botClient.SendTextMessageAsync(
                chatId: chatId,
                text: helpText,
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
        }
    }
}
