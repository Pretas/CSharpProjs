using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools
{
    public class LibFile
    {
        public static void WriteFile(string path, List<double> lst)
        {
            using (var file = File.CreateText(path))
            {
                foreach (var arr in lst)
                {
                    file.WriteLine(arr);
                }
            }
        }

        public static void WriteFile(string path, List<object[]> lst, char delemeter)
        {
            using (var file = File.CreateText(path))
            {
                foreach (var arr in lst)
                {
                    for (int i = 0; i < arr.Length; i++)
                    {
                        if (i == arr.Length - 1)
                        {
                            file.WriteLine(arr[i]);
                        }
                        else
                        {
                            file.Write(arr[i].ToString());
                            file.Write(delemeter);
                        }                        
                    }
                }
            }
        }

        public static void WriteTextFileWithHeader(string filePath, DataTable dt, bool isAppend)
        {
            using (StreamWriter sw = new StreamWriter(filePath, isAppend, Encoding.GetEncoding("utf-8")))
            {
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    sw.Write(dt.Columns[i].ColumnName);
                    if (i == dt.Columns.Count - 1) sw.WriteLine();
                    else sw.Write(",");
                }

                foreach (DataRow row in dt.Rows)
                {
                    object[] array = row.ItemArray;
                    for (int i = 0; i < array.Length; i++)
                    {
                        string str;
                        if (array[i].GetType().ToString() == @"System.DataTime") str = ((DateTime)array[i]).ToString("yyyy-MM-dd hh:mm:ss");
                        else str = array[i].ToString();

                        sw.Write(str);

                        if (i == array.Length - 1) sw.WriteLine();
                        else sw.Write(@",");
                    }
                }
            }
        }

        public static List<string[]> ReadCSV(string filePath)
        {
            List<string[]> res = new List<string[]>();

            using (StreamReader sr = new StreamReader(filePath, Encoding.UTF8))
            {
                while (!sr.EndOfStream)
                {
                    res.Add(sr.ReadLine().Split(','));
                }
            }

            return res;
        }
    }
}
