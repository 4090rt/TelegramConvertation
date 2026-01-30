using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramConvertorBots.DataBase
{
    public class SaveLastCommand
    {
        private readonly Microsoft.Extensions.Logging.ILogger _logger;

        public SaveLastCommand(Microsoft.Extensions.Logging.ILogger logger)
        {
            _logger = logger;
        }

        public async Task AddLastCommand(string user,DateTime date)
        {
            PoolSqlConnection pool = new PoolSqlConnection();
            SqlConnection connection = null;

            try
            {
                connection = pool.PoolOpen();
                string comand = "INSERT INTO [LastCommandUser] (UserName, DateLasdCommand) VALUES (@U, @D)";

                using (var sqlcommand = new SqlCommand(comand, connection))
                {
                    sqlcommand.Parameters.AddWithValue("@U", user);
                    sqlcommand.Parameters.AddWithValue("@D", date);
                    await sqlcommand.ExecuteNonQueryAsync().ConfigureAwait(false);

                    _logger.LogInformation("Дата последней команды пользователя сохранена!");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("ВОзникло исключени" + ex.Message + ex.StackTrace);
            }
            finally
            {
                pool.PoolClose(connection);
            }

        }
    }
}
