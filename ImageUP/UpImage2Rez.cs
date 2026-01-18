using Microsoft.Extensions.Logging;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace TelegramConvertorBots.ImageUP
{
    public class UpImage2Rez
    {
        private readonly ITelegramBotClient _botclient;
        private readonly Microsoft.Extensions.Logging.ILogger _logger;

        public UpImage2Rez(ITelegramBotClient botclient, Microsoft.Extensions.Logging.ILogger logger)
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

        public async Task<string> ImagesUPMin(string inputpath, double strength)
        {
            try
            {
                _logger.LogInformation("Запущен 3 этап улучшения");
                using (Mat src = Cv2.ImRead(inputpath))
                {
                    if (src.Empty())
                    {
                        _logger.LogError("Файл не найден или не загружен");
                        return null;
                    }

                    string outputh = GetOutputPath(inputpath);
                    Mat mat = new Mat();

                    ApplySharpening(src, mat, strength);
                    Cv2.ImWrite(outputh, mat);

                    _logger.LogInformation("Качество успешно улучшено!");
                    return outputh;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Ошибка при улучшении 3 этап");
                return null;
            }
        }


        public async Task<string> ImagesUPMiddle(string inputpath, double strength)
        {
            try
            {
                _logger.LogInformation("Запущен 3 этап улучшения");
                using (Mat src = Cv2.ImRead(inputpath))
                {
                    if (src.Empty())
                    {
                        _logger.LogError("Файл не найден или не загружен");
                        return null;
                    }
                    Mat mat = new Mat();
                    string outpath = GetOutputPath(inputpath);

                    ApplySharpening(src, mat, strength);
                    Cv2.ImWrite(outpath, mat);

                    _logger.LogInformation("Качество успешно улучшено!");
                    return outpath;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Ошибка при улучшении 3 этап");
                return null;
            }
        }

        public async Task<string> ImagesUPHigh(string inputpath, double strength)
        {
            try
            {
                _logger.LogInformation("Запущен 3 этап улучшения");
                using (Mat src = Cv2.ImRead(inputpath))
                {
                    if (src.Empty())
                    {
                        _logger.LogError("Файл не найден или не загружен");
                        return null;
                    }
                    Mat mat = new Mat();
                    string outpath = GetOutputPath(inputpath);

                    ApplySharpening(src, mat, strength);
                    Cv2.ImWrite(outpath, mat);

                    _logger.LogInformation("Качество успешно улучшено!");
                    return outpath;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Ошибка при улучшении 3 этап");
                return null;
            }
        }

        private void ApplySharpening(Mat src, Mat dst, double strength)
        {
            Mat blurred = new Mat();
            // Размываем изображение
            Cv2.GaussianBlur(src, blurred, new Size(0, 0), 3);

            // Применяем формулу: sharpened = original + strength * (original - blurred)
            Cv2.AddWeighted(
               src1:src,           // исходное
               alpha: 1.0 + strength,  // усиливаем оригинал
               src2: blurred,      // размытое
               beta: -strength,    // вычитаем размытое
               gamma: 0,           // смещение яркости
               dst: dst            // результат
           );
        }
    }
}
