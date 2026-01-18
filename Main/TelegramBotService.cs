using HarfBuzzSharp;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramConvertorBots.CompressionsImages;

namespace TelegramConvertorBots.Main
{
    public class TelegramBotService : IHostedService
    {        
        private readonly ITelegramBotClient _botClient;
        private readonly ILogger<TelegramBotService> _logger;
        private readonly Models.BotConfig _config;
        private readonly CommandHandler.CommandHandlerr _commandHandlerr;

        //присваиваем значения
        public TelegramBotService(IOptions<Models.BotConfig> config, ILogger<TelegramBotService> logger, CommandHandler.CommandHandlerr commandHandler)
        {
            _config = config.Value;
            _logger = logger;
            _commandHandlerr = commandHandler;
            _botClient = new TelegramBotClient(_config.BotToken);

            _logger.LogInformation("TelegramBotService создан");
        }
        // метод запуска бота и обновления сообщений и нажатий по кнопка
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // информация в лог
            _logger.LogInformation("Запуск Telegram бота...");

            // объект ReceiverOptions - обновление настроек  -    AllowedUpdates  - какие настройки - 
            var receiverOptions = new Telegram.Bot.Polling.ReceiverOptions
            {
                AllowedUpdates = new[]
                {
                    // обновляем пользовтаельские сообщения
                    UpdateType.Message,
                    // обновляем сообщения-ответы на inline-кнопки
                    UpdateType.CallbackQuery
                },
                // отбрасывает все накопленные сообщения за время отключения бота и работает только с новыми
                ThrowPendingUpdates = true,
            };
            // запуск прослушивания 
            _botClient.StartReceiving(
                 updateHandler: HandleUpdateAsync,// Метод для обработки обновлений
                 pollingErrorHandler: HandlePollingErrorAsync,// Метод для обработки обновлений
                 receiverOptions: receiverOptions,// Настройки выше
                 cancellationToken: cancellationToken   // Токен отмены
             );

            try
            {
                // etMeAsync - проверка подключения и вывод информации
                var ME = await _botClient.GetMeAsync(cancellationToken);
                _logger.LogInformation($"Бот @{ME.Username} запущен успешно!");
                _logger.LogInformation($"ID бота: {ME.Id}");
                _logger.LogInformation($"Имя бота: {ME.FirstName}");
            }
            // иначе
            catch (Exception ex)
            {
                _logger.LogError(ex, "Не удалось получить информацию о боте");
                throw;
            }
        }
        // обработка типов обновления
        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                //блок свитч
                switch (update.Type)
                {
                    // если тип сообщение то вызываем метод обработки сообщений
                    case UpdateType.Message:
                        await _commandHandlerr.HandlerMessageAsync(update.Message, cancellationToken);
                        break;
                    // если тип CallbackQuery сообщение то вызываем метод обработки CallbackQuery сообщений
                    case UpdateType.CallbackQuery:
                        // 1. Отвечаем на callback (чтоб убрать часики на кнопке)
                        await _botClient.AnswerCallbackQueryAsync(
                            callbackQueryId: update.CallbackQuery.Id,
                            cancellationToken: cancellationToken
                        );

                        // 2. Отправляем сообщение в чат
                        var progressMsg = await _botClient.SendTextMessageAsync(
                            chatId: update.CallbackQuery.Message.Chat.Id,
                            text: "⏳ Обрабатываю ваш запрос...",
                            cancellationToken: cancellationToken
                        );

                        await _commandHandlerr.HandleCallbackQueryAsync(update.CallbackQuery, cancellationToken);
                        break;

                    default:
                        _logger.LogDebug($"Необработанный тип обновления: {update.Type}");
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка обработки обновления");
            }
        }
        // обработка  сообщений об ошибке
        public Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellation)
        {
            string errorMessage;
            switch (exception)
            {
                case ApiRequestException apiRequestException:
                    errorMessage = $"Telegram API Error: {apiRequestException.ErrorCode} - {apiRequestException.Message}";
                    break;

                default:
                    errorMessage = exception.ToString();
                    break;
            }
            _logger.LogError(errorMessage);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Остановка Telegram бота...");
            return Task.CompletedTask;
        }
    }
}
