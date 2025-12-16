using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
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
                var host = CreateHostBuilder(args).Build();
                Console.WriteLine("✅ Конфигурация загружена");
                Console.WriteLine("✅ Сервисы зарегистрированы");
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
                config.SetBasePath(Directory.GetCurrentDirectory());// базовая папка
                config.AddJsonFile("jsconfig1.json", optional: false, reloadOnChange: true);// берем файл json

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
                //Регистрируем BotConfig в IOptions<BotConfig>
                services.Configure<BotConfig>(
                    context.Configuration.GetSection("TelegramBot"));
                // 📌 Это связывает секцию "TelegramBot" из JSON с классом BotConfig
                // Позволяет использовать IOptions<BotConfig> в конструкторах

                //Регистрируем бота как синглтон
                services.AddSingleton<ITelegramBotClient>(sp =>
                {
                    var config = sp.GetRequiredService<IOptions<BotConfig>>();
                    return new TelegramBotClient(config.Value.BotToken);
                });

                // Регистрируем обработчик команд
                services.AddSingleton<CommandHandler.CommandHandlerr>();
                //Регистрируем фоновую службу
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
