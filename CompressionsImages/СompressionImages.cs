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
        public async Task<string> Compressions(string filepath, int quality, int comressionlevel)
        {
            try
            {
                string timestamp = DateTime.Now.ToString("HHmmss");
                string outputPathjpg = System.IO.Path.Combine(
               System.IO.Path.GetDirectoryName(filepath),
               $"{System.IO.Path.GetFileNameWithoutExtension(filepath)}{timestamp}.jpg"
                );
                string outputPathpng = System.IO.Path.Combine(
                System.IO.Path.GetDirectoryName(filepath),
                $"{System.IO.Path.GetFileNameWithoutExtension(filepath)}{timestamp}.png"
                );

                FileInfo fileInfo = new FileInfo(filepath);
                var info = Path.GetExtension(filepath);
                long fileSizeKB = fileInfo.Length / 1024;
                if (!File.Exists(filepath))
                {
                    Console.WriteLine($"❌ Файл не найден: {filepath}");
                    return "Файл не найден";
                }


                switch (info)
                {

                    case ".jpg":
                        using (var image = SixLabors.ImageSharp.Image.Load(filepath))
                        {
                            var encoder = new JpegEncoder
                            {
                                Quality = quality

                            };
                            await Task.Run(() => image.Save(outputPathjpg, encoder));
                        }
                        if (!File.Exists(outputPathjpg))
                        {
                            Console.WriteLine($"❌ Не удалось создать файл: {outputPathjpg}");
                            return null;
                        }
                        return outputPathjpg;
                        break;

                    case ".png":
                        using (var image = SixLabors.ImageSharp.Image.Load(filepath))
                        {
                            var encoder = new PngEncoder
                            {
                                CompressionLevel = (PngCompressionLevel)comressionlevel
                            };
                            await Task.Run(() => image.Save(outputPathpng, encoder));
                        }
                        if (!File.Exists(outputPathpng))
                        {
                            Console.WriteLine($"❌ Не удалось создать файл: {outputPathpng}");
                            return null;
                        }
                        return outputPathpng;
                        break;
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка Cжатия: {ex.Message}");
                return null;
            }
        }
    }
}
