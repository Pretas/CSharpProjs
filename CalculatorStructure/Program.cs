using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalculatorStructure
{
    public class A
    {
        public AIn In = new AIn();
        public AOut Out = new AOut();
    }

    public class AIn
    {
        public AIn() { }
        public int V1;
        public int V2;
    }

    public class AOut : ICloneable
    {
        public int V1;
        public int V2;
        public bool V3;
        public Dictionary<int, AOut> Vs = new Dictionary<int, AOut>();        

        public object Clone()
        {
            AOut clone = new AOut
            {
                V1 = V1,
                V2 = V2,
                V3 = V3
            };
            return clone;
        }

        public void StackResult(int period)
        {
            AOut clone = (AOut)Clone();
            Vs.Add(period, clone);
        }
    }

    public class B
    {
        public BIn In = new BIn();
        public BOut Out= new BOut();
    }

    public class BIn
    {
        public BIn() { }
        public int V1;
        public int V2;
    }

    public class BOut
    {
        public Dictionary<int, BOut> Vs = new Dictionary<int, BOut>();
        public void StackByPeriod(int period)
        {
            Vs.Add(period, this);
        }

        public int V1;
        public int V2;
    }

    public class C
    {
        public CIn In = new CIn();
        public COut Out = new COut();
    }

    public class CIn
    {
        public CIn() { }
        public int V1;
        public int V2;
    }

    public class COut
    {
        public Dictionary<int, COut> Vs = new Dictionary<int, COut>();
        public void StackByPeriod(int period)
        {
            Vs.Add(period, this);
        }

        public int V1;
        public int V2;
    }
    
    public class Methods
    {
        public static void SetSomething1(A a, C c)
        {
            a.Out.V1 = a.In.V1 + c.In.V1;
            a.Out.V2 = a.In.V2 + c.In.V2;
        }

        public static void SetSomething2(B b, C c)
        {
            b.Out.V1 = b.In.V1 + c.In.V1;
            b.Out.V2 = b.In.V2 + c.In.V2;
        }

        public static void SetSomething3(A a, B b, C c)
        {
            c.Out.V1 = b.In.V1 + a.In.V1;
            c.Out.V2 = b.In.V2 + a.In.V2;
        }
    }

    public class Product001
    {
        public A MemberA = new A();
        public B MemberB = new B();
        public C MemberC = new C();

        public void Execute()
        {
            while (MemberA.Out.V3 == true)
            {
                Methods.SetSomething1(MemberA, MemberC);
                Methods.SetSomething2(MemberB, MemberC);
                Methods.SetSomething3(MemberA, MemberB, MemberC);
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
        }        
    }
}
