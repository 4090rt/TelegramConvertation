using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace TelegramConvertorBots.HttpBlock
{
    public class InfoBotCachingRequests
    {
        private readonly Microsoft.Extensions.Caching.Memory.IMemoryCache _memorycache;
        private readonly Microsoft.Extensions.Logging.ILogger<InfoBotCachingRequests> _logger;
        private readonly Microsoft.Extensions.Logging.ILogger<ParsedClass> _loggerPars;
        private readonly IHttpClientFactory _httpClientFactory;

        public InfoBotCachingRequests(Microsoft.Extensions.Logging.ILogger<InfoBotCachingRequests> logger,
           Microsoft.Extensions.Logging.ILogger<ParsedClass> loggerPars,
           IHttpClientFactory httpClientFactory,
           Microsoft.Extensions.Caching.Memory.IMemoryCache memorycache)
        {
            _memorycache = memorycache;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _loggerPars = loggerPars;
        }

        public async Task<Model> CachingRrquest(string url, CancellationToken cancellation = default)
        {
            var key_cache = $"key_cache_{url}";

            if (_memorycache.TryGetValue(key_cache, out Model cache))
            {
                _logger.LogInformation($"📦 Данные из кэша для {key_cache}");
                return cache;
            }
            try
            {
                _logger.LogInformation("Начинаю запрос");

                var result = await Rrquest(url, cancellation);

                var options = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(10))
                    .SetSlidingExpiration(TimeSpan.FromMinutes(8));

                _memorycache.Set(key_cache, result, options); 
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("Возникло исключение" + ex.Message + ex.StackTrace);
                return new Model();
            }
        }

        public async Task<Model> Rrquest(string url, CancellationToken cancellation = default)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("ApiclientInfiBots");

                _logger.LogInformation("Начинаю запрос");
                HttpResponseMessage recpon = await client.GetAsync(url).ConfigureAwait(false);
                if (recpon.IsSuccessStatusCode)
                {
                    if (recpon != null)
                    {
                        try
                        {
                            _logger.LogInformation("Читаю ответ");
                            var content = await recpon.Content.ReadAsStreamAsync().ConfigureAwait(false);
                            _logger.LogInformation("Отве прочитан");

                            _logger.LogInformation("Начинаю парсинг");
                            ParsedClass parsed = new ParsedClass(_loggerPars);
                            var result = await parsed.Parse(content);
                            _logger.LogInformation("Парсинг закончен");

                            return result;
                        }
                        catch
                        {
                            _logger.LogError("Возникло исключение при парсинге");
                            return new Model();
                        }
                    }
                    else
                    {
                        _logger.LogError("Не удалось найти ответ");
                        return new Model();
                    }
                }
                else
                {
                    _logger.LogError("При выполнении запроса возникла ошибка. Статус код:" + recpon.StatusCode);
                    return new Model();
                }
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError("Операция отменена" + ex.Message + ex.StackTrace);
                return new Model();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError("Возникло исключение при выполнении запроса" + ex.Message + ex.StackTrace);
                return new Model();
            }
            catch (Exception ex)
            {
                _logger.LogError("Возникло исключение" + ex.Message + ex.StackTrace);
                return new Model();
            }
        }
    }
}
