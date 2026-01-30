using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramConvertorBots.DataBase;

namespace TelegramConvertorBots.Commands
{
    public class SelectAllUsersCommand
    {
        private readonly Microsoft.Extensions.Logging.ILogger _logger;

        public SelectAllUsersCommand(Microsoft.Extensions.Logging.ILogger logger)
        { 
            _logger = logger;
        }

        public async Task AllUsers()
        {
            try
            {
                AddUserCommands commands = new AddUserCommands(_logger);
                await commands.AddUser();
            }
            catch (Exception ex)
            {
                _logger.LogError("Возникло исключение" + ex.Message + ex.StackTrace);
            }
        }
    }
}
