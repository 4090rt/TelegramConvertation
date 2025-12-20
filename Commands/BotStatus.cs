using Aspose.Pdf.Forms;
using Microsoft.Extensions.Options;
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
    public class BotStatus
    {
        public readonly Dictionary<long, Models.UserSession> _userSession;
        private readonly Models.BotConfig _botConfig;
        public readonly ITelegramBotClient _botClient;

        public BotStatus(Models.BotConfig botConfig, ITelegramBotClient botClient)
        {
            _userSession = new Dictionary<long, Models.UserSession>();
            _botConfig = botConfig;
            _botClient = botClient;
        }
        public async Task SendBotStatusAsync(long chatId, CancellationToken cancellationToken)
        {
            var activeUsers = _userSession.Count(s =>
            (DateTime.UtcNow - s.Value.LastActivity).TotalMinutes < 5);

            var maxSizeMB = _botConfig.MaxFileSize / 1024 / 1024;

            var statusText =
                "📊 <b>Статус бота:</b>\n\n" +
                $"• Активных пользователей: {activeUsers}\n" +
                $"• Всего сессий: {_userSession.Count}\n" +
                $"• Поддерживаемых форматов: {_botConfig.SupportedFormats.Length}\n" +
                $"• Макс. размер файла: {maxSizeMB}MB\n\n" +
                "✅ Бот работает нормально";

            await _botClient.SendTextMessageAsync(
                chatId: chatId,
                text: statusText,
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken
                );
        }
    }
}
