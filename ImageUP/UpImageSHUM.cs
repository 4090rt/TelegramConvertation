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
    public class UpImageSHUM
    {
        private readonly ITelegramBotClient _botclient;
        private readonly Microsoft.Extensions.Logging.ILogger _logger;

        public UpImageSHUM(ITelegramBotClient botclient, Microsoft.Extensions.Logging.ILogger logger)
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


        public async Task<string> ImagesUPMin(string inputpath, int level)
        {
            try
            {
                _logger.LogInformation("Начат 4 этап улучшения");
                using (Mat src = Cv2.ImRead(inputpath))
                {
                    if (src.Empty())
                    {
                        _logger.LogError("Файл не найден или не добавлен");
                    }

                    string outpath = GetOutputPath(inputpath);
                    Mat mat = new Mat();

                    Cv2.FastNlMeansDenoisingColored
                        (
                            src: src,                   // Входное изображение
                            dst: mat,                   // Выходное изображение
                            h: (float)level,         // Параметр фильтрации (основной)
                            hColor: (float)level,    // Параметр для цветных компонент
                            templateWindowSize: 7,      // Размер шаблона (7, 21)
                            searchWindowSize: 21        // Размер окна поиска (21, 35)
                        );
                    Cv2.ImWrite(outpath, mat);

                    _logger.LogInformation("Качество успешно улучшено!");
                    return outpath;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Ошибка при улучшении 4 этап");
                return null;
            }
        }

        public async Task<string> ImagesUPMiddle(string inputpath, int level)
        {
            try
            {
                _logger.LogInformation("Начат 4 этап улучшения");
                using (Mat src = Cv2.ImRead(inputpath))
                {
                    if (src.Empty())
                    {
                        _logger.LogError("Файл не найден или не добавлен");
                    }

                    string outpath = GetOutputPath(inputpath);
                    Mat mat = new Mat();

                    Cv2.FastNlMeansDenoisingColored
                        (
                            src: src,                   // Входное изображение
                            dst: mat,                   // Выходное изображение
                            h: (float)level,         // Параметр фильтрации (основной)
                            hColor: (float)level,    // Параметр для цветных компонент
                            templateWindowSize: 7,      // Размер шаблона (7, 21)
                            searchWindowSize: 21        // Размер окна поиска (21, 35)
                        );
                    Cv2.ImWrite(outpath, mat);

                    _logger.LogInformation("Качество успешно улучшено!");
                    return outpath;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Ошибка при улучшении 4 этап");
                return null;
            }
        }

        public async Task<string> ImagesUPHigh(string inputpath, int level)
        {
            try
            {
                _logger.LogInformation("Начат 4 этап улучшения");
                using (Mat src = Cv2.ImRead(inputpath))
                {
                    if (src.Empty())
                    {
                        _logger.LogError("Файл не найден или не добавлен");
                    }

                    string outpath = GetOutputPath(inputpath);
                    Mat mat = new Mat();

                    Cv2.FastNlMeansDenoisingColored
                        (
                            src: src,                   // Входное изображение
                            dst: mat,                   // Выходное изображение
                            h: (float)level,         // Параметр фильтрации (основной)
                            hColor: (float)level,    // Параметр для цветных компонент
                            templateWindowSize: 7,      // Размер шаблона (7, 21)
                            searchWindowSize: 21        // Размер окна поиска (21, 35)
                        );
                    Cv2.ImWrite(outpath, mat);

                    _logger.LogInformation("Качество успешно улучшено!");
                    return outpath;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Ошибка при улучшении 4 этап");
                return null;
            }
        }
    }
}
