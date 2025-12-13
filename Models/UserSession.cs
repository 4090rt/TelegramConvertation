using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace TelegramConvertorBots.Models
{
    // модель данных пользовательской сессии
    public class UserSession
    {
        public long ChatId { get; set; }
        public UserState state { get; set; }
        public string CurrentFilePath { get; set; }
        public string CurrentFilaName { get; set; }
        public string CurrentFormat { get; set; }
        public DateTime ConvertationTime { get; set; } = DateTime.UtcNow;
        public DateTime LastActivity { get; set; } = DateTime.UtcNow;

    }
    // перечень состояний пользователя
    public enum UserState
    {
        Idle,                 // Ничего не делает
        WaitingForFile,       // Ждет файл от пользователя
        WaitingForFormat,     // Ждет выбор формата
        Processing,           // В процессе конвертации
        WaitingForUrl         // Ждет URL от пользователя
    }
    // модель данных результатов конвертации
    public class Convertresults
    { 
        public long ChatID { get; set; }
        public string InputPath { get; set; } = string.Empty;
        public string OutputFormat { get; set; } = string.Empty;
        public string OriginalFileName { get; set; }
        public DateTime RequestTime { get; set; } = DateTime.UtcNow;
    }
}
