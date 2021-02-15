using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools;

namespace ConsoleTest
{
    public class TestMMF
    {
        public static void ConsoleMMF()
        {
            // Thread1 : MMF 저장
            var dd = GetRandomData();
            MMFManager m1 = new MMFManager("TestMMF", typeof(Dictionary<int, double[]>), dd, SerializeType.Binary);


            // Thread2 : MMF 로딩
            var ret = m1.LoadMMF() as Dictionary<int, double[]>;

            Console.ReadKey();
        }

        private static Dictionary<int, double[]> GetRandomData()
        {
            var res = new Dictionary<int, double[]>();
            double[] d;

            Random rd = new Random();

            for (int i = 0; i < 100; i++)
            {
                d = new double[10000];
                for (int j = 0; j < 10000; j++)
                {
                    d[j] = rd.NextDouble() - 0.5;
                }

                res.Add(i, d);
            }

            return res;
        }

    }
}
