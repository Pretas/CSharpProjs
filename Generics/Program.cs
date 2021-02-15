using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Generics;
using System.Reflection;

namespace Generics
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] keys = new string[2];
            keys[0] = "000001";
            keys[1] = "000002";

            var infBOP = new Dictionary<string, InforceT>();
            infBOP = Functions.GetTable<string, InforceT>(keys);
        }
    }

    class Functions
    {
        public static Dictionary<T, U> GetTable<T, U>(T[] keys)
        {
            var table = new Dictionary<T, U>();

            var ins1 = (U)Activator.CreateInstance(typeof(U));
            table.Add(keys[0], ins1);

            var ins2 = (U)Activator.CreateInstance(typeof(U));
            MethodInfo methodInfo = typeof(U).GetMethod("Cleansing");
            methodInfo.Invoke(ins2, null);
            table.Add(keys[1], ins2);

            return table;
        }        
    }

    class InforceT
    {
        public InforceT()
        {
            contNo = "000001";
            seg = "VA";
            gurType = 12;
            av = 0;
        }   

        public string contNo;
        public string seg;
        public int gurType;
        public double av;

        public void Cleansing()
        {
            contNo = "000002";
            seg = "VUL";
            gurType = 1;
            av = 100;
        }
    }
}
