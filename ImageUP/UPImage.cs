using Microsoft.Extensions.Logging;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace TelegramConvertorBots.ImageUP
{
    public class UPImage
    {
        private readonly ITelegramBotClient _botclient;
        private readonly ILogger _logger;


        public UPImage(ITelegramBotClient botclient, ILogger logger)
        {
            _botclient = botclient;
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

        public async Task<string> ImagesUPMin(string inputpath, double scaleFactor)
        {
            try
            {
                _logger.LogInformation("2 этап улучшения запущен");
                using (Mat src = Cv2.ImRead(inputpath))
                {

                    if (src.Empty())
                    {
                        _logger.LogError("Не удалось загрузиить изображение");
                    }

                    Mat mat = new Mat();
                    string outpath = GetOutputPath(inputpath);

                    Cv2.Resize(src, mat, new Size(), scaleFactor, scaleFactor, InterpolationFlags.Lanczos4);
                    Cv2.ImWrite(outpath, mat);

                    _logger.LogInformation("Качетсво успешно улучшено");
                    return outpath;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Ошибка улучшения: {ex.Message}");
                return null;
            }
        }

        public async Task<string> ImagesUPMiddle(string inputpath, double scaleFactor)
        {
            try
            {
                _logger.LogInformation("2 этап улучшения запущен");

                using (Mat src = Cv2.ImRead(inputpath))
                {
                    if (src.Empty())
                    {
                        _logger.LogError("Не удалось загрузиить изображение");
                    }

                    Mat mat = new Mat();
                    var outputpath = GetOutputPath(inputpath);

                    Cv2.Resize(src, mat, new Size(), scaleFactor, scaleFactor, InterpolationFlags.Lanczos4);
                    Cv2.ImWrite(outputpath, mat);

                    _logger.LogInformation("Качество успешно улучшено ");
                    return outputpath;
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"❌ Ошибка улучшения {ex.Message}");
                return null;
            }
        }

        public async Task<string> ImagesUPHigh(string inputpath, double scaleFactor)
        {
            try
            {
                _logger.LogInformation("2 этап улучшения запущен");

                using (Mat src = Cv2.ImRead(inputpath))
                {
                    if (src.Empty())
                    {
                        _logger.LogError("Не удалось загрузиить изображение");
                    }

                    Mat mat = new Mat();
                    string outputpath = GetOutputPath(inputpath);

                    Cv2.Resize(src, mat, new Size(), scaleFactor, scaleFactor, InterpolationFlags.Lanczos4);
                    Cv2.ImWrite(outputpath, mat);

                    _logger.LogInformation("Качество успешно улучшено ");
                    return outputpath;
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"❌ Ошибка улучшения {ex.Message}");
                return null;
            }
        }
    }
}
