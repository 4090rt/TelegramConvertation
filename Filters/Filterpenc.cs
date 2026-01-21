using Microsoft.Extensions.Logging;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace TelegramConvertorBots.Filters
{
    public class Filterpenc
    {
        private readonly ITelegramBotClient _botclient;
        private readonly Microsoft.Extensions.Logging.ILogger _logger;

        public Filterpenc(ITelegramBotClient botClient, Microsoft.Extensions.Logging.ILogger logger)
        { 
            _botclient = botClient;
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

        public async Task<string> PencFilter(string filepath)
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
                    Mat colormat = new Mat();
                    string outpath = GetOutputPath(filepath);

                    Cv2.PencilSketch(src, mat, colormat, 30, 0.9f);
                    Cv2.ImWrite(outpath, mat);

                    _logger.LogInformation("Фильтр успешно применен");
                    return outpath;
                }
            }
            catch(Exception ex)
            {
                _logger.LogError("Не удалось применить фильтр" + ex.Message);
                return null;
            }
        }
    }
}
