using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleTest
{
    public class TestClass
    {
        public int M1;
        public string M2;
        public double M3;
    }

    public class SerializationTest
    {
        public static Dictionary<int, TestClass> GetObject3()
        {
            Dictionary<int, TestClass> dic = new Dictionary<int, TestClass>();

            Random rd = new Random();
            TestClass obj;
            for (int i = 0; i < 100000; i++)
            {
                obj = new TestClass();
                obj.M1 = rd.Next();
                obj.M2 = obj.M1.ToString();
                obj.M3 = rd.NextDouble();
                dic.Add(i, obj);
            }

            return dic;
        }


        public static Dictionary<int, double[,]> GetObject2()
        {
            Dictionary<int, double[,]> dic = new Dictionary<int, double[,]>();

            Random rd = new Random();
            double[,] cont;
            for (int i = 0; i < 100000; i++)
            {
                int lth = rd.Next(1, 10);
                int lth2 = rd.Next(1, 10);
                cont = new double[lth, lth2];
                for (int j = 0; j < lth; j++)
                {
                    for (int k = 0; k < lth2; k++)
                    {
                        cont[j, k] = rd.NextDouble();
                    }
                }

                dic.Add(i, cont);
            }

            return dic;
        }


        public static Dictionary<int, double[]> GetObject()
        {
            Dictionary<int, double[]> dic = new Dictionary<int, double[]>();

            Random rd = new Random();
            double[] cont;
            for (int i = 0; i < 100000; i++)
            {
                int lth = rd.Next(1, 10);
                cont = new double[lth];
                for (int j = 0; j < lth; j++)
                {
                    cont[j] = rd.NextDouble();
                }

                dic.Add(i, cont);
            }

            return dic;            
        }

        public static void TestSerializationJson()
        {
            Dictionary<int, TestClass> dic = GetObject3();

            byte[] data = Tools.SerializationUtil.SerializeJson(dic);

            object obj2 = Tools.SerializationUtil.DeserializeJson(data, typeof(Dictionary<int, TestClass>));
            
            //Dictionary<int, double[,]> dic2 = (Dictionary<int, double[,]>)obj2;

            //Dictionary<int, double[,]> dic = GetObject2();
            //byte[] data = Tools.SerializationUtil.SerializeJson(dic);

            //object obj2 = Tools.SerializationUtil.DeserializeJson(data, typeof(Dictionary<int, double[,]>));
            //Dictionary<int, double[,]> dic2 = (Dictionary<int, double[,]>)obj2;



        }

    }
}
