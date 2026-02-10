using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using TelegramConvertorBots.DataBase;
using TelegramConvertorBots.HttpBlock;
using TelegramConvertorBots.Models;

namespace TelegramConvertorBots.Commands
{
    public class MainComands
    {
        private readonly ITelegramBotClient _botClient;
        private readonly Models.BotConfig _botConfig;
        private readonly Dictionary<long, Models.UserSession> _userSession;
        private readonly Microsoft.Extensions.Logging.ILogger _logger;
        private readonly Microsoft.Extensions.Caching.Memory.IMemoryCache _memoryCache;
        private readonly Microsoft.Extensions.Logging.ILogger<InfoBotCachingRequests> _loggerinfo;
        private readonly Microsoft.Extensions.Logging.ILogger<ParsedClass> _loggerPars;
        private readonly IHttpClientFactory _httpClientFactory;

        public MainComands(ITelegramBotClient botClient, Models.BotConfig botConfig, Microsoft.Extensions.Logging.ILogger logger, Microsoft.Extensions.Caching.Memory.IMemoryCache memoryCache,
            IHttpClientFactory httpClientFactory, Microsoft.Extensions.Logging.ILogger<ParsedClass> loggerPars, Microsoft.Extensions.Logging.ILogger<InfoBotCachingRequests> loggerinfo)
        {
            _botClient = botClient;
            _botConfig = botConfig;
            _userSession = new Dictionary<long, Models.UserSession>();
            _logger = logger;
            _memoryCache = memoryCache;
            _httpClientFactory = httpClientFactory;
            _loggerPars = loggerPars;
            _loggerinfo = loggerinfo;
        }

        public async Task HandleComandAsync(long chatId, string command, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Обработка команды: {command} от чата {chatId}");

            switch (command)
            {
                case "/start":
                    WelcomeMessage welcome = new WelcomeMessage(_botClient);
                    await welcome.SendWelcomeMessageAsync(chatId, cancellationToken);
                    break;

                case "/help":
                    HelpMessage help = new HelpMessage(_botClient);
                    await help.SendHelpMessageAsync(chatId, cancellationToken);
                    break;

                case "/formats":
                    FormatsList formatsList = new FormatsList(_botClient);
                    await formatsList.SendFormatsListAsync(chatId, cancellationToken);
                    break;

                case "/convert":
                    StartConversionSession startConversionSession = new StartConversionSession(_botClient);
                    await startConversionSession.StartConversionSessionAsync(chatId, cancellationToken);
                    break;

                case "/status":
                    BotStatus botStatus = new BotStatus(_botConfig, _botClient);
                    await botStatus.SendBotStatusAsync(chatId, cancellationToken);
                    break;

                case "/cancel":
                    CancelCurrentOperation cancel = new CancelCurrentOperation(_botClient);
                    await cancel.CancelCurrentOperationAsync(chatId, cancellationToken);
                    break;
                case "/sendmail":
                    SendMail send = new SendMail(_botClient);
                    await send.SendMails(chatId, cancellationToken);
                    break;
                case "/compression":
                    Startofcompression start = new Startofcompression(_botClient);
                    await start.SendStartofcompression(chatId, cancellationToken);
                    break;
                case "/all":
                    SelectAllUsersCommand commandall = new SelectAllUsersCommand(_logger);
                    await commandall.AllUsers();
                    break;
                case "/bot":
                    string url = "https://ipinfo.io/json";
                    InfoBotCachingRequests request = new InfoBotCachingRequests(_loggerinfo, _loggerPars, _httpClientFactory, _memoryCache);
                    var result = await request.CachingRrquest(url,cancellationToken);

                    await _botClient.SendTextMessageAsync (
                        chatId: chatId,
                        text: $"🏠 Мой Сервер находится здесь:\n  {result.timezone}  {result.country} \n  {result.region} \n  {result.city},\n 🇷🇺 Я РУССКИЙ, ОТЕЧЕСТВЕННЫЙ БОТ.\n⚒️ Я работаю на процессоре INTEL XEON E5 2680,\n 🚚Привезенным и купленым в дружественном государстве ",
                        cancellationToken: cancellationToken
                        );
                    break;
                //case "/search":
                //    string username = "lilchicfgt";
                //    SearchUserCommand search = new SearchUserCommand(_logger);
                //    await search.SeachingUser(username);
                //    break;
                case "/DateNow":
                    DateTime date = DateTime.Now;
                    var optionss = new NoLockOptions
                    {
                        NolockUsing = true,
                        Logging = true
                    };
                    DateTimeNowCommands commandnew = new DateTimeNowCommands(_memoryCache, _logger);
                    await commandnew.Cache(date, 10 ,1, optionss);
                    break;

                default:
                    await _botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "Неизвестная команда. Используйте /help для списка команд",
                        cancellationToken: cancellationToken
                        );
                    break;
            }
        }
    }
}
