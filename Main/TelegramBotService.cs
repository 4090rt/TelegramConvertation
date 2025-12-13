using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums; 


namespace TelegramConvertorBots.Main
{
    public class TelegramBotService : IHostedService
    {        
        private readonly ITelegramBotClient _botClient;
        private readonly ILogger<TelegramBotService> _logger;
        private readonly Models.BotConfig _config;
        private readonly CommandHandler.CommandHandlerr _commandHandler;
    }
}
