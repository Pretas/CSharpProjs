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
    public partial class SendReceive
    {
        public static void Send<T>(Socket clientSock, T obj, SerializeType st = SerializeType.Binary)
        {
            // Serialize Type 보내기
            send(clientSock, SerializationUtil.Serialize((int)st));

            // 데이터 보내기
            byte[] dataByte = SerializationUtil.Serialize(obj, st);            
            send(clientSock, dataByte);
        }

        public static T Receive<T>(Socket clientSock)
        {
            // Serialize Type 받음
            SerializeType st = (SerializeType)SerializationUtil.Deserialize(Receive(clientSock), typeof(SerializeType));

            // 전송데이터 받음
            byte[] dataByte = Receive(clientSock);

            if (dataByte != null)
            {
                return (T)SerializationUtil.Deserialize(dataByte, typeof(T), st);
            }
            else
            {
                throw new Exception("null data was transferred.");
            }
        }

        public static void SendByBuffer<T>(Socket clientSock, int byteCntBySending, T obj, SerializeType st = SerializeType.Binary)
        {
            // Serialize Type 보내기
            send(clientSock, SerializationUtil.Serialize((int)st));

            // serialize object
            byte[] dataByte = SerializationUtil.Serialize(obj, st);

            // send memory length
            send(clientSock, SerializationUtil.Serialize((long)dataByte.Length));

            // count the number of sending
            int bunchCnt = dataByte.Length / byteCntBySending + (dataByte.Length % byteCntBySending > 1 ? 1 : 0);

            // send the number of sending
            send(clientSock, SerializationUtil.Serialize(bunchCnt));

            int now = 0;

            // loop for sending N times
            for (int i = 0; i < bunchCnt; i++)
            {
                // count the number of byte in one sending
                int byteCnt = Math.Min(byteCntBySending, dataByte.Length - now);

                // copy part of memory to send
                byte[] sub = new byte[byteCnt];
                Array.Copy(dataByte, now, sub, 0, byteCnt);
                now += byteCnt;

                // send
                send(clientSock, sub);
            }
        }
        
        public static T ReceiveByBuffer<T>(Socket clientSock)
        {
            // Serialize Type 받음
            SerializeType st = (SerializeType)SerializationUtil.Deserialize(Receive(clientSock), typeof(SerializeType));
            
            // 데이터 길이 받음
            long byteCnt = (long)SerializationUtil.Deserialize(Receive(clientSock), typeof(long));

            // 묶음 개수 받음
            int bunchCnt = (int)SerializationUtil.Deserialize(Receive(clientSock), typeof(int));
            
            List<byte> total = new List<byte>();

            for (int i = 0; i < bunchCnt; i++)
            {
                byte[] received = Receive(clientSock);
                total.AddRange(received.ToList());
            }

            return (T)SerializationUtil.Deserialize(total.ToArray(), typeof(T), st);
        }
    }

    // 데이터 형식에 관계없이 통신하는 함수들
    public partial class SendReceive
    {
        protected static void send(Socket clientSock, byte[] data)
        {
            // 객체의 바이트수 계산, null이거나 바이트가 0이면 실데이터는 전송하지 않음
            int dl = 0;
            if (data != null || data.Length == 0) dl = data.Length;

            // 객체의 바이트수 전송
            byte[] dlb = BitConverter.GetBytes(dl);
            clientSock.Send(dlb);

            // 객체의 바이트수 답변 받음
            byte[] lb1 = GetBytesFromStream(clientSock, 4);

            // 객체의 바이트수가 잘 전달되었는지 체크
            bool isRightLength = true;
            for (int i = 0; i < 4; i++)
            {
                if (dlb[i] != lb1[i]) isRightLength = false;
            }

            // 잘 전달되었는지 아닌지 전송, 잘못 전송되었다면 예외처리
            if (isRightLength == true)
            {
                clientSock.Send(Encoding.UTF8.GetBytes(@"!#%&("));
            }
            else
            {
                clientSock.Send(Encoding.UTF8.GetBytes(@"@$^*)"));
                throw new Exception(@"incorrect message length sended");
            }

            // 바이트수가 0 이상이어야 실데이터가 있으므로 전송
            if (dl > 0)
            {
                //메모리전송
                clientSock.Send(data);
            }
        }

        public static byte[] Receive(Socket clientSock)
        {
            // 객체의 바이트수 수신
            byte[] dlb = GetBytesFromStream(clientSock, 4);

            // 객체 바이트수 발신(echo)
            clientSock.Send(dlb);

            // 올바른 바이트수였는지 여부 수신
            // : "!#%&(" 이면 OK, "@$^*)"이면 에러
            byte[] respond = GetBytesFromStream(clientSock, 5);

            // 데이터 길이가 맞는지 다시 확인받음
            string respondStr = Encoding.UTF8.GetString(respond);
            if (respondStr == @"@$^*)") throw new Exception(@"incorrect message length received");

            int lth = BitConverter.ToInt32(dlb, 0);
            if (lth > 0)
            {
                byte[] dataReceived = GetBytesFromStream(clientSock, lth);
                return dataReceived;
            }
            else return null;
        }
        
        private static byte[] GetBytesFromStream(Socket sock, int lth)
        {
            byte[] dataBytes = new byte[lth];
            int countReceived = sock.Receive(dataBytes);
            if (countReceived < lth)
            {
                int lthNested = lth - countReceived;
                byte[] dataBytesNested = GetBytesFromStream(sock, lthNested);
                for (int i = 0; i < lthNested; i++) dataBytes[countReceived + i] = dataBytesNested[i];
            }

            return dataBytes;
        }
    }

    public class ServerWithMultiClients
    {
        public Socket ServerSocket { get; private set; }
        public List<Socket> ClientSockets { get; private set; }
        int Port { get; set; }
        byte[] Buffer { get; set; }

        public void SetupServer(int clientsCnt)
        {
            ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPHostEntry ipEntry = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress[] addr = ipEntry.AddressList;

            IPEndPoint ep = new IPEndPoint(addr[1], 100);
            ServerSocket.Bind(ep);
            ServerSocket.Listen(100);

            ClientSockets = new List<Socket>();

            for (int i = 0; i < clientsCnt; i++)
            {
                var cSocket = ServerSocket.Accept();
                ClientSockets.Add(cSocket);
                Console.WriteLine("client no. " + i + " was accepted.");
            }
        }

        public void CloseAllSockets()
        {
            foreach (var sock in ClientSockets)
            {
                sock.Shutdown(SocketShutdown.Both);
                sock.Close();
            }
        }
    }
    
    public class SocketNetworkingTools
    {
        public static void SendOnFileStreamOld(Socket toSocket, string fileName)
        {
            FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read);

            int fileLength = (int)stream.Length;
            byte[] buffer = BitConverter.GetBytes(fileLength);
            toSocket.Send(buffer);

            byte[] resp = new byte[1024];
            toSocket.Receive(resp);

            BinaryReader br = new BinaryReader(stream);
            byte[] data = br.ReadBytes(fileLength);
            toSocket.Send(data);
        }

        public static void ReceiveOnFileStreamOld(Socket fromSocket, string fileName)
        {
            byte[] fileLength = new byte[1024];
            fromSocket.Receive(fileLength);
            int dataLength = BitConverter.ToInt32(fileLength, 0);

            byte[] resp = BitConverter.GetBytes(true);
            fromSocket.Send(resp);

            FileStream stream = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            BinaryWriter w = new BinaryWriter(stream);
            byte[] data = new byte[dataLength];
            fromSocket.Receive(data);
            w.Write(data, 0, dataLength);
        }

        public static void SendOnFileStream(Socket toSocket, string fileName)
        {
            FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read);

            int fileLength = (int)stream.Length;
            byte[] buffer = BitConverter.GetBytes(fileLength);
            toSocket.Send(buffer);

            byte[] resp = new byte[1024];
            toSocket.Receive(resp);

            BinaryReader br = new BinaryReader(stream);
            byte[] data = br.ReadBytes(fileLength);
            toSocket.Send(data);
        }

        public static void ReceiveOnFileStream(Socket fromSocket, string fileName)
        {
            byte[] fileLength = new byte[1024];
            fromSocket.Receive(fileLength);
            int dataLength = BitConverter.ToInt32(fileLength, 0);

            byte[] resp = BitConverter.GetBytes(true);
            fromSocket.Send(resp);

            FileStream stream = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            BinaryWriter w = new BinaryWriter(stream);
            byte[] data = new byte[dataLength];
            fromSocket.Receive(data);
            w.Write(data, 0, dataLength);
        }
    }

    public class ServerAsync
    {
        public Socket ServerSocket { get; set; }
        List<Socket> ClientSockets { get; set; }
        int Port { get; set; }
        byte[] Buffer { get; set; }

        public void SetupServer()
        {
            ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint ep = new IPEndPoint(IPAddress.Any, 100);
            ServerSocket.Bind(ep);
            ServerSocket.Listen(100);
            ServerSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
        }

        public void CloseAllSockets()
        {
            foreach (var sock in ClientSockets)
            {
                sock.Shutdown(SocketShutdown.Both);
                sock.Close();
            }
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            Socket sock = ServerSocket.EndAccept(ar);
            ClientSockets.Add(sock);
            sock.BeginReceive(Buffer, 0, 100, SocketFlags.None, new AsyncCallback(ReceiveCallback), null);
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            Socket sock = (Socket)ar.AsyncState;
            int recv = sock.EndReceive(ar);
            sock.BeginReceive(Buffer, 0, 100, SocketFlags.None, new AsyncCallback(ReceiveCallback), null);
        }
    }

    public class ServerTCP
    {
        public Socket ServerSocket { get; private set; }
        public int Port { get; private set; }

        public void SetupServer(int port)
        {
            this.Port = port;

            ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPHostEntry ipEntry = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress[] addr = ipEntry.AddressList;

            IPEndPoint ep = new IPEndPoint(addr[1], port);
            ServerSocket.Bind(ep);
            ServerSocket.Listen(100);

            Console.WriteLine("Server {0}:{1} is listening", addr[1].ToString(), port);
        }

        public void Close()
        {
            ServerSocket.Shutdown(SocketShutdown.Both);
            ServerSocket.Close();
        }

        public static Socket GetClientSocket(Socket serverSocket)
        {
            return serverSocket.Accept();
        }
    }

    public class ClientTCP
    {
        public IPAddress ServerIP { get; private set; }
        public int Port { get; private set; }
        public Socket ClientSocket { get; private set; }

        public ClientTCP(string ip, int port)
        {
            ServerIP = IPAddress.Parse(ip);
            Port = port;
        }

        public void ConnectToServer()
        {
            ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            int tryCount = 20;
            int tryCounter = 0;

            while (tryCounter < tryCount)
            {
                try
                {
                    IPEndPoint ep = new IPEndPoint(ServerIP, Port);
                    ClientSocket.Connect(ep);
                    return;
                }
                catch (SocketException e)
                {
                    tryCounter++;
                    Thread.Sleep(10000);

                    if (tryCounter == tryCount) throw new Exception(string.Format(@"Failed to connect to {0}:{1}, {2}", ServerIP.ToString(), Port, e));
                }
            }            
        }

        public void Close()
        {
            ClientSocket.Shutdown(SocketShutdown.Both);
            ClientSocket.Close();
        }
    }
    
    //public class ServerSocket
    //{
    //    public Socket sock;
    //    public Socket clientSock;

    //    public ServerSocket(int port)
    //    {
    //        sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

    //        // (2) 포트에 바인드
    //        IPEndPoint ep = new IPEndPoint(IPAddress.Any, port);
    //        sock.Bind(ep);

    //        // (3) 포트 Listening 시작
    //        sock.Listen(10);

    //        // (4) 연결을 받아들여 새 소켓 생성 (하나의 연결만 받아들임)
    //        clientSock = sock.Accept();
    //    }

    //    public void Close()
    //    {
    //        // (7) 소켓 닫기
    //        clientSock.Close();
    //        sock.Close();
    //    }
    //}

    //public class ClientSocket
    //{
    //    public Socket sock;

    //    public ClientSocket(string serverIP, int port)
    //    {
    //        // (1) 소켓 객체 생성 (TCP 소켓)
    //        sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

    //        int tryCount = 20;
    //        int tryCounter = 0;
    //        while (tryCounter < tryCount)
    //        {
    //            try
    //            {
    //                // (2) 서버에 연결
    //                IPEndPoint ep = new IPEndPoint(IPAddress.Parse(serverIP), port);
    //                sock.Connect(ep);
    //            }
    //            catch (SocketException e)
    //            {
    //                Console.WriteLine(@"Client for {0}/{1} : {2}", serverIP, port.ToString(), e.Message);
    //                Thread.Sleep(10000);
    //            }
    //            finally
    //            {
    //                tryCounter++;
    //            }
    //        }
    //    }

    //    public void Close()
    //    {
    //        // (5) 소켓 닫기
    //        sock.Close();
    //    }
    //}

}