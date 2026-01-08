using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramConvertorBots.Commands;
using TelegramConvertorBots.CompressionsImages;
using TelegramConvertorBots.HandleDOcumentAsync;
using TelegramConvertorBots.Models;
using TelegramConvertorBots.WorkTheFiles;

namespace TelegramConvertorBots.CommandHandler
{
    public class CommandHandlerr
    {
        //Создаем глобальные переменные 
        public readonly ITelegramBotClient _botClient;
        public readonly Dictionary<long, Models.UserSession> _userSession;
        public readonly Models.BotConfig _botConfig;
        public readonly ILogger _logger;
        private readonly WelcomeMessage _welcomeMessage;
        private readonly HelpMessage _helpMessage;
        private readonly FormatsList _formatslist;
        private readonly StartConversionSession _startConversionSession;
        private readonly BotStatus _botStatus;
        private readonly CancelCurrentOperation _cancelCurrentOperation;
        private readonly MainComands _mainComands;
        private readonly ConvertStart _convertStart;
        private readonly CurrentFormat _currentFormat;
        private readonly HandleDocument _handleDocument;
        private readonly DocumentDowloaded _documentDowloaded;
        private readonly Telegram.Bot.Types.Message _message;

        public CommandHandlerr(
            //Берем классы и их настройки и присваиваем их в переменные - использование настроек с классами
            ITelegramBotClient botClient,
            IOptions<Models.BotConfig> config,
            ILogger<CommandHandlerr> logger)
        {
            //присваиваем значения к глобальным переменным
            _botClient = botClient;
            _userSession = new Dictionary<long, Models.UserSession>();
            _logger = logger;
            _botConfig = config.Value;

            _welcomeMessage = new WelcomeMessage(botClient);
            _helpMessage = new HelpMessage(botClient);
            _formatslist = new FormatsList(botClient);
            _startConversionSession = new StartConversionSession(botClient);
            _botStatus = new BotStatus(_botConfig,botClient);
            _cancelCurrentOperation = new CancelCurrentOperation(botClient);
            _mainComands = new MainComands(botClient, _botConfig, _logger);

            _currentFormat = new CurrentFormat(botClient, _logger, _userSession);
            _handleDocument = new HandleDocument(botClient, _logger, _userSession);
            _documentDowloaded = new DocumentDowloaded(botClient, _userSession, _logger);
        }
        public async Task HandlerMessageAsync(Telegram.Bot.Types.Message message, CancellationToken cancellationToken)
        {
            var chatId = message.Chat.Id;
            var userId = message.From?.Id ?? 0;
            var username = message.From?.Username ?? "Anonymous";

            _logger.LogInformation($"Сообщение от @{username} (ID: {userId}) в чате {chatId}");

            if (!_userSession.ContainsKey(chatId))
            {
                _userSession[chatId] = new Models.UserSession { ChatId = chatId };
            }

            var session = _userSession[chatId];
            session.LastActivity = DateTime.UtcNow;


            if (!string.IsNullOrEmpty(message.Text))
            {
                await HandleTextMessageAsync(message, chatId, cancellationToken);
                return;
            }


            if (message.Document != null)
            {
                session.state = Models.UserState.WaitingForFile;

                string emailToUse = !string.IsNullOrEmpty(session.Email) ? session.Email : "";
                ConvertStart convertStart = new ConvertStart(_botClient, _userSession, message.Document, _logger);
                await convertStart.HadleUserInputAsync(chatId, cancellationToken);


                session.Email = null;
                return;
            }

            if (message.Photo != null)
            {
                session.state = Models.UserState.WaitingForFile;
                DowloadImage dowload = new DowloadImage(_botClient, _userSession, _logger);
                await dowload.ImageDowloadedAsync(message,cancellationToken,chatId,_logger);
            }

            await _botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Отправьте файл или используйте команды. /help - список команд",
                cancellationToken: cancellationToken
            );
        }

        private async Task HandleTextMessageAsync(Telegram.Bot.Types.Message message, long chatId, CancellationToken cancellationToken)
        {
            string text = message.Text.Trim();
            var session = _userSession[chatId];


            if (text.StartsWith("/"))
            {
                MainComands mainmethid = new MainComands(_botClient, _botConfig, _logger);
                await mainmethid.HandleComandAsync(chatId, text.ToLower(), cancellationToken);
                return;
            }


            if (text.Contains("@"))
            {

                session.Email = text;
                session.state = Models.UserState.WaitingForFile;

                await _botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: $"✅ Email сохранен: {text}\n📤 Теперь отправьте файл для конвертации и отправки на почту.",
                    cancellationToken: cancellationToken);
                return;
            }


            await _botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "📤 Отправьте файл для конвертации.\n📧 Или отправьте email (например: user@mail.com) для получения файла на почту.",
                cancellationToken: cancellationToken);
        }

        public async Task HandleCallbackQueryAsync(CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            var chatId = callbackQuery.Message.Chat.Id;
            var callbackData = callbackQuery.Data;
            switch (callbackData)
            {
                case "/sendmail":
                    StartSendCinvertation convert = new StartSendCinvertation(_botClient);
                    await convert.StartConversionSessionAsyncc(chatId, cancellationToken);
                    await _botClient.AnswerCallbackQueryAsync(
                    callbackQueryId: callbackQuery.Id,
                    cancellationToken: cancellationToken);
                    break;
                case "/word":
                    string format1 = "pdf1";
                    var session1 = _userSession[chatId];
                    string filepath1 = session1.CurrentFilePath;
                        SimpleFactory factory1 = new SimpleFactory(_botClient, _userSession, _logger);
                        Formats formats1 = factory1.createProduct(format1, chatId, filepath1, cancellationToken);
                        await formats1.CurrentFormats(format1, chatId, filepath1, cancellationToken);
                        await _botClient.AnswerCallbackQueryAsync(
                        callbackQueryId: callbackQuery.Id,
                        cancellationToken: cancellationToken);
                        break;
                case "/txt":
                    string format2 = "txt";
                    var session2 = _userSession[chatId];
                    string filepath2 = session2.CurrentFilePath;
                    SimpleFactory factory2 = new SimpleFactory(_botClient, _userSession, _logger);
                    Formats formats2 = factory2.createProduct(format2, chatId, filepath2, cancellationToken);
                    await formats2.CurrentFormats(format2, chatId, filepath2, cancellationToken);
                    await _botClient.AnswerCallbackQueryAsync(
                    callbackQueryId: callbackQuery.Id,
                    cancellationToken: cancellationToken);
                    break;
                case "/HTML":
                    string format3 = "pdf3";
                    var session3 = _userSession[chatId];
                    string filepath3 = session3.CurrentFilePath;
                    SimpleFactory factory3 = new SimpleFactory(_botClient, _userSession, _logger);
                    Formats formats3 = factory3.createProduct(format3, chatId, filepath3, cancellationToken);
                    await formats3.CurrentFormats(format3, chatId, filepath3, cancellationToken);
                    await _botClient.AnswerCallbackQueryAsync(
                    callbackQueryId: callbackQuery.Id,
                    cancellationToken: cancellationToken);
                    break;
                case "/Lite":
                    int quality1 = 90;
                    int complevel1 = 2;
                    var session4 = _userSession[chatId];
                    string filepath4 = session4.CurrentFilePath;
                    СompressionImages compress = new СompressionImages(_botClient);
                    var convertpath = await compress.Compressions(filepath4, quality1, complevel1,cancellationToken,chatId);
                    CompressionsImagesSend compresedfile = new CompressionsImagesSend(_botClient, _logger,_userSession);
                    await compresedfile.SendImageToChatAsync(chatId, convertpath, cancellationToken);
                   await _botClient.AnswerCallbackQueryAsync(
                    callbackQueryId: callbackQuery.Id,
                    cancellationToken: cancellationToken);
                    break;
                case "/Middle":
                    int quality = 60;
                    int complevel2 = 6;
                    var session5 = _userSession[chatId];
                    string filepath5 = session5.CurrentFilePath;
                    СompressionImages compress2 = new СompressionImages(_botClient);
                    var convertpath2 = await compress2.Compressions(filepath5, quality, complevel2, cancellationToken, chatId);
                    CompressionsImagesSend compresedfile2 = new CompressionsImagesSend(_botClient, _logger, _userSession);
                    await compresedfile2.SendImageToChatAsync(chatId, convertpath2, cancellationToken);
                    await _botClient.AnswerCallbackQueryAsync(
                   callbackQueryId: callbackQuery.Id,
                   cancellationToken: cancellationToken);
                    break;
                case "/Stronger":
                    int quality3 = 40;
                    int complevel3 = 8;
                    var session6 = _userSession[chatId];
                    string filepath6 = session6.CurrentFilePath;
                    СompressionImages compress3 = new СompressionImages(_botClient);
                    var convertpath3 = await compress3.Compressions(filepath6, quality3, complevel3, cancellationToken, chatId);
                    CompressionsImagesSend compresedfile3 = new CompressionsImagesSend(_botClient, _logger, _userSession);
                    await compresedfile3.SendImageToChatAsync(chatId, convertpath3, cancellationToken);
                    await _botClient.AnswerCallbackQueryAsync(
                   callbackQueryId: callbackQuery.Id,
                   cancellationToken: cancellationToken);
                    break;

                case "/pdf":
                    string format4 = "docx";
                    var session7 = _userSession[chatId];
                    string filepath7 = session7.CurrentFilePath;
                    SimpleFactory factory4 = new SimpleFactory(_botClient, _userSession, _logger);
                    Formats formats4 = factory4.createProduct(format4, chatId, filepath7, cancellationToken);
                    await formats4.CurrentFormats(format4, chatId, filepath7, cancellationToken);
                    await _botClient.AnswerCallbackQueryAsync(
                    callbackQueryId: callbackQuery.Id,
                    cancellationToken: cancellationToken);
                    break;
            }
        }
    }
}
