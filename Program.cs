using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using TelegramConvertorBots.DataBase;
using TelegramConvertorBots.Logs;
using TelegramConvertorBots.Main;
using TelegramConvertorBots.Models;

namespace TelegramConvertorBots
{
    //Основной класс
    class Program
    {
        //Основной метод
        static async Task Main(string[] args)
        {
            Console.WriteLine("🚀 Запуск Telegram Converter Bot...");

            try
            {
                var hostBuilder = CreateHostBuilder(args);

                // ДОПОЛНИТЕЛЬНО настраиваем сервисы
                hostBuilder.ConfigureServices((context, services) =>
                {
                    services.AddLogging(configure =>
                    {
                        configure.ClearProviders();
                        configure.AddConsole();
                        configure.AddDebug();
                        configure.SetMinimumLevel(LogLevel.Information);
                    });
                    // Добавляем кэш
                    services.AddMemoryCache();

                    // Регистрируем FromDate
                    services.AddSingleton<FromDate>();
                    services.AddSingleton<SearchUserUsername>();

                    services.AddMemoryCache();

                    services.AddHttpClient("TelegramClient", client1 =>
                    {
                        client1.Timeout = TimeSpan.FromSeconds(60);
                        client1.DefaultRequestHeaders.Accept.ParseAdd("application/json");
                        client1.DefaultRequestHeaders.AcceptLanguage.ParseAdd("ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");
                        client1.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip, deflate, br");
                        client1.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
                    })
                   .AddTransientHttpErrorPolicy(policy => // Circuit Breaker
                       policy.CircuitBreakerAsync(
                           handledEventsAllowedBeforeBreaking: 5,
                           durationOfBreak: TimeSpan.FromSeconds(30)))
                   .AddTransientHttpErrorPolicy(pollu => // Retry
                       pollu.WaitAndRetryAsync(3, retry =>
                           TimeSpan.FromSeconds(Math.Pow(2, retry))))
                   .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler // Auto Parsing
                   {
                       AutomaticDecompression = System.Net.DecompressionMethods.GZip |
                               System.Net.DecompressionMethods.Deflate
                   });
                });

                var host = hostBuilder.Build();


                Console.WriteLine("✅ Конфигурация загружена");
                Console.WriteLine("✅ Сервисы зарегистрированы");


                // Тестируем сервис ДО запуска хоста
                using (var scope = host.Services.CreateScope())
                {
                    var fromDateService = scope.ServiceProvider.GetRequiredService<FromDate>();
                    var options = new NoLockOptions
                    {
                        NolockUsing = true,
                        Logging = true
                    };

                    var data = await fromDateService.CacheReq(
                        DateTime.UtcNow,
                        options,
                        page: 1,
                        pagesize: 10);

                    Console.WriteLine($"✅ Тест сервиса: получено {data.Count} записей");
                }

                Console.WriteLine("✅ Запускаем хост...");
                await host.RunAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Критическая ошибка: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine("\nНажмите любую клавишу для выхода...");
                Console.ReadKey();
            }
        }
        static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args).
            ConfigureAppConfiguration((context, config) => // Создаем хост с аргументами командной строки
            {
                // настройка конфигурации
                //config.SetBasePath(Directory.GetCurrentDirectory());// базовая папка
                //config.AddJsonFile("jsconfig1.json", optional: false, reloadOnChange: true);// берем файл json если нет falseoptional: false
                //reloadOnChange: если измнаенился во время работы - перезапускаем конфигурацию
                // добавление переменных окружения
                config.AddEnvironmentVariables();


                //Добавляем аргументы командной строки
                if (args != null)
                {
                    config.AddCommandLine(args);
                }
            })
            .ConfigureServices((context,services) =>
            {
                // тут мы с помощью Configuration.GetSElection выбираем информацию о боте из Json файла и передаем
                // в модель данных BotConfig, для дальнейшего использования в коде
                services.Configure<BotConfig>(
                    context.Configuration.GetSection("TelegramBot"));

                //Регистрируем бота как синглтон
                services.AddSingleton<ITelegramBotClient>(sp =>
                {
                    var config = sp.GetRequiredService<IOptions<BotConfig>>();
                    return new TelegramBotClient(config.Value.BotToken);
                });

                // Регистрируем обработчик команд
                services.AddSingleton<CommandHandler.CommandHandlerr>();
                //Регистрируем фоновую службу, AddHostedService добавляем  - он служит как связь между тг апи и ботом
                // и передает команды из тг в Main класс где обрабатываются файлы и команды
                services.AddHostedService<TelegramBotService>();

               // Регистрируем словарь для сессий пользователей
                services.AddSingleton<Dictionary<long, UserSession>>();

                Console.WriteLine("✅ Сервисы сконфигурированы");
            })
             .ConfigureLogging((context, logging) =>
             { 
             
                logging.ClearProviders();
                 logging.AddConfiguration(context.Configuration.GetSection("Logging"));

                 logging.AddConsole();
                 logging.AddDebug();

                 Console.WriteLine("✅ Логирование настроено");
             })
            .UseConsoleLifetime();
    }
}
