using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tools
{
    public enum Role
    {
        Server, Client
    }

    public class TestJob
    {
        public Dictionary<string, Action<Socket, Role>> Actions;

        public List<string> MsgFrom = new List<string>();

        public TestJob()
        {
            Actions = new Dictionary<string, Action<Socket, Role>>();
            Actions.Add(@"Job1", DoJob1);
            Actions.Add(@"Job2", DoJob2);
        }

        public void DoHeadJob(object sockObj)
        {
            Socket sock = (Socket)sockObj;
        }

        public void DoUpperJob(object sockObj)
        {
            Socket sock = (Socket)sockObj;

            bool isFinished = false;
            Action<Socket, Role> action;

            while (!isFinished)
            {
                string jobName = Tools.SendReceive.Receive<string>(sock);

                if (jobName.ToLower() == "theend")
                {
                    Thread.Sleep(1000);
                    isFinished = true;
                }
                else
                {
                    action = Actions[jobName];
                    action(sock, Role.Server);
                }                
            }
        }

        public void DoLowerJob(object sockObj)
        {
            Socket sock = (Socket)sockObj;

            Tools.SendReceive.Send<String>(sock, "Job1");
            DoJob1(sock, Role.Client);

            Tools.SendReceive.Send<String>(sock, "Job2");
            DoJob2(sock, Role.Client);

            Tools.SendReceive.Send<String>(sock, "TheEnd");
        }

        public void DoWorkerJob(object sockObj)
        {
            Socket sock = (Socket)sockObj;
        }


        public void DoJob1(Socket sock, Role role)
        {
            if (role == Role.Server)
            {
                Tools.SendReceive.Send<string>(sock, Guid.NewGuid().ToString());
            }
            else
            {
                MsgFrom.Add(Tools.SendReceive.Receive<string>(sock));
            }
        }

        public void DoJob2(Socket sock, Role role)
        {
            if (role == Role.Server)
            {
                MsgFrom.Add(Tools.SendReceive.Receive<string>(sock));
            }
            else
            {
                Tools.SendReceive.Send<string>(sock, Guid.NewGuid().ToString());
            }
        }
    }

    class SocketUtilTest
    {
        public static void EchoServerTest()
        {
            IPHostEntry ipEntry = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress[] addr = ipEntry.AddressList;
            Console.WriteLine("enter your role(s, 1,2,3...)");
            string role = Console.ReadLine();

            int clCnt = 2;

            if (role == @"s")
            {
                TestJob tj = new TestJob();

                Task[] tasks = new Task[clCnt];
                Tools.ServerTCP sv = new ServerTCP();
                for (int i = 0; i < clCnt; i++)
                {
                    Socket sock = ServerTCP.GetClientSocket(sv.ServerSocket);
                    tasks[i] = new Task(tj.DoUpperJob, sock);
                    tasks[i].Start();
                }

                for (int i = 0; i < clCnt; i++) tasks[i].Wait();
            }
            else
            {
                TestJob tj = new TestJob();

                Tools.ClientTCP cl = new Tools.ClientTCP(addr[1].ToString(), 100);
                cl.ConnectToServer();

                tj.DoLowerJob(cl);
            }

            Console.WriteLine($"{role} finished");
            Console.ReadKey();
        }
        
        public static void MultiClientsTest()
        {
            IPHostEntry ipEntry = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress[] addr = ipEntry.AddressList;
            Console.WriteLine("enter your role(s, 1,2,3...)");
            string role = Console.ReadLine();

            int loopNo = 1000;

            if (role == @"s")
            {
                int clCnt = 3;

                Tools.ServerWithMultiClients sv = new Tools.ServerWithMultiClients();
                sv.SetupServer(clCnt);
                List<string>[] recvM = new List<string>[clCnt];

                Action<object> ac = x =>
                {
                    Socket sock = ((Tuple<Socket, int>)x).Item1;
                    int clNo = ((Tuple<Socket, int>)x).Item2;

                    recvM[clNo] = new List<string>();

                    for (int i = 0; i < loopNo; i++)
                    {
                        string recv = Tools.SendReceive.Receive<string>(sock);
                        recvM[clNo].Add(recv);
                    }
                };

                Task[] tasks = new Task[clCnt];

                for (int i = 0; i < clCnt; i++)
                {
                    tasks[i] = new Task(ac, Tuple.Create(sv.ClientSockets[i], i));
                    tasks[i].Start();
                }

                for (int i = 0; i < clCnt; i++)
                {
                    tasks[i].Wait();
                }

                Console.WriteLine("server finished");
                Console.ReadKey();
            }
            else
            {
                Tools.ClientTCP cl = new Tools.ClientTCP(addr[1].ToString(), 100);
                cl.ConnectToServer();

                for (int i = 0; i < loopNo; i++)
                {
                    string msg = string.Format(@"{0}_{1}", role, i);
                    Tools.SendReceive.Send<string>(cl.ClientSocket, msg);
                    Thread.Sleep(10 + Convert.ToInt32(role));
                }

                cl.Close();

                Console.WriteLine("client finished");
                Console.ReadKey();
            }

        }

        // 여러 클라이언트로부터 동시에 데이터를 버퍼로 받으면서 바로 하드에 저장
        // 메모리 문제를 해결하기 위함
        public static void TCPTestBuffers()
        {
            IPHostEntry ipEntry = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress[] addr = ipEntry.AddressList;
            Console.WriteLine("enter your role(s, 1,2,3...)");
            string role = Console.ReadLine();

            if (role == @"s")
            {
                int clCnt = 3;

                Tools.ServerWithMultiClients sv = new Tools.ServerWithMultiClients();
                sv.SetupServer(clCnt);
                List<string>[] recvM = new List<string>[clCnt];

                Action<object> ac = x =>
                {
                    Socket sock = ((Tuple<Socket, int>)x).Item1;
                    int clNo = ((Tuple<Socket, int>)x).Item2;

                    MemoryMappedFile mmf = MemoryMappedFileUtil.SaveMMF(clNo.ToString(), sock);
                    Console.WriteLine("received data from " + clNo);

                    recvM[clNo] = MemoryMappedFileUtil.LoadMMF<List<string>>(clNo.ToString());

                    long mem = GC.GetTotalMemory(true) / 1000000;
                    Console.WriteLine("total memory is " + mem);
                };

                Task[] tasks = new Task[clCnt];

                for (int i = 0; i < clCnt; i++)
                {
                    tasks[i] = new Task(ac, Tuple.Create(sv.ClientSockets[i], i));
                    tasks[i].Start();
                }

                for (int i = 0; i < clCnt; i++)
                {
                    tasks[i].Wait();
                }

                Console.WriteLine("server finished");
                Console.ReadKey();
            }
            else
            {
                Tools.ClientTCP cl = new Tools.ClientTCP(addr[1].ToString(), 100);
                cl.ConnectToServer();

                List<string> dt = new List<string>();

                for (int i = 0; i < 1000000; i++)
                {
                    dt.Add(string.Format(@"{0}_{1}", role, string.Format("{0:D12}", i)));
                }

                Tools.SendReceive.SendByBuffer(cl.ClientSocket, 1000000, dt);

                cl.Close();

                Console.WriteLine("client finished");
                Console.ReadKey();
            }
        }

        public static void DisconnectionTest()
        {
            IPHostEntry ipEntry = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress[] addr = ipEntry.AddressList;
            Console.WriteLine("enter your role(s, 1,2,3...)");
            string role = Console.ReadLine();

            int portNo = 100;

            if (role == @"s")
            {
                int clCnt = 2;

                Tools.ServerTCP sv = new ServerTCP();
                sv.SetupServer(portNo);

                Action<object> ac = x =>
                {
                    Socket sock = ((Tuple<Socket, int>)x).Item1;
                    int clNo = ((Tuple<Socket, int>)x).Item2;

                    Console.WriteLine("waits...");
                    Console.ReadKey();

                    SendReceive.Send<bool>(sock, true);
                };

                Task[] tasks = new Task[clCnt];

                for (int i = 0; i < clCnt; i++)
                {
                    Socket clSock = Tools.ServerTCP.GetClientSocket(sv.ServerSocket);
                    Console.WriteLine($"client no.{i} was accepted");
                    tasks[i] = new Task(ac, Tuple.Create(clSock, i));
                    tasks[i].Start();
                }

                for (int i = 0; i < clCnt; i++)
                {
                    tasks[i].Wait();
                }

                Console.WriteLine("server finished");
                Console.ReadKey();
            }
            else
            {
                Tools.ClientTCP cl = new Tools.ClientTCP(addr[1].ToString(), portNo);
                cl.ConnectToServer();

                bool yn = Tools.SendReceive.Receive<bool>(cl.ClientSocket);

                cl.Close();

                Console.WriteLine("client finished");
                Console.ReadKey();
            }

        }
    }
}
