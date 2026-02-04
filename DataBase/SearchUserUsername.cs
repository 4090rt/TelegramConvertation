using Microsoft.Extensions.Caching.Memory;
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
    public class DataUser
    {
        public string UserName { get; set; }
        public DateTime DateLasdCommand { get; set; }
    }

    public class NoLock
    {
        public bool NolockUsing { get; set; }
        public bool Logging { get; set; }
    }
    public  class SearchUserUsername
    {
        private readonly Microsoft.Extensions.Caching.Memory.IMemoryCache _cachememory;
        private readonly Microsoft.Extensions.Logging.ILogger _logger;
        private bool _IsValid;

        public SearchUserUsername(Microsoft.Extensions.Caching.Memory.IMemoryCache cachememory, Microsoft.Extensions.Logging.ILogger logger)
        { 
            _cachememory = cachememory;
            _logger = logger;
            Task.Run(async () => await Inithialize()).ConfigureAwait(false);
        }

        public async Task Inithialize()
        {
            if (_IsValid) return;

            bool result = await ProverkaIndex();

            if (result == false)
            {
               await IndexCreate();
               await ProverkaIndex();
            }
            _IsValid = true;
        }

        public async Task<List<DataUser>> Cache(string name, int page, int pagecount, NoLock options)
        {
            string cachekey = $"User_key_From{name}+{DateTime.UtcNow}";

            if (_cachememory.TryGetValue(cachekey, out List<DataUser> cachekeyuser))
            {
                _logger.LogInformation($"📦 Данные из кэша для {cachekeyuser}");
                return cachekeyuser;
            }
            _logger.LogInformation($"🗃️ Запрос к БД для {cachekey}");
            var result = await CacheRequest(name, page, pagecount, options);
            var optionscache = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
                .SetSlidingExpiration(TimeSpan.FromMinutes(3));

            _cachememory.Set(cachekey, result, optionscache);

            return result;
        }

        public async Task<List<DataUser>> CacheRequest(string name, int page, int pagecount, NoLock options)
        {
            if (options == null)
            { 
                options = new NoLock();
            }
            PoolSqlConnection open = new PoolSqlConnection();
            SqlConnection connection = null;
            try
            {
                connection = open.PoolOpen();
                string nolock = options.NolockUsing ? "WITH(NOLOCK)" : "";
                string command = $"SELECT UserName, DateLasdCommand FROM LastCommandUser {nolock} WHERE Username = @U ORDER BY DateLasdCommand DESC, UserName ASC OFFSET @offset Rows FETCH NEXT @pahesize ROWS ONLY";
                var item = new List<DataUser>();

                if (options.Logging == true)
                {
                    _logger.LogInformation($"🔧 Query options: UseNoLock={options.NolockUsing}, Date={DateTime.UtcNow}");
                }
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                using (var sqlcommand = new SqlCommand(command, connection))
                {
                    sqlcommand.Parameters.AddWithValue("@U", name);

                    using (var result = await sqlcommand.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        int rowcount = 0;

                        while (await result.ReadAsync())
                        {
                            string user = result.GetString(0);
                            DateTime dateTime = result.GetDateTime(1);
                            rowcount++;
                            _logger.LogInformation($"{user}, {dateTime}");
                            rowcount++;

                            var data = new DataUser
                            {
                                UserName = user,
                                DateLasdCommand = dateTime
                            };
                            item.Add(data);
                        }
                        _logger.LogInformation($"найдено {rowcount} записей за {stopwatch.ElapsedMilliseconds}мс");
                    }
                }
                stopwatch.Stop();
                return item;
            }
            catch (Exception ex)
            {
                _logger.LogError("Возникло исключение " + ex.Message + ex.StackTrace);
                return new List<DataUser>();
            }
            finally
            {
                if (connection != null)
                {
                    open.PoolClose(connection);
                }
            }
        }

        public async Task IndexCreate()
        {
            PoolSqlConnection open = new PoolSqlConnection();
            SqlConnection connection = null;
            try
            {
                connection = open.PoolOpen();
                string command = @"
                    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_LastCommandUser_FROMUSER' AND object_id = OBJECT_ID('LastCommandUser'))
                        CREATE INDEX IX_LastCommandUser_FROMUSER ON LastCommandUser(UserName)
                        INCLUDE (DateLasdCommand)";
                using (var sqlcommand = new SqlCommand(command, connection))
                {
                    await sqlcommand.ExecuteNonQueryAsync().ConfigureAwait(false);
                    _logger.LogInformation("Индекс создан!");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Возникло исключение" + ex.Message + ex.StackTrace);
            }
            finally
            {
                if (connection != null)
                {
                    open.PoolClose(connection);
                }
            }
        }

        public async Task<bool> ProverkaIndex()
        {
            PoolSqlConnection open = new PoolSqlConnection();
            SqlConnection connection = null;
            try
            {
                connection = open.PoolOpen();
                string command = "SELECT 1 FROM sys.indexes WHERE name = 'IX_LastCommandUser_FROMUSER' AND object_id = OBJECT_ID('LastCommandUser')";

                using (var sqlcommand = new SqlCommand(command, connection))
                { 
                   var result = await sqlcommand.ExecuteScalarAsync().ConfigureAwait(false);
                   bool exec = Convert.ToInt32(result) == 1;

                    if (exec)
                    {
                        _logger.LogInformation($"✅ Индекс '{result}' существует!");
                        return true;
                    }
                    else
                    {
                        _logger.LogInformation($"❌ Индекс 'IX_COM_DateLasdCommand' не найден");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Возникло исключение" + ex.Message + ex.StackTrace);
                return false;
            }
            finally
            {
                if (connection != null)
                {
                    open.PoolClose(connection);
                }
            }
        }
    }
}
