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
    public class QueryNolock
    {
        public bool UseNoLock { get; set; } = true;
        public bool LogDetails { get; set; } = true;
    }

    public  class SearchUserUsername
    {
        private readonly Microsoft.Extensions.Logging.ILogger _logger;
        private bool _isInitialized = false;
        public SearchUserUsername(Microsoft.Extensions.Logging.ILogger logger)
        { 
            _logger = logger;
            Task.Run(async () => await Inichializate()).ConfigureAwait(false);
        }

        public async Task Inichializate()
        {
            if (_isInitialized) return;

            bool ises = await IndexProverka();

            if (!ises)
            {
                await CreateIndex();
                await IndexProverka();
            }

            _isInitialized = true;
        }
        public async Task SearchUser(string username, QueryNolock options = null)
        {
            if (options == null)
            {
               options =  new QueryNolock();
            }
            PoolSqlConnection pool = new PoolSqlConnection();
            SqlConnection connect = new SqlConnection();
            try
            {
                connect = pool.PoolOpen();

                if (options.LogDetails == true)
                {
                    _logger.LogInformation(
                      $"🔧 Query options: UseNoLock={options.UseNoLock}, User={username}");
                }

                string nolockclass = options.UseNoLock ?"WITH(NOLOCK)" : "";
                string command = $"SELECT UserName, DateLasdCommand FROM LastCommandUser {nolockclass} WHERE UserName = @U";

                var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                using (var sqlcommand = new SqlCommand(command, connect))
                { 
                    sqlcommand.Parameters.AddWithValue("@U", username);
                    using (var result = await sqlcommand.ExecuteReaderAsync())
                    {
                        int rowcount = 0;
                        while (await result.ReadAsync())
                        { 
                            string usernames = result.GetString(0);
                            DateTime date = result.GetDateTime(1);
                            rowcount++;

                            _logger.LogInformation($"{username} + {date}");
                        }
                        _logger.LogInformation($"Поиск пользователя '{username}': найдено {rowcount} записей за {stopwatch.ElapsedMilliseconds}мс");
                    }
                }
                stopwatch.Stop();
            }
            catch (Exception ex)
            {
                _logger.LogError("Возникло исключение " + ex.Message + ex.StackTrace);
            }
            finally
            {
                if(connect != null)
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
                    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_LastCommandUser_UserName' AND object_id = OBJECT_ID('LastCommandUser'))
                        CREATE INDEX IX_LastCommandUser_UserName ON LastCommandUser(UserName)
                        INCLUDE (DateLasdCommand)";

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
                if (connect != null)
                pool.PoolClose(connect);
            }
        }

        private async Task<bool> IndexProverka()
        {
            PoolSqlConnection pool = new PoolSqlConnection();
            SqlConnection connect = new SqlConnection();
            try
            {
                connect = pool.PoolOpen();
                string command = "SELECT 1 FROM sys.indexes WHERE name = 'IX_LastCommandUser_UserName' AND object_id = OBJECT_ID('LastCommandUser')";

                using (var sqlcommand = new SqlCommand(command, connect))
                { 
                   var result = await sqlcommand.ExecuteScalarAsync().ConfigureAwait(false);
                    if (result != null && result != DBNull.Value)
                    {
                        bool exists = Convert.ToInt32(result) == 1;

                        if (exists)
                        {
                            _logger.LogInformation($"✅ Индекс '{result}' существует!");
                        }
                        else
                        {
                            _logger.LogInformation($"❌ Индекс 'IX_COM_User' не найден");
                        }
                        return exists;
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Возникло исключение " + ex.Message + ex.StackTrace);
                return false;
            }
            finally
            {
                if (connect != null)
                    pool.PoolClose(connect);
            }
        }
    }
}
