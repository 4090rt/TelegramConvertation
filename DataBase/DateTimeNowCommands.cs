using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Caching;
using Telegram.Bot.Types;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace TelegramConvertorBots.DataBase
{
    public class DateTimeNow
    {
        public string UserName { get; set; }
        public DateTime DateLasdCommand { get; set; }
    }

    public class NoLockOptions2
    { 
        public bool UsingLock { get; set; }
        public bool Logging { get; set; }
    }

    public class DateTimeNowCommands
    {
        private readonly Microsoft.Extensions.Caching.Memory.IMemoryCache _memoryCache;
        private readonly Microsoft.Extensions.Logging.ILogger _logger;
        private bool _ischeked = false;
        public DateTimeNowCommands(Microsoft.Extensions.Caching.Memory.IMemoryCache memoryCache, Microsoft.Extensions.Logging.ILogger logger) 
        {
            _memoryCache = memoryCache;
            _logger = logger;

            Task.Run(async () => await Inthializate()).ConfigureAwait(false);
        }

        public async Task Inthializate()
        {
            if (_ischeked) return;

            bool cheked = await  ProverkaINdex();

            if (cheked == false)
            {
                await CreateIndex();
                await ProverkaINdex();
            }

            _ischeked = true;
        }

        public async Task<List<DateTimeNow>> Cache(DateTime date, int pagecount, int page, NoLockOptions optionsLock)
        {
            string keycache = $"datetime_{DateTime.UtcNow}";

            if (_memoryCache.TryGetValue(keycache, out List<DateTimeNow> cache))
            {
                _logger.LogInformation($"📦 Данные из кэша для {cache}");
                return cache;
            }
            _logger.LogInformation($"🗃️ Запрос к БД для {cache}");
            List<DateTimeNow> datecache = await CacheRequest(date,pagecount,page,optionsLock);

            var cacheoptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
                .SetSlidingExpiration(TimeSpan.FromMinutes(2));

            _memoryCache.Set(keycache, datecache, cacheoptions);

            return datecache;
        }

        public async Task<List<DateTimeNow>> CacheRequest(DateTime date, int pagecount, int page, NoLockOptions optionsLock)
        {
            int offset = (1 - page) * pagecount;
            if (optionsLock == null)
            { 
                optionsLock = new NoLockOptions();
            }

            PoolSqlConnection open = new PoolSqlConnection();
            SqlConnection connection = null;

            try
            {
                connection = open.PoolOpen();

                var items = new List<DateTimeNow>();
                string nolockstirng = optionsLock.NolockUsing ? "WITH(NOLOCK)" : "";
                string command = $"SELECT UserName, DateLasdCommand FROM LastCommandUser {nolockstirng} WHERE DateLasdCommand = @D ORDER BY DateLasdCommand DESC, UserName ASC OFFSET @offset Rows FETCH NEXT @pagesize ROWS ONLY";

                if (optionsLock.Logging == true)
                {
                    _logger.LogInformation($"🔧 Query options: UseNoLock={optionsLock.NolockUsing}, Date={date}");
                }

                var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                using (var sqlcommand = new SqlCommand(command, connection))
                {
                    sqlcommand.Parameters.AddWithValue("@D", date);
                    sqlcommand.Parameters.AddWithValue("@offset", offset);
                    sqlcommand.Parameters.AddWithValue("@pagesize", pagecount);

                    using (var result = await sqlcommand.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        int Rowcount = 0;

                        while (await result.ReadAsync())
                        {
                            string username = result.GetString(0);
                            DateTime datetime = result.GetDateTime(1);
                            Rowcount++;
                            _logger.LogInformation($"{username}, {datetime}");

                            var objects = new DateTimeNow
                            {
                               UserName = username,
                               DateLasdCommand = datetime
                            };
                            items.Add(objects);
                        }
                        _logger.LogInformation($"найдено {Rowcount} записей за {stopwatch.ElapsedMilliseconds}мс");
                    }
                }
                stopwatch.Stop();
                return items;
            }
            catch (Exception ex)
            {
                _logger.LogError("Возникло исключение " + ex.Message + ex.StackTrace);
                return new List<DateTimeNow>();
            }
            finally
            {
                if (connection != null)
                {
                    open.PoolClose(connection);
                }
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
                    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_LastCommandUser_FROMDATETIMENOW' AND object_id = OBJECT_ID('LastCommandUser'))
                        CREATE INDEX IX_LastCommandUser_FROMDATETIMENOW ON LastCommandUser(DateLasdCommand)
                        INCLUDE (UserName)"; ;
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

        public async Task<bool> ProverkaINdex()
        {
            PoolSqlConnection open = new PoolSqlConnection();
            SqlConnection connection = null;

            try
            {
                connection = open.PoolOpen();
                string command = "SELECT 1 FROM sys.indexes WHERE name = 'IX_LastCommandUser_FROMDATETIMENOW' AND object_id = OBJECT_ID('LastCommandUser')";

                using (var sqlcommand = new SqlCommand(command, connection))
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
                if (connection != null)
                    open.PoolClose(connection);
            }
        }
    }
}

