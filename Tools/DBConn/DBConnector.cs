using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools
{
    public class DBConnParams
    {
        public string ServerName = "";
        public string ServerID = "";
        public string ServerPW = "";
        public string DbName = "";

        public DBConnParams()
        { }

        public DBConnParams(string[] args)
        {
            ServerName = args[0];
            ServerID = args[1];
            ServerPW = args[2];
            DbName = args[3];
        }

        public void InitMY()
        {
            ServerName = @"localhost";
            ServerID = @"kkh";
            ServerPW = @"kkh198400";
            DbName = @"proj";
        }

        public void InitMS()
        {
            ServerName = @"DESKTOP-3SKP3T7\KKH";
            ServerID = @"sa";
            ServerPW = @"3355";
            DbName = @"proj";
        }
    }

    public interface IDbConnection
    {
        void Execute(string query);
        List<object[]> GetResult(string query);
        IDataReader GetDataReader(string query);
        void Insert(List<object[]> data, string tableName);
    }

}
