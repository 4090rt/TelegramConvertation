using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramConvertorBots.Models;

namespace TelegramConvertorBots.CommandHandler
{
    public class CommandHandlerr
    {
        //Создаем глобальные переменные 
        public readonly ITelegramBotClient _botClient;
        public readonly Dictionary<long, Models.UserSession> _userSession;
        public readonly Models.BotConfig _botConfig;
        public readonly ILogger _logger;

        public CommandHandlerr(
            //Берем классы и их настройки и присваиваем их в переменные - использование настроек с классами
            ITelegramBotClient botClient,
            IOptions<Models.BotConfig> config,
            ILogger<CommandHandlerr> logger)
        {
            //присваиваем значения к глобальным переменным
            _botClient = botClient;
            _userSession = new Dictionary<long, Models.UserSession>();
            _logger = logger;
            _botConfig = config.Value;

        }

        // главнй метод взаимодействия, получаем из message свойства и передаем  в операторы для дальнейшего использования в метеодах
        public async Task HandlerMessageAsync(Telegram.Bot.Types.Message message, CancellationToken cancellationToken)
        {
            //Свойства
            var chatId = message.Chat.Id;
            var userId = message.From?.Id ?? 0;
            var username = message.From?.Username ?? "Anonymous";
            //  отображаем в логах
            _logger.LogInformation($"Сообщение от @{username} (ID: {userId}) в чате {chatId}");
            // метод для работы с текстовыми сообщениями
            if (!string.IsNullOrEmpty(message.Text))
            {
                await HandleTextMessageAsync(chatId, message.Text, cancellationToken);
                return;
            }
            // метод для работы с документами 
            if (message.Document != null)
            {
                await HandleDocumentAsync(chatId, message.Document, cancellationToken);
            }
            // иначе
            await _botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Отправьте файл или используйте команды./help - список команд",
                cancellationToken: cancellationToken
            );
        }

        // проверяем, что отправил пользователь, если команда -  в метод для работы с командами - если нет, то возможно, что это фалй - в метод принимающий файлы
        private async Task HandleTextMessageAsync(long chatId, string text, CancellationToken cancellationToken)
        {
            // Проверяем, является ли текст командой
            char a = '/';
            if (text.StartsWith(a.ToString()))
            {
                await HandleComandAsync(chatId, text.ToLower(), cancellationToken);
            }
            //иначе в метод приемки файлов
            else
            {
                await HandleUserInputAsync(chatId, text, cancellationToken);
            }
        }

        //основной метод  для работы с командами
        public async Task HandleComandAsync(long chatId, string command, CancellationToken cancellationToken)
        {
            // отображение в логи
            _logger.LogInformation($"Обработка команды: {command} от чата {chatId}");
            // сопоставление команд и методов
            switch (command)
            {
                case "/start":
                    await SendWelcomeMessageAsync(chatId, cancellationToken);
                    break;

                case "/help":
                    await SendHelpMessageAsync(chatId, cancellationToken);
                    break;

                case "/formats":
                    await SendFormatsListAsync(chatId, cancellationToken);
                    break;

                case "/convert":
                    await StartConversionSessionAsync(chatId, cancellationToken);
                    break;

                case "/status":
                    await SendBotStatusAsync(chatId, cancellationToken);
                    break;

                case "/cancel":
                    await CancelCurrentOperationAsync(chatId, cancellationToken);
                    break;

                // иначе
                default:
                    await _botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "Неизвестная команда. Используйте /help для списка команд",
                        cancellationToken: cancellationToken
                        );
                    break;
            }
        }

        // метод команды Welcom
        private async Task SendWelcomeMessageAsync(long chatId, CancellationToken cancellationToken)
        {
            // текст команды
            var welcomeText =
                "👋 Добро пожаловать в File Converter Bot!\n\n" +
                "Я могу конвертировать файлы между различными форматами.\n\n" +
                "📋 Основные команды:\n" +
                "/convert - Начать конвертацию файла\n" +
                "/formats - Показать поддерживаемые форматы\n" +
                "/help - Полный список команд\n" +
                "/cancel - Отменить текущую операцию\n\n" +
                "📤 Просто отправьте мне файл или используйте команду /convert";
            // отображение
            await _botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: welcomeText,
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken);
        }

        // метод команды Help
        private async Task SendHelpMessageAsync(long chatId, CancellationToken cancellationToken)
        {
            // текст команды
            var helpText =
                "📖 <b>File Converter Bot - Справка</b>\n\n" +
                "🔄 <b>Конвертация файлов:</b>\n" +
                "1. Используйте /convert или просто отправьте файл\n" +
                "2. Выберите целевой формат\n" +
                "3. Получите конвертированный файл\n\n" +
                "📋 <b>Поддерживаемые форматы:</b>\n" +
                "• PDF → DOCX, JPG, PNG, TXT\n" +
                "• DOCX → PDF, TXT\n" +
                "• JPG/PNG → PDF\n" +
                "• TXT → PDF, DOCX\n\n" +
                "⚡ <b>Основные команды:</b>\n" +
                "/start - Начальное приветствие\n" +
                "/convert - Начать конвертацию\n" +
                "/formats - Список форматов\n" +
                "/status - Статус бота\n" +
                "/cancel - Отмена операции\n\n" +
                "📝 <b>Ограничения:</b>\n" +
                "• Макс. размер файла: 50MB\n" +
                "• Поддерживаются документы и изображения\n" +
                "• Конвертация может занять некоторое время";
            // отображение 
            await _botClient.SendTextMessageAsync(
                chatId: chatId,
                text: helpText,
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
        }
        // метод команды  доступных форматов
        private async Task SendFormatsListAsync(long chatId, CancellationToken cancellationToken)
        {
            // текст команды
            var formatsText =
                "📊 <b>Поддерживаемые форматы конвертации:</b>\n\n" +
                "🔸 <b>Из PDF в:</b>\n" +
                "   • DOCX (Word документ)\n" +
                "   • JPG (изображение)\n" +
                "   • PNG (изображение)\n" +
                "   • TXT (текстовый файл)\n\n" +
                "🔸 <b>Из DOCX в:</b>\n" +
                "   • PDF (документ)\n" +
                "   • TXT (текстовый файл)\n\n" +
                "🔸 <b>Из изображений (JPG/PNG) в:</b>\n" +
                "   • PDF (документ)\n\n" +
                "🔸 <b>Из TXT в:</b>\n" +
                "   • PDF (документ)\n" +
                "   • DOCX (Word документ)\n\n" +
                "💡 <b>Как использовать:</b>\n" +
                "1. Отправьте файл или используйте /convert\n" +
                "2. Введите целевой формат (например: docx)\n" +
                "3. Получите результат";
            // добавление кнопки, по нажатию на которую начинается процесс конвертации
            var keyboard = new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton[] { "🚀 Начать конвертацию (/convert)" }
            })
            {
                // автоматическое измеенение размеров под экран пользователя
                ResizeKeyboard = true,
                // скрываем кнпоку после нажатия
                OneTimeKeyboard = true
            };
            // отображение
            await _botClient.SendTextMessageAsync(
            chatId: chatId,
            text: formatsText,
            parseMode: ParseMode.Html,
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);
        }
        // метож выбора формата файла 1 этап конвертации
        private async Task StartConversionSessionAsync(long chatId, CancellationToken cancellationToken)
        {
            // проверяем, есть ли сессия с текущим id чата
            if (!_userSession.ContainsKey(chatId))
            {
                // если нет - создаем
                _userSession[chatId] = new Models.UserSession { ChatId = chatId };
            }
            //настраиваем сессию
            // формируем объект
            var session = _userSession[chatId];
            // ставим флаг ожидания файла
            session.state = Models.UserState.WaitingForFile;
            // устанвливаем флаг активности - текущае время 
            session.LastActivity = DateTime.UtcNow;
            // иначе 
            await _botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "📤 <b>Отправьте файл для конвертации</b>\n\n" +
                     "Поддерживаемые форматы:\n" +
                     "• PDF, DOCX, DOC\n" +
                     "• JPG, PNG, GIF, BMP\n" +
                     "• TXT\n\n" +
                     "Максимальный размер: 50MB\n" +
                     "Или используйте /cancel для отмены",
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
        }
        // метод проверки активности юзеров
        private async Task SendBotStatusAsync(long chatId, CancellationToken cancellationToken)
        {
            //  кол-во юзеров которые были онлайн менее 5 минут назад 
            var activeUsers = _userSession.Count(s =>
            (DateTime.UtcNow - s.Value.LastActivity).TotalMinutes < 5);
            // текст команды
            var statusText =
                "📊 <b>Статус бота:</b>\n\n" +
                $"• Активных пользователей: {activeUsers}\n" +
                $"• Всего сессий: {_userSession.Count}\n" +
                $"• Поддерживаемых форматов: {_botConfig.SupportedFormats.Length}\n" +
                $"• Макс. размер файла: {_botConfig.MaxFileSize / 1024 / 1024}MB\n\n" +
            // отображение
                "✅ Бот работает нормально";
            await _botClient.SendTextMessageAsync(
                chatId: chatId,
                text: statusText,
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken
                );
        }
        // метод отмены операции 
        private async Task CancelCurrentOperationAsync(long chatId, CancellationToken cancellationToken)
        {
            // проверяем есть ли текущий id в словаре
            if (_userSession.ContainsKey(chatId))
            {   // если есть то сбрасываем флаг состояния на ожидание
                _userSession[chatId].state = UserState.Idle;
                // текущий флаг 
                _userSession[chatId].CurrentFilePath = null;
                // текущий формат
                _userSession[chatId].CurrentFormat = null;
            }
            // отображение  
            await _botClient.SendTextMessageAsync(
                 chatId: chatId,
                 text: "❌ Операция отменена\n\n" +
                      "Вы можете начать заново с помощью /convert",
                 cancellationToken: cancellationToken);
        }

        // мето принимающий файл 2 этап конвертации 
        private async Task HandleUserInputAsync(long chatId, string text, CancellationToken cancellationToken)
        {
            // проверяем есть текущий id(ключе) в словаре(сессии)
            if (!_userSession.TryGetValue(chatId, out var session))
            {
                // если нет отобржение
                await _botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Используйте /convert для начала конвертации",
                    cancellationToken: cancellationToken
                    );
                return;
            }
            switch (session.state)
            {
                // если флаг равен ожиданию формата - вызываем метод определяющий 2 формат файла
                case UserState.WaitingForFormat:
                    await ProcessFormatSelectionAsync(chatId, cancellationToken, text, session).ConfigureAwait(false);
                    break;
                // иначе отображаем
                default:
                    await _botClient.SendTextMessageAsync(
                      chatId: chatId,
                      text: "Я не понимаю. Используйте /help для списка команд",
                      cancellationToken: cancellationToken);
                      break;
            }

        }
        // метод определения формата файла из доступных 3 этап кконвертации
        private async Task ProcessFormatSelectionAsync(long chatId, CancellationToken cancellationToken, string format, UserSession session)
        { 
            // приводим файл к нормально состоянию
            format = format.ToLower().Trim();
            if (format.StartsWith("."))
            {
                format = format.Substring(1);
            }
            // создаем массив с доступными форматами
            var supportsformat = new[] { "docx", "txt", "PDF", "HTML" };
            // проверяем, соответсвует ли format одному из форматов из массива
            if (!supportsformat.Contains(format))
            {
                // иначе отображаем
                await _botClient.SendTextMessageAsync(
                   chatId: chatId,
                   text: $"❌ Формат '{format}' не поддерживается.\n\n" +
                        "Доступные форматы: docx, pdf, jpg, png, txt\n\n" +
                        "Введите формат еще раз:",
                   cancellationToken: cancellationToken);
                return;
            }
            // устанавливаем флаг текущего формата на format
            session.CurrentFormat = format;
            // устанавливаем флаг состояния на в прроцессе
            session.state = UserState.Processing;
            // отображаем
            await _botClient.SendTextMessageAsync(
                 chatId: chatId,
                 text: $"🔄 Начинаю конвертацию в {format.ToUpper()}...\n\n" +
                      "Это может занять некоторое время.",
                 cancellationToken: cancellationToken);

            // Здесь будет вызов  конвертера
            // Пока просто имитируем конвертацию
            await Task.Delay(2000, cancellationToken);

            session.state = UserState.Idle;
            // отображаем
            await _botClient.SendTextMessageAsync(
                chatId: chatId,
                text: $"✅ Конвертация завершена!\n\n" +
                     "В реальной версии здесь будет файл для скачивания.\n" +
                     "Пока это демо-версия.",
                cancellationToken: cancellationToken);
        }

        // Заглушки для обработки файлов (реализуем в день 2)
        private Task HandleDocumentAsync(long chatId, Document document, CancellationToken cancellationToken)
        {
            return _botClient.SendTextMessageAsync(
                chatId: chatId,
                text: $"📄 Получен документ: {document.FileName}\n\n" +
                     "В демо-версии обработка файлов не реализована.\n" +
                     "Работающий функционал будет в день 2.",
                cancellationToken: cancellationToken);
        }

        public Task HandleCallbackQueryAsync(CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            // Заглушка для обработки callback-запросов
            return Task.CompletedTask;
        }
    }
}
