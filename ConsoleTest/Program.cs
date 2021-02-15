using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.IO.MemoryMappedFiles;

namespace ConsoleTest
{    
    class Program
    {
        static void Main(string[] args)
        {
            TestCF.TestProjRun(args);
            //TestCF.TestCloud(args);

            //TestETC.TestDB();

            //TestETC.TestScenGen();
            //TestETC.TestIrCurve();            
            //TestETC.TestRandom3();
            //TestETC.TestRandom2();



            //TestETC.ConsoleWriteMoving();
            //TestMMF.ConsoleMMF();
            //SerializationTest.TestSerializationJson();

            //Tools.MongoDBTest.Test();

            //Tools.TestClass.DisconnectionTest();
            //Tools.TestClass.TCPTestBuffers();

            //Test();

        }
    }
}
