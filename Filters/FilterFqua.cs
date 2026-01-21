using Microsoft.Extensions.Logging;
using OpenCvSharp;
using OpenCvSharp.XPhoto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace TelegramConvertorBots.Filters
{
    public class FilterFqua
    {
        private readonly ITelegramBotClient _botClient;
        private readonly ILogger _logger;

        public FilterFqua(ITelegramBotClient botClient, ILogger logger)
        {
            _botClient = botClient;
            _logger = logger;
        }

        private string GetOutputPath(string inputPath)
        {
            string name = System.IO.Path.GetFileNameWithoutExtension(inputPath);
            string ext = System.IO.Path.GetExtension(inputPath);

            return System.IO.Path.Combine(
                System.IO.Path.GetDirectoryName(inputPath),
                $"{name}_upscaled_{ext}"
            );
        }

        public async Task<string> FquaFilter(string filepath)
        {
            try
            {
                _logger.LogInformation("Начался процесс применения филтра");
                using (Mat src = Cv2.ImRead(filepath))
                {
                    if (src.Empty())
                    {
                        _logger.LogWarning("Изображение не найдено");
                        return null;
                    }

                    Mat mat = new Mat();
                    string outpath = GetOutputPath(filepath);

                    Cv2.Stylization(
                           src: src,
                           dst: mat,
                           85,    // Среднее размытие
                           0.75f  // Сохраняем больше деталей
                       );

                    Cv2.ImWrite(outpath,mat);

                    _logger.LogInformation("Фильтр успешно применен");
                    return outpath;

                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Не удалось применить фильтр" + ex.Message);
                return null;
            }
        }
    }
}
