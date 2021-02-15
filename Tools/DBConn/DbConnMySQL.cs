using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools
{
    public class MySqlDBConn : IDbConnection
    {
        public string ConnStr { get; private set; }
        public MySqlConnection Conn { get; private set; }

        public MySqlDBConn(DBConnParams dp)
        {
            ConnStr = string.Format("server={0};uid={1};pwd={2};database={3};charset=utf8;", dp.ServerName, dp.ServerID, dp.ServerPW, dp.DbName);
        }

        public void Execute(string query)
        {
            Conn = new MySqlConnection(ConnStr);
            Conn.Open();

            MySqlCommand command = new MySqlCommand(query, Conn);
            command.ExecuteNonQuery();

            Conn.Close();
        }

        public List<object[]> GetResult(string query)
        {
            Conn = new MySqlConnection(ConnStr);
            Conn.Open();

            MySqlCommand command = new MySqlCommand(query, Conn);
            MySqlDataReader rdr = command.ExecuteReader();

            if (rdr == null) return null;

            List<object[]> dt = new List<object[]>();

            while (rdr.Read())
            {
                object[] line = new object[rdr.FieldCount];

                for (int i = 0; i < rdr.FieldCount; i++)
                {
                    line[i] = rdr[i];
                }

                dt.Add(line);
            }

            Conn.Close();

            return dt;
        }
        
        public IDataReader GetDataReader(string query)
        {
            Conn = new MySqlConnection(ConnStr);
            Conn.Open();

            MySqlCommand command = new MySqlCommand(query, Conn);
            return command.ExecuteReader();
        }

        public void Insert(List<object[]> data, string tableName)
        {
            Conn = new MySqlConnection(ConnStr);
            Conn.Open();

            string query = string.Format(@"select * from {0} limit 0", tableName);
            MySqlCommand command = new MySqlCommand(query, Conn);
            IDataReader rd = command.ExecuteReader();
            DataTable dt = LibsData.GetDataTable(rd, data);
            Conn.Close();

            string filePath = GetAvailableFilePath();
            System.IO.File.Delete(filePath);
            LibFile.WriteTextFileWithHeader(filePath, dt, false);
            InsertFromTextFile(filePath, tableName);
            System.IO.File.Delete(filePath);
        }

        public void InsertFromTextFile(string filePath, string tableName)
        {
            Conn = new MySqlConnection(ConnStr);
            Conn.Open();

            var loader = new MySqlBulkLoader(Conn);
            loader.TableName = tableName;
            loader.FileName = filePath;
            loader.FieldTerminator = ",";
            loader.LineTerminator = "\r\n";
            loader.NumberOfLinesToSkip = 1;
            loader.Load();

            Conn.Close();
        }

        public string GetAvailableFilePath()
        {
            return string.Format(@"{0}\{1}.txt", @"C:\ProgramData\MySQL\MySQL Server 8.0\Uploads\", Guid.NewGuid().ToString());
        }

    }
}
