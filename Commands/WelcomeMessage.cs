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
    public class WelcomeMessage
    {
        private readonly ITelegramBotClient _botClient;
        public WelcomeMessage(ITelegramBotClient botClient)
        {
            _botClient = botClient;
        }
        public async Task SendWelcomeMessageAsync(long chatId, CancellationToken cancellationToken)
        {
            var welcomeText =
                "👋 Добро пожаловать в File Converter Bot!\n\n" +
                "Я могу конвертировать файлы между различными форматами.\n\n" +
                "📋 Основные команды:\n" +
                "/convert - Начать конвертацию файла\n" +
                "/sendmail - Начать конвертацию файла\n" +
                "/formats - Показать поддерживаемые форматы\n" +
                "/help - Полный список команд\n" +
                "/cancel - Отменить текущую операцию\n\n" +
                "📤 Просто отправьте мне файл или используйте команду /convert, также можете указать почту для отправки на нее";

            await _botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: welcomeText,
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken);
        }

    }
}
