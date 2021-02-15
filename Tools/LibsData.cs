using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Tools
{
    public class LibsData
    {      
        internal static DataTable GetDataTable(IDataReader rd, List<object[]> data)
        {
            DataTable dt = new DataTable();

            for (int i = 0; i < rd.FieldCount; i++)
            {
                var colName = rd.GetName(i);
                var colType = rd.GetFieldType(i);

                dt.Columns.Add(new DataColumn(colName, colType));

            }

            foreach (var item in data)
            {
                DataRow newRow = dt.NewRow();

                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    newRow[i] = Convert.ChangeType(item[i], dt.Columns[i].DataType);
                }

                dt.Rows.Add(newRow);
            }

            return dt;
        }

        public static List<T> GetInstance<T>(List<object[]> dt)
        {
            List<T> res = new List<T>();

            Type t = typeof(T);
            var fields = t.GetFields();

            object newInstance;

            foreach (var line in dt)
            {
                newInstance = Activator.CreateInstance(t);

                for (int i = 0; i < line.Length; i++)
                {
                    fields[i].SetValue(newInstance, line[i]);
                }

                res.Add((T)newInstance);
            }

            return res;
        }
    }
}
