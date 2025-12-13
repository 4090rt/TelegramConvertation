using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramConvertorBots.Models
{
    // модель данных настроек бота
    public class BotConfig
    {
        public string BotToken { get; set; } = string.Empty;
        public long AdminId { get; set; }
        public long MaxFileSize { get; set; } = 50 * 1024 * 1024; // 50 MB
        public string[] SupportedFormats { get; set; } = Array.Empty<string>();
    }
}
