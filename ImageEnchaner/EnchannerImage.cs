using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;

namespace TelegramConvertorBots.ImageEnchaner
{
    public class EnchannerImage
    {
        public readonly ITelegramBotClient _botclient;
        private readonly Microsoft.Extensions.Logging.ILogger _logger;
        public EnchannerImage(ITelegramBotClient botClient, Microsoft.Extensions.Logging.ILogger logger)
        {
            _botclient = botClient;
            _logger = logger;
        }

        public async Task<string> Enchanners(string filepath, EnhancementLevel level, CancellationToken canceltoken, long chatid)
        {
            try
            {
                using (var image = SixLabors.ImageSharp.Image.Load(filepath))
                {
                    image.Mutate(x => ApplyEnhancements(x, level));

                    string outpath = GetOutputPath(filepath, "enchanners");
                    await Task.Run(() => image.Save(outpath));
                    _logger.LogInformation("");
                    return outpath;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Возникло исключение" + ex.Message);
                return null;
            }
        }

        private string GetOutputPath(string inputPath, string suffix)
        {
            string dir = Path.GetDirectoryName(inputPath);
            string name = Path.GetFileNameWithoutExtension(inputPath);
            string ext = Path.GetExtension(inputPath);

            return Path.Combine(dir, $"{name}_{suffix}{ext}");
        }

        private void ApplyEnhancements(IImageProcessingContext context, EnhancementLevel level)
        {
            switch (level)
            { 
                case EnhancementLevel.Low:
                    context.Contrast(1.1f)   // Контраст +10%
                    .Brightness(1.05f)        // +5% яркость
                    .Saturate(1.1f);  // +10% насыщенность
                    break;
                case EnhancementLevel.Medium:
                    context.Contrast(1.2f)
                        .Brightness(1.1f)// Яркость +5%
                        .Saturate(1.12f) // +10% насыщенность
                        .GaussianSharpen(0.8f);   // Легкая резкость
                    break;
                case EnhancementLevel.High:
                    context.Contrast(1.3f)
                    .Brightness(1.12f)
                    .Saturate(1.3f)
                    .GaussianSharpen(1.2f);
                    //.Vignette(Colo, 0.4f, 0.6f);// Виньетирование
                    break;

            }
        }
    }
}
