using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace TelegramConvertorBots.DataBase
{
    public  class SearchUserUsername
    {
        private readonly Microsoft.Extensions.Logging.ILogger _logger;
        private bool _isInitialized = false;
        public SearchUserUsername(Microsoft.Extensions.Logging.ILogger logger)
        { 
            _logger = logger;
        }

        public async Task Inichializate()
        {
            if (_isInitialized) return;
            await CreateIndex();
            await IndexProverka();
        }
        public async Task SearchUser(string username)
        {
            PoolSqlConnection pool = new PoolSqlConnection();
            SqlConnection connect = new SqlConnection();
            try
            {
                connect = pool.PoolOpen();
                Task.Run(async () => await Inichializate()).Wait();
                string command = "SELECT UserName, DateLasdCommand FROM LastCommandUser WHERE UserName = @U";

                using (var sqlcommand = new SqlCommand(command, connect))
                { 
                    sqlcommand.Parameters.AddWithValue("@U", username);
                    using (var result = await sqlcommand.ExecuteReaderAsync())
                    {
                        while (await result.ReadAsync())
                        { 
                            string usernames = result.GetString(0);
                            DateTime date = result.GetDateTime(1);

                            _logger.LogInformation($"{username} + {date}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Возникло исключение " + ex.Message + ex.StackTrace);
            }
            finally
            {
                pool.PoolClose(connect);
            }
        }

        public async Task CreateIndex()
        {
            PoolSqlConnection pool = new PoolSqlConnection();
            SqlConnection connect = new SqlConnection();
            try
            {
                connect = pool.PoolOpen();
                string command = @"
                    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_LastCommandUser_Username' AND object_id = OBJECT_ID('LastCommandUser'))
                        CREATE INDEX IX_LastCommandUser_Username ON LastCommandUser(Username)";

                using (var sqlcommand = new SqlCommand(command, connect))
                { 
                    await sqlcommand.ExecuteNonQueryAsync().ConfigureAwait(false);
                    _logger.LogInformation("Индекс создан!");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Возникло исключение " + ex.Message + ex.StackTrace);
            }
            finally
            {
                pool.PoolClose(connect);
            }
        }

        private async Task IndexProverka()
        {
            PoolSqlConnection pool = new PoolSqlConnection();
            SqlConnection connect = new SqlConnection();
            try
            {
                connect = pool.PoolOpen();
                string command = "SELECT 1 FROM sys.indexes WHERE name = 'IX_LastCommandUser_Username' AND object_id = OBJECT_ID('LastCommandUser')";

                using (var sqlcommand = new SqlCommand(command, connect))
                { 
                   var result = await sqlcommand.ExecuteScalarAsync().ConfigureAwait(false) as string;

                    if (!string.IsNullOrEmpty(result))
                    {
                        _logger.LogInformation($"✅ Индекс '{result}' существует!");
                    }
                    else
                    {
                        _logger.LogInformation($"❌ Индекс 'IX_COM_User' не найден");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Возникло исключение " + ex.Message + ex.StackTrace);
            }
            finally
            {
                pool.PoolClose(connect);
            }
        }
    }
}
