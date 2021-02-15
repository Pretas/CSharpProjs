using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;

namespace Tools
{
    public class SQLiteUtil
    {
        public void Init()
        {
            string baseConnectionString = @"Data Source = C:\SQLITEDATABASES\SQLITEDB1.sqlite; Version=3;";
            
            var conn = new SQLiteConnection(baseConnectionString);
            conn.Open();


        }
    }
}
