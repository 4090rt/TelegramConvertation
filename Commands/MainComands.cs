using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using TelegramConvertorBots.Models;

namespace TelegramConvertorBots.Commands
{
    public class MainComands
    {
        private readonly ITelegramBotClient _botClient;
        private readonly Models.BotConfig _botConfig;
        private readonly Dictionary<long, Models.UserSession> _userSession;
        private readonly Microsoft.Extensions.Logging.ILogger _logger;

        public MainComands(ITelegramBotClient botClient, Models.BotConfig botConfig, Microsoft.Extensions.Logging.ILogger logger)
        {
            _botClient = botClient;
            _botConfig = botConfig;
            _userSession = new Dictionary<long, Models.UserSession>();
            _logger = logger;
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
