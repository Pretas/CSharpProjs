using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProphetTest
{
    class Program
    {
        static void Main(string[] args)
        {
            

            ProphetLiab pl = new ProphetLiab();
            pl.DoProjectionProd00001(10);
            pl.DoProjectionProd00002(10);

            Console.ReadLine();
        }
    }

    public partial class ProphetLiab
    {
        public void DoProjectionProd00001(int t)
        {
            InitIndicator001();
            InitIndicator003();
            InitInputVariableProduct00001();

            double res = b1[t];
        }

        public void DoProjectionProd00002(int t)
        {
            InitIndicator002();
            InitIndicator004();
            InitInputVariableProduct00002();

            double res = b2[t];
        }
    }

    public partial class ProphetLiab
    {
        public ProphetLiab()
        {
            a1 = new DataObj<double>(A1);
            a2 = new DataObj<double>(A2);
            a3 = new DataObj<double>(A3);
            b1 = new DataObj<double>(B1);
            b2 = new DataObj<double>(B2);
            c1 = new DataObj<double>(C1);
            c2 = new DataObj<double>(C2);
        }

        public void A1(int t)
        {
            a1[t] = t == 0 ? 0.0 : a1[t - 1] + 0.01;
        }
        public void A2(int t)
        {
            a2[t] = t == 0 ? 0.0 : a2[t - 1] + a1[t];
        }
        public void A3(int t)
        {
            a3[t] = t == 0 ? 0.0 : a3[t - 1] + 0.02;
        }
        public void B1(int t)
        {
            b1[t] = t == 0 ? 0.0 : b1[t-1] + a2[t] * c1[t];
        }
        public void B2(int t)
        {
            b2[t] = t == 0 ? 1.0 : b2[t - 1] * a3[t] + c2[t];
        }
        public void C1(int t)
        {
            c1[t] = t == 0 ? 0.0 : c1[t - 1] + 0.01;
        }
        public void C2(int t)
        {
            c2[t] = t == 0 ? 0.0 : c2[t - 1] + 0.01;
        }

        public DataObj<double> a1, a2, a3, b1, b2, c1, c2;
    }

    public partial class ProphetLiab
    {
        public void InitIndicator001()
        {
            a1.Clear();
            a2.Clear();
        }
        public void InitIndicator002()
        {
            a3.Clear();
        }
        public void InitIndicator003()
        {
            b1.Clear();
        }
        public void InitIndicator004()
        {
            b2.Clear();
        }

        public void InitInputVariableProduct00001()
        { b1.Clear(); }

        public void InitInputVariableProduct00002()
        { b2.Clear(); }
    }

    public class DataObj<T> : Dictionary<int, T>
    {
        protected Action<int> Calc;

        public DataObj(Action<int> calcMethod) : base()
        {
            Calc = calcMethod;
        }

        public new T this[int t]
        {
            get
            {
                if (!ContainsKey(t)) Calc(t);
                return base[t];
            }
            set
            {
                base[t] = value;
            }
        }
    }
}
