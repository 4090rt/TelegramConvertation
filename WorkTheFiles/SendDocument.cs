using iText.Forms.Form.Element;
using Microsoft.Extensions.Logging;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using TelegramConvertorBots.Models;

namespace TelegramConvertorBots.WorkTheFiles
{
    public class SendDocument
    {
        private readonly ITelegramBotClient _botClient;
        private readonly Dictionary<long, Models.UserSession> _userSession;
        private readonly Microsoft.Extensions.Logging.ILogger _logger;

        public SendDocument(ITelegramBotClient botClient, Microsoft.Extensions.Logging.ILogger logger, Dictionary<long, Models.UserSession> userSession)
        {
            _botClient = botClient;
            _logger = logger;
            _userSession = userSession;
        }

        public async Task SendDocumentToChatAsync(long chatId, string filePath, CancellationToken cancellationToken)
        {
            try
            {
                if (!System.IO.File.Exists(filePath))
                {
                    _logger?.LogError($"Файл не найден: {filePath}");
                    await _botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "❌ Ошибка: файл не найден",
                        cancellationToken: cancellationToken
                    );
                    return;
                }
                var filename = Path.GetFileName(filePath);
                _logger?.LogInformation($"Отправка файла: {filename} в чат {chatId}");

                using (var filestream = System.IO.File.OpenRead(filePath))
                {
                    var session = _userSession[chatId];
                    var inputFile = new Telegram.Bot.Types.InputFiles.InputOnlineFile(filestream, filename);

                    if (session.Email != null)
                    {
                        await _botClient.SendDocumentAsync(
                        chatId: chatId,
                        document: inputFile,
                        caption: $"✅ Успешно! Ваш конвертированный файл, также файл отправлен на почту {session.Email}",
                        cancellationToken: cancellationToken
                    );
                    }
                    else
                    {
                        await _botClient.SendDocumentAsync(
                        chatId: chatId,
                        document: inputFile,
                        caption: "✅ Успешно! Ваш конвертированный файл, почта для отправки не указана",
                        cancellationToken: cancellationToken
                    );
                    }
                }
            }
            catch (FileNotFoundException ex)
            {
                _logger?.LogError(ex, $"Файл не найден: {filePath}");
                await _botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "❌ Файл не найден после конвертации",
                    cancellationToken: cancellationToken
                );
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Ошибка отправки файла {filePath}");
                await _botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: $"❌ Ошибка отправки файла: {ex.Message}",
                    cancellationToken: cancellationToken
                );
            }
        }
    }
}

