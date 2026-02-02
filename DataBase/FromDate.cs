using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace TelegramConvertorBots.DataBase
{
    public class NoLockOptions
    { 
        public bool NolockUsing { get; set; }
        public bool Logging { get; set; }
    }
    public class Data
    {
        public string UserName { get; set; }
        public DateTime DateLasdCommand { get; set; }
    }
    public class FromDate
    {
        private readonly Microsoft.Extensions.Logging.ILogger _logger;
        private bool _isvalid = false;
        private readonly Microsoft.Extensions.Caching.Memory.IMemoryCache _cache;

        public FromDate(Microsoft.Extensions.Logging.ILogger logger, Microsoft.Extensions.Caching.Memory.IMemoryCache cache)
        { 
            _logger = logger;
            _cache = cache;
            Task.Run(async () => await inithialize()).ConfigureAwait(false);
        }

        public async Task inithialize()
        {
            if (_isvalid) return;

            bool result = await ProverkaIndex();

            if (result == false)
            {
                await CreateIndex();
                await ProverkaIndex();
            }
            _isvalid = true;
        }

        public async Task<List<Data>> CacheReq(DateTime date, NoLockOptions options, int page = 1, int pagesize = 15)
        {
            string cacheKey = $"data_{date}";

            if (_cache.TryGetValue(cacheKey, out List<Data> chacheData))
            {
                _logger.LogInformation($"📦 Данные из кэша для {chacheData}");
                return chacheData;
            }
            _logger.LogInformation($"🗃️ Запрос к БД для {chacheData}");
            List<Data> result = await RequestFromFate(date, options, page, pagesize);

            var chacheoptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
                .SetSlidingExpiration(TimeSpan.FromMinutes(2));

            _cache.Set(cacheKey, result, chacheoptions);

            return result;
        }


        public async Task<List<Data>> RequestFromFate(DateTime date, NoLockOptions options, int page = 1, int pagesize = 15 )
        {
            int PaginationFormul = (page - 1) * pagesize;

            if (options == null)
            { 
                options = new NoLockOptions();
            }

            PoolSqlConnection open = new PoolSqlConnection();
            SqlConnection connect = new SqlConnection();

            try
            {
                connect = open.PoolOpen();
                var resultat = new List<Data>();
                string nolock = options.NolockUsing ? "WITH(NOLOCK)" : "";
                string command = $"SELECT UserName, DateLasdCommand FROM LastCommandUser {nolock} WHERE DateLasdCommand = @D ORDER BY DateLasdCommand DESC, UserName ASC OFFSET @offset Rows FETCH NEXT @pahesize ROWS ONLY";

                if (options.Logging == true)
                {
                    _logger.LogInformation($"🔧 Query options: UseNoLock={options.NolockUsing}, Date={date}");
                }
                
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                using (var sqlcommand = new SqlCommand(command, connect))
                {
                    sqlcommand.Parameters.AddWithValue("@D", date);
                    sqlcommand.Parameters.AddWithValue("@offset", PaginationFormul);
                    sqlcommand.Parameters.AddWithValue("@pahesize", pagesize);

                    using (var result = await sqlcommand.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        if (result != null)
                        {
                            int rowcount = 0;
                            while (await result.ReadAsync())
                            {
                                string user = result.GetString(0);
                                DateTime dateTime = result.GetDateTime(1);
                                rowcount++;
                                _logger.LogInformation($"{user}, {dateTime}");

                                var data = new Data
                                {
                                    UserName = user,
                                    DateLasdCommand = dateTime
                                };
                                resultat.Add(data);
                            }
                            _logger.LogInformation($"найдено {rowcount} записей за {stopwatch.ElapsedMilliseconds}мс");
                        }
                        else
                        {
                            return new List<Data>();
                        }
                    }
                }
                stopwatch.Stop();
                return resultat;
            }
            catch (Exception ex)
            {
                _logger.LogError("Возникло исключение " + ex.Message + ex.StackTrace);
                return new List<Data>();
            }
            finally
            {
                if (connect != null)
                {
                    open.PoolClose(connect);
                }
            }
        }

        public async Task CreateIndex()
        {

            PoolSqlConnection open = new PoolSqlConnection();
            SqlConnection connect = new SqlConnection();
            try
            {
                connect = open.PoolOpen();
                string command = @"
                    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_LastCommandUser_FROMDATE' AND object_id = OBJECT_ID('LastCommandUser'))
                        CREATE INDEX IX_LastCommandUser_FROMDATE ON LastCommandUser(DateLasdCommand)
                        INCLUDE (UserName)";

                using (var commandsql = new SqlCommand(command, connect))
                {
                    await commandsql.ExecuteNonQueryAsync().ConfigureAwait(false);
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
                    open.PoolClose(connect);
            }
        }

        public async Task<bool> ProverkaIndex()
        {

            PoolSqlConnection open = new PoolSqlConnection();
            SqlConnection connect = new SqlConnection();
            try
            {
                connect = open.PoolOpen();
                string command = "SELECT 1 FROM sys.indexes WHERE name = 'IX_LastCommandUser_FROMDATE' AND object_id = OBJECT_ID('LastCommandUser')";

                using (var commandsql = new SqlCommand(command, connect))
                {
                    var result = await commandsql.ExecuteScalarAsync().ConfigureAwait(false);

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
