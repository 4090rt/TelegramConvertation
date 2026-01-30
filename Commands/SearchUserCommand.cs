using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramConvertorBots.DataBase;

namespace TelegramConvertorBots.Commands
{
    public class SearchUserCommand
    {
        private readonly Microsoft.Extensions.Logging.ILogger _logger;

        public SearchUserCommand(Microsoft.Extensions.Logging.ILogger logger)
        { 
            _logger = logger;
        }

        public async Task SeachingUser(string Username)
        {
            SearchUserUsername searchUserCommand = new SearchUserUsername(_logger);
            await searchUserCommand.SearchUser(Username);
        }
    }
}
