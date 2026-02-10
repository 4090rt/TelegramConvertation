using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net.Http;
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
using TelegramConvertorBots.CompressionRatios;
using TelegramConvertorBots.CompressionsImages;
using TelegramConvertorBots.DataBase;
using TelegramConvertorBots.Filters;
using TelegramConvertorBots.HandleDOcumentAsync;
using TelegramConvertorBots.HttpBlock;
using TelegramConvertorBots.ImageEnchaner;
using TelegramConvertorBots.ImageUP;
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
        private readonly Microsoft.Extensions.Caching.Memory.IMemoryCache _memoryCache;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly Microsoft.Extensions.Logging.ILogger<ParsedClass> _loggerparsed;
        private readonly Microsoft.Extensions.Logging.ILogger<InfoBotCachingRequests> _loggerinfo;
        public CommandHandlerr(
            //Берем классы и их настройки и присваиваем их в переменные - использование настроек с классами
            ITelegramBotClient botClient,
            IOptions<Models.BotConfig> config,
            ILogger<CommandHandlerr> logger,
            Microsoft.Extensions.Caching.Memory.IMemoryCache memoryCache,
            IHttpClientFactory httpClientFactory,
            Microsoft.Extensions.Logging.ILogger<ParsedClass> loggerparsed,
            Microsoft.Extensions.Logging.ILogger<InfoBotCachingRequests> loggerinfo
            )
        {
            //присваиваем значения к глобальным переменным
            _botClient = botClient;
            _userSession = new Dictionary<long, Models.UserSession>();
            _loggerparsed = loggerparsed;
            _loggerinfo = loggerinfo;
            _logger = logger;
            _botConfig = config.Value;
            _httpClientFactory = httpClientFactory;
            _welcomeMessage = new WelcomeMessage(botClient);
            _helpMessage = new HelpMessage(botClient);
            _formatslist = new FormatsList(botClient);
            _startConversionSession = new StartConversionSession(botClient);
            _botStatus = new BotStatus(_botConfig,botClient);
            _cancelCurrentOperation = new CancelCurrentOperation(botClient);
            _mainComands = new MainComands(_botClient,_botConfig,_logger,_memoryCache,_httpClientFactory,_loggerparsed,_loggerinfo);

            _currentFormat = new CurrentFormat(botClient, _logger, _userSession);
            _handleDocument = new HandleDocument(botClient, _logger, _userSession);
            _documentDowloaded = new DocumentDowloaded(botClient, _userSession, _logger);
            _memoryCache =  memoryCache;

        }
        public async Task HandlerMessageAsync(Telegram.Bot.Types.Message message, CancellationToken cancellationToken)
        {
            var chatId = message.Chat.Id;
            var userId = message.From?.Id ?? 0;
            var username = message.From?.Username ?? "Anonymous";
            var datetime = DateTime.UtcNow; 

            string info = $"Сообщение от @{username} (ID: {userId}) в чате {chatId}";
            SaveUsersClass save = new SaveUsersClass(_logger);
            await save.UserAdd(info);

            SaveLastCommand savelast = new SaveLastCommand(_logger);
            await savelast.AddLastCommand(username, datetime);

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
                MainComands mainmethid = new MainComands(_botClient, _botConfig, _logger, _memoryCache, _httpClientFactory, _loggerparsed, _loggerinfo);
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
                case "/Compression":
                  Compressionretes compressionretes = new Compressionretes(_botClient);
                    await compressionretes.ButtonsImage(chatId,cancellationToken);
                  await _botClient.AnswerCallbackQueryAsync(
                      callbackQueryId: callbackQuery.Id,
                      cancellationToken: cancellationToken);
                    break;

                case "/Enchanner":
                    ButtonEnchanmest buttonEnchanmest = new ButtonEnchanmest(_botClient);
                       await buttonEnchanmest.ImageButtonsvariants(chatId,cancellationToken);
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
                    var convertpath = await compress.Compressions(filepath4, quality1, complevel1, cancellationToken, chatId);
                    CompressionsImagesSend compresedfile = new CompressionsImagesSend(_botClient, _logger, _userSession);
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
                    await compresedfile3.SendImageToChatAsync(chatId, convertpath3,cancellationToken);
                    await _botClient.AnswerCallbackQueryAsync(
                   callbackQueryId: callbackQuery.Id,
                   cancellationToken: cancellationToken);
                    break;

                case "/Low":
                    double parametr1 = 1.7;
                    double strenght1 = 0.5;
                    int levelsh1 = 3;
                    var session8 = _userSession[chatId];
                    string filepath8 = session8.CurrentFilePath;
                    EnchannerImage image1 = new EnchannerImage(_botClient, _logger);
                    var convertpath4 = await image1.Enchanners(filepath8, EnhancementLevel.Low, cancellationToken,chatId); 
                    UPImage uPImage1 = new UPImage(_botClient, _logger);
                    var controlpath1 = await uPImage1.ImagesUPMin(convertpath4, parametr1);
                    UpImage2Rez upImage2Rezq1 = new UpImage2Rez(_botClient, _logger);
                    var controlpath1_1 = await upImage2Rezq1.ImagesUPMin(controlpath1, strenght1);
                    UpImageSHUM upImageSHUM1 = new UpImageSHUM(_botClient, _logger);
                    var controlpath2_1 = await upImageSHUM1.ImagesUPMin(controlpath1_1, levelsh1);
                    EnchannerSend send1 = new EnchannerSend(_botClient, _logger, _userSession);
                    await send1.SendImageToChatAsync(chatId, controlpath2_1,cancellationToken);
                    await _botClient.AnswerCallbackQueryAsync(
                    callbackQueryId: callbackQuery.Id,
                    cancellationToken: cancellationToken);
                    break;

                case "/Medium":
                    double parametr2 = 1.9;
                    double strenght2 = 1.0;
                    int levelsh2 = 3;
                    var session9 = _userSession[chatId];
                    string filepath9 = session9.CurrentFilePath;
                    EnchannerImage image2 = new EnchannerImage(_botClient, _logger);
                    var convertpath5 = await image2.Enchanners(filepath9, EnhancementLevel.Medium, cancellationToken, chatId);
                    UPImage uPImage = new UPImage(_botClient, _logger);
                    var controlpath2 = await uPImage.ImagesUPMiddle(convertpath5, parametr2);
                    UpImage2Rez upImage2Rezw2 = new UpImage2Rez(_botClient, _logger);
                    var controlpath1_2 = await upImage2Rezw2.ImagesUPMin(controlpath2, strenght2);
                    UpImageSHUM upImageSHUM2 = new UpImageSHUM(_botClient, _logger);
                    var controlpath2_2 = await upImageSHUM2.ImagesUPMin(controlpath1_2, levelsh2);
                    EnchannerSend send = new EnchannerSend(_botClient, _logger, _userSession);
                    await send.SendImageToChatAsync(chatId, controlpath2_2, cancellationToken);
                    await _botClient.AnswerCallbackQueryAsync(
                    callbackQueryId: callbackQuery.Id,
                    cancellationToken: cancellationToken);
                    break;

                case "/High":
                    double parametr3 = 2.5;
                    double strenght3 = 1.5;
                    int levelsh3 = 3;
                    var session10 = _userSession[chatId];
                    string filepath10 = session10.CurrentFilePath;
                    EnchannerImage image3 = new EnchannerImage(_botClient, _logger);
                    var convertpath6 = await image3.Enchanners(filepath10, EnhancementLevel.High, cancellationToken, chatId);
                    UPImage uPImage3 = new UPImage(_botClient, _logger);
                    var controlpath3 = await uPImage3.ImagesUPMiddle(convertpath6, parametr3);
                    UpImage2Rez upImage2Rezw3 = new UpImage2Rez(_botClient, _logger);
                    var controlpath1_3 = await upImage2Rezw3.ImagesUPMin(controlpath3, strenght3);
                    UpImageSHUM upImageSHUM3 = new UpImageSHUM(_botClient, _logger);
                    var controlpath3_3 = await upImageSHUM3.ImagesUPMin(controlpath1_3, levelsh3);
                    EnchannerSend send3 = new EnchannerSend(_botClient, _logger, _userSession);
                    await send3.SendImageToChatAsync(chatId, controlpath3_3, cancellationToken);
                    await _botClient.AnswerCallbackQueryAsync(
                    callbackQueryId: callbackQuery.Id,
                    cancellationToken: cancellationToken);
                    break;

                case "/Filters":
                    FilterButton but = new FilterButton(_botClient);
                    await but.FilterButtons(chatId, cancellationToken);
                    await _botClient.AnswerCallbackQueryAsync(
                    callbackQueryId: callbackQuery.Id,
                    cancellationToken: cancellationToken);
                    break;

                case "/pencil":
                    var session11 = _userSession[chatId];
                    string filepath11 = session11.CurrentFilePath;
                    Filterpenc pencil = new Filterpenc(_botClient, _logger);
                    var resultpenc = await pencil.PencFilter(filepath11);
                    EnchannerSend send4 = new EnchannerSend(_botClient, _logger, _userSession);
                    await send4.SendImageToChatAsync(chatId, resultpenc, cancellationToken);
                    await _botClient.AnswerCallbackQueryAsync(
                    callbackQueryId: callbackQuery.Id,
                    cancellationToken: cancellationToken);
                    break;

                case "/multi":
                    var session12 = _userSession[chatId];
                    string filepath12 = session12.CurrentFilePath;
                    Filtermult multi = new Filtermult(_botClient, _logger);
                    var resultmult = await multi.MultFilter(filepath12);
                    EnchannerSend send5 = new EnchannerSend(_botClient, _logger, _userSession);
                    await send5.SendImageToChatAsync(chatId, resultmult, cancellationToken);
                    await _botClient.AnswerCallbackQueryAsync(
                    callbackQueryId: callbackQuery.Id,
                    cancellationToken: cancellationToken);
                    break;
                case "/aquarel":
                    var session13 = _userSession[chatId];
                    string filepath13 = session13.CurrentFilePath;
                    FilterFqua aqua = new FilterFqua(_botClient, _logger);
                    var resultaqua = await aqua.FquaFilter(filepath13);
                    EnchannerSend send6 = new EnchannerSend(_botClient, _logger, _userSession);
                    await send6.SendImageToChatAsync(chatId, resultaqua, cancellationToken);
                    await _botClient.AnswerCallbackQueryAsync(
                    callbackQueryId: callbackQuery.Id,
                    cancellationToken: cancellationToken);
                    break;
                case "/Oil":
                    var session14 = _userSession[chatId];
                    string filepath14 = session14.CurrentFilePath;
                    FilterOil oil = new FilterOil(_botClient, _logger);
                    var resultoil= await oil.OilFilter(filepath14);
                    EnchannerSend send7 = new EnchannerSend(_botClient, _logger, _userSession);
                    await send7.SendImageToChatAsync(chatId, resultoil, cancellationToken);
                    await _botClient.AnswerCallbackQueryAsync(
                    callbackQueryId: callbackQuery.Id,
                    cancellationToken: cancellationToken);
                    break;
            }
        }
    }
}
