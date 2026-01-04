using DocumentFormat.OpenXml.Drawing.Charts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
  

namespace TelegramConvertorBots
{
    public class СompressionImages
    {
        public async Task<string> Compressions(string filepath, int quality)
        {
            try
            {
                string timestamp = DateTime.Now.ToString("HHmmss");
                string outputPath = System.IO.Path.Combine(
               System.IO.Path.GetDirectoryName(filepath),
               $"{System.IO.Path.GetFileNameWithoutExtension(filepath)}{timestamp}.jpg"
                );

                FileInfo fileInfo = new FileInfo(filepath);
                long fileSizeKB = fileInfo.Length / 1024;
                if (!File.Exists(filepath))
                {
                    Console.WriteLine($"❌ Файл не найден: {filepath}");
                    return "Файл не найден";
                }

                using (var image = SixLabors.ImageSharp.Image.Load(filepath))
                {
                    var encoder = new JpegEncoder
                    {
                        Quality = quality,
                       
                    };
                    await Task.Run(() =>image.Save(outputPath,encoder));
                }
                if (!File.Exists(outputPath))
                {
                    Console.WriteLine($"❌ Не удалось создать файл: {outputPath}");
                    return null;
                }
                return outputPath;

                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка Cжатия: {ex.Message}");
                return null;
            }
        }
    }
}
