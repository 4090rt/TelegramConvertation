using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramConvertorBots.DataBase
{
    public class AddUserCommands
    {
        private readonly Microsoft.Extensions.Logging.ILogger _logger;

        public AddUserCommands(Microsoft.Extensions.Logging.ILogger logger)
        { 
            _logger = logger;
        }

        public async Task AddUser()
        {
            PoolSqlConnection pool = new PoolSqlConnection();
            SqlConnection connect = null;

            try
            {
                connect = pool.PoolOpen();
                string sqlcommand = "SELECT * FROM USERCOMMANDS";

                using (var commandnew = new SqlCommand(sqlcommand, connect))
                {
                    using (var result = await commandnew.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        while (await result.ReadAsync())
                        { 
                            string user = result.GetString(0);

                            _logger.LogInformation($"{user}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Возникло исключение при попытке вывода информации о юзере" + ex.Message + ex.StackTrace);
            }
            finally
            {
                pool.PoolClose(connect);
            }
        }
    }
}
