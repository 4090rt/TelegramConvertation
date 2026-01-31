using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace TelegramConvertorBots.DataBase
{
    public class NolockClass
    { 
       public bool NoLock { get; set; }
       public bool LoggingNokock { get; set; }
    }
    public class DateTimeNowCommands
    {
        private readonly Microsoft.Extensions.Logging.ILogger _logger;
        private bool _ischekedindex = false;

        public DateTimeNowCommands(Microsoft.Extensions.Logging.ILogger logger)
        { 
            _logger = logger;
            Task.Run(async () => await Initialization()).ConfigureAwait(false);
        }

        public async Task Initialization()
        {
            if (_ischekedindex) return;

            bool eses = await IndexProverka();

            if (!eses)
            {
                await CreateIndex();
                await IndexProverka();
            }
            _ischekedindex= true;
        }

        public async Task CreateSqlRequest(DateTime datetime, NolockClass options)
        {
            if (options == null)
            {
                options = new NolockClass();
            }
            PoolSqlConnection open = new PoolSqlConnection();
            SqlConnection connect = null;

            try
            {
                connect = open.PoolOpen();

                if (options.LoggingNokock == true)
                {
                    _logger.LogInformation($"🔧 Query options: UseNoLock={options.LoggingNokock}, Date={datetime}");
                }

                string nolockcom = options.NoLock ? "WITH(NOLOCK)" : "";
                string command = $"SELECT UserName, DateLasd,Command FROM LastCommandUser {nolockcom} WHERE DateLasdCommand = @D";

                var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                using (var sqlcommand = new SqlCommand(command, connect))
                {
                    sqlcommand.Parameters.AddWithValue("@D", datetime);
                    using (var results = await sqlcommand.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        if (results != null)
                        {
                            int rowcount = 0;
                            while (await results.ReadAsync())
                            {
                                string username = results.GetString(0);
                                DateTime dateTime = results.GetDateTime(1);
                                rowcount++;
                                _logger.LogInformation($"{username}, {dateTime}");
                            }
                            _logger.LogInformation($"найдено {rowcount} записей за {stopwatch.ElapsedMilliseconds}мс");
                        }
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
                if (connect != null)
                    open.PoolClose(connect);
            }
        }

        public async Task CreateIndex()
        {
            PoolSqlConnection open = new PoolSqlConnection();
            SqlConnection connection = null;
            try
            {
                connection = open.PoolOpen();
                string command = @"
                    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_LastCommandUser_DateLasdCommand' AND object_id = OBJECT_ID('LastCommandUser'))
                        CREATE INDEX IX_LastCommandUser_DateLasdCommand ON LastCommandUser(DateLasdCommand)
                        INCLUDE (UserName)";

                using (var sqlcommand = new SqlCommand(command, connection))
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
                if (connection != null)
                    open.PoolClose(connection);
            }
        }

        public async Task<bool> IndexProverka()
        {
            PoolSqlConnection open = new PoolSqlConnection();
            SqlConnection connect = null;
            try
            {
                connect = open.PoolOpen();
                string command = "SELECT 1 FROM sys.indexes WHERE name = 'IX_LastCommandUser_DateLasdCommand' AND object_id = OBJECT_ID('LastCommandUser')";

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
                            _logger.LogInformation($"❌ Индекс 'IX_COM_DateLasdCommand' не найден");
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
                    open.PoolClose(connect);
            }
        }
    }
}
