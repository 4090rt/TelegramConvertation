using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramConvertorBots.DataBase
{
    public class SaveUsersClass
    {
        private readonly Microsoft.Extensions.Logging.ILogger _logger;

        public SaveUsersClass(Microsoft.Extensions.Logging.ILogger logger)
        { 
            _logger = logger;
        }

        public async Task UserAdd(string Info)
        {
            PoolSqlConnection pool = new PoolSqlConnection();
            SqlConnection connect = null;
            try
            {
                connect = pool.PoolOpen();
                string command = "INSERT INTO [USERCOMMANDS] ([User]) VALUES (@U)";

                using (var commandsql = new SqlCommand(command, connect))
                { 
                    commandsql.Parameters.AddWithValue("@U", Info);
                    await commandsql.ExecuteNonQueryAsync().ConfigureAwait(false);

                    _logger.LogInformation("Успешной сохранено");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Возникло исключение" + ex.Message + ex.StackTrace);
            }
            finally
            {
                pool.PoolClose(connect);
            }
        }
    }
}
