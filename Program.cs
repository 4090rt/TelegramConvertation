using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramConvertorBots.Logs;

namespace TelegramConvertorBots
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hi");
            Nlogs logs = new Nlogs();
            logs.Start();
        }
    }
}
