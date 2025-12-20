using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;

namespace TelegramConvertorBots.WorkTheFiles
{
    public class SendDocument
    {
        private readonly ITelegramBotClient _botClient;

        public SendDocument(ITelegramBotClient botClient) 
        {
            _botClient = botClient;
        }

        public async Task SendDocumentToChatAsync(long chatId, string filePath, CancellationToken cancellationToken)
        {
            using (var filestream = System.IO.File.OpenRead(filePath))
            {
                var filename = System.IO.Path.GetFileName(filePath);


                await _botClient.SendDocumentAsync(
                    chatId: chatId,
                    document: filestream,
                    caption: "Ваш конвертированный файл",
                    cancellationToken: cancellationToken
                );
            }
        }

    }
}
