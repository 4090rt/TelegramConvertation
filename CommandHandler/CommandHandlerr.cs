using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramConvertorBots.Commands;
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

            _convertStart = new ConvertStart(botClient);
            _currentFormat = new CurrentFormat(botClient);
            _handleDocument = new HandleDocument(botClient, _logger);
            _documentDowloaded = new DocumentDowloaded(botClient);
        }
        public async Task HandlerMessageAsync( Telegram.Bot.Types.Message message, CancellationToken cancellationToken)
        {
            var chatId = message.Chat.Id;
            var userId = message.From?.Id ?? 0;
            var username = message.From?.Username ?? "Anonymous";

            _logger.LogInformation($"Сообщение от @{username} (ID: {userId}) в чате {chatId}");

            if (!string.IsNullOrEmpty(message.Text))
            {
                await HandleTextMessageAsync(message, chatId,  cancellationToken);
                return;
            }

            if (message.Document != null)
            {
                ConvertStart convertStart = new ConvertStart(_botClient);
                await convertStart.HadleUserInputAsync(chatId, cancellationToken);
            }

            await _botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Отправьте файл или используйте команды./help - список команд",
                cancellationToken: cancellationToken
            );
        }

        private async Task HandleTextMessageAsync(Telegram.Bot.Types.Message message, long chatId,CancellationToken cancellationToken)
        {
            string text = message.Text;
            // Проверяем, является ли текст командой
            char a = '/';
            if (text.StartsWith(a.ToString()))
            {
                MainComands mainmethid = new MainComands(_botClient,_botConfig,_logger);
                await mainmethid.HandleComandAsync(chatId, text.ToLower(), cancellationToken);
            }
            else
            {
                ConvertStart convertStart = new ConvertStart(_botClient);
                await convertStart.HadleUserInputAsync(chatId, cancellationToken);
            }
        }

        public Task HandleCallbackQueryAsync(CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            // Заглушка для обработки callback-запросов
            return Task.CompletedTask;
        }
    }
}
