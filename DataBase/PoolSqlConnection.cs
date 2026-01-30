using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramConvertorBots.DataBase
{
    public class PoolSqlConnection
    {
        private readonly Stack<SqlConnection> _available = new Stack<SqlConnection>();
        private readonly List<SqlConnection> _inUse = new List<SqlConnection>();
        private readonly int _maxPollSize = 10;
        private readonly object _lock = new object();

        public SqlConnection Connection()
        { 
            string connectionstring = Properties.Settings.Default.BadeConverBotConnectionString;
            var sqlconnection = new SqlConnection(connectionstring);
            sqlconnection.Open();
            return sqlconnection;
        }

        public SqlConnection PoolOpen()
        {
            lock (_lock)
            {
                SqlConnection conn;

                if (_available.Count > 0)
                {
                    conn = _available.Pop();

                    if (conn.State != System.Data.ConnectionState.Open)
                    {
                        conn.Dispose();
                        conn = Connection();
                    }
                }
                else if (_inUse.Count + _available.Count < _maxPollSize)
                {
                    conn = Connection();
                }
                else
                {
                    throw new Exception("Пул занят!");
                }

                _inUse.Add(conn);
                return conn;
            }
        }

        public void PoolClose(SqlConnection conn)
        {
            if (conn == null) return;

            lock (_lock)
            {
                if (_inUse.Contains(conn))
                {
                    _inUse.Remove(conn);

                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        _available.Push(conn);
                    }
                    else
                    {
                        conn.Dispose();
                    }
                }
                else
                { 
                    throw new Exception("Соединение не найдено");
                }
            }
        }
    }
}
