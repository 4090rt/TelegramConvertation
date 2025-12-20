using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramConvertorBots.HandleDOcumentAsync;

namespace TelegramConvertorBots.WorkTheFiles
{
    public class DocumentDowloaded
    {
        private readonly ITelegramBotClient _botclient;

        public DocumentDowloaded(ITelegramBotClient botClient)
        { 
            _botclient = botClient;
        }

        public async Task DocumentDowloadedAsync(Document documents, CancellationToken canceltoken, long chatid)
        {
            var fileInfo = await _botclient.GetFileAsync(documents.FileId, canceltoken);
            var filename = documents.FileName ?? "Без названия"; ;
            var filesize = documents.FileSize / 1024.0 / 1024.0;
            var type = documents.MimeType ?? "Неизвестный тип";


            await  _botclient.SendTextMessageAsync(
                chatId: chatid,
                text: "⏳ Скачиваю файл...",
                cancellationToken: canceltoken
            );

            var getpilepath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "dowlod",
                "TelegramBotConvert"
                );
            Directory.CreateDirectory(getpilepath);

            var filePath = Path.Combine(getpilepath,
              $"{DateTime.Now:yyyyMMdd_HHmmss}_{chatid}_{documents.FileName ?? "file"}");

            using (var Filestream = System.IO.File.Create(filePath))
            {
                await _botclient.DownloadFileAsync(fileInfo.FilePath, Filestream, canceltoken);
            }

            if (type == "pdf")
            {
                PDF_WordConvert convert = new PDF_WordConvert();
                var converteredfile = await convert.PDFConvertToWord(filePath, canceltoken);

                SendDocument senddocument = new SendDocument(_botclient);
                await senddocument.SendDocumentToChatAsync(chatid, converteredfile, canceltoken);

                System.IO.File.Delete(getpilepath);
                System.IO.File.Delete(converteredfile);
            }

        }
    }
}
