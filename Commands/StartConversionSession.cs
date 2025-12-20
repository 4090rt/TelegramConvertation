using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using TelegramConvertorBots.Models;

namespace TelegramConvertorBots.Commands
{
    public class StartConversionSession
    {
        public readonly Dictionary<long, Models.UserSession> _usersession;
        public readonly ITelegramBotClient _botClient;

        public StartConversionSession(ITelegramBotClient botClient)
        {
            _usersession = new Dictionary<long, Models.UserSession>();
            _botClient = botClient;
        }

        public async Task StartConversionSessionAsync(long chatId, CancellationToken cancellationToken)
        {
            if (!_usersession.ContainsKey(chatId))
            {
                _usersession[chatId] = new Models.UserSession { ChatId = chatId };
            }
            var session = _usersession[chatId];
            session.state = Models.UserState.WaitingForFile;
            session.LastActivity = DateTime.UtcNow;

            await _botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "📤 <b>Отправьте файл для конвертации</b>\n\n" +
                     "Поддерживаемые форматы:\n" +
                     "• PDF, DOCX, DOC\n" +
                     "• JPG, PNG, GIF, BMP\n" +
                     "• TXT\n\n" +
                     "Максимальный размер: 50MB\n" +
                     "Или используйте /cancel для отмены",
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
        }
    }
}
