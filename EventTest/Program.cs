using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventTest
{
    delegate void EventHandlerKKH(string message);

    class MyNotifier
    {
        public event EventHandlerKKH SomethingHappened;

        public void DoSomething(int number)
        {
            int temp = number % 10;

            if (temp != 0 && temp % 3 == 0)
            {
                SomethingHappened(String.Format("{0} : 짝짝", number));
            }
        }
    }

    class Program
    {
        static public void MyHandler(string message)
        {
            Console.WriteLine(message);
        }

        static void Main(string[] args)
        {
            var mn = new MyNotifier();
            mn.SomethingHappened += new EventHandlerKKH(MyHandler);

            for (int i = 1; i < 30; i++)
            {
                mn.DoSomething(i);
            }
        }
    }
}
