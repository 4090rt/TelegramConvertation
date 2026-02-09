using NLog.Targets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramConvertorBots.HttpBlock
{
    public class ParsedClass
    {
        private readonly Microsoft.Extensions.Logging.ILogger<ParsedClass> _logger;

        public ParsedClass(Microsoft.Extensions.Logging.ILogger<ParsedClass> logger)
        { 
            _logger = logger;
        }

        public async Task<Model> Parse(Stream stream)
        {
            try
            {
                using (var reader = new StreamReader(stream))
                {
                    var result = await reader.ReadToEndAsync();

                    var options = new JsonSerializeOptions
                    {
                        
                    };
                }
            }
            catch (Exception ex)
            { 
            
            }
        }
    }
}
