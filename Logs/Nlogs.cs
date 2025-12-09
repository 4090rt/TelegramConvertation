using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NLog;

namespace TelegramConvertorBots.Logs
{
    public partial class Nlogs : Form
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        public Nlogs()
        {
            InitializeComponent();
            Start();
        }

        public void Start()
        {
            logger.Info("Запуск");

            try
            {
                logger.Info("Бот успешно запущен");
            }
            catch (Exception ex)
            {
                logger.Info(ex, "Ошибка запуска");
            }
        }
    }
}
