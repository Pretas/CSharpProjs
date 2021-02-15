using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;

namespace Tools
{
    public class MsSqlDbConn : IDbConnection
    {
        public string ConnStr { get; private set; }
        public SqlConnection Conn { get; private set; }
    
        public MsSqlDbConn(DBConnParams dp)
        {
            ConnStr = $@"server = {dp.ServerName}; uid = {dp.ServerID}; pwd = {dp.ServerPW}; database = {dp.DbName}; Connect Timeout=300;";

            //ConnStr = $@"Data Source = {dp.DbName}; Initial Catalog = {dp.ServerName}; User ID = {dp.ServerID}; Password = {dp.ServerPW};";
        }
        
        public void Execute(string query)
        {
            using (Conn = new SqlConnection(ConnStr))
            {
                using (SqlCommand command = new SqlCommand(query, Conn))
                {
                    Conn.Open();

                    command.ExecuteNonQuery();
                }
            }
        }

        public List<object[]> GetResult(string query)
        {
            using (Conn = new SqlConnection(ConnStr))
            {
                using (SqlCommand command = new SqlCommand(query, Conn))
                {
                    Conn.Open();

                    using (SqlDataReader dr = command.ExecuteReader())
                    {
                        List<object[]> dt = new List<object[]>();

                        while (dr.Read())
                        {
                            object[] line = new object[dr.FieldCount];

                            for (int i = 0; i < dr.FieldCount; i++)
                            {
                                line[i] = dr[i];
                            }

                            dt.Add(line);
                        }

                        return dt;
                    }
                }
            }
        }


        public IDataReader GetDataReader(string query)
        {
            Conn = new SqlConnection(ConnStr);
            Conn.Open();

            SqlCommand command = new SqlCommand(query, Conn);
            return command.ExecuteReader();
        }

        public void Insert(List<object[]> data, string tableName)
        {
            DataTable dt = null;

            using (Conn = new SqlConnection(ConnStr))
            {
                Conn.Open();
                string query = string.Format(@"select top 0 * from {0}", tableName);

                using (SqlCommand command = new SqlCommand(query, Conn))
                {
                    using (SqlDataReader dr = command.ExecuteReader())
                    {
                        dt = LibsData.GetDataTable(dr, data);
                    }
                }

                // data to dataReader
                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(Conn))
                {
                    bulkCopy.DestinationTableName = tableName;
                    bulkCopy.WriteToServer(dt);
                }

                Conn.Close();
            }          
        }       

        public void Insert(IDataReader dr, string tableName)
        {
            using (Conn = new SqlConnection(ConnStr))
            {
                Conn.Open();

                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(Conn))
                {
                    using (dr)
                    {
                        bulkCopy.DestinationTableName = tableName;
                        bulkCopy.BulkCopyTimeout = 0;
                        bulkCopy.WriteToServer(dr);                    
                    }                      
                }

                Conn.Close();
            }           
        }
    }
}
