using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tools
{
    public class MMFManager
    {
        public string FileName { get; private set; }
        public MemoryMappedFile Memory { get; private set; }
        public SerializeType SType { get; set; }
        public Type DataType { get; private set; }

        public MMFManager(string fileName, Type dt, object obj, SerializeType st)
        {
            FileName = fileName;
            DataType = dt;

            SaveMMF(obj, st);
        }

        public MMFManager(string fileName, Type dt, Socket sock)
        {
            FileName = fileName;
            DataType = dt;

            SaveMMF(sock);
        }

        public void SaveMMF(object obj, SerializeType st)
        {
            SType = st;
            
            try
            {
                byte[] byteObj = SerializationUtil.Serialize(obj, SType);

                Memory = MemoryMappedFile.CreateNew(FileName, byteObj.Length);

                int lth = byteObj.Length;

                using (var wt = Memory.CreateViewAccessor(0, lth))
                {
                    int pos = 0;
                    wt.WriteArray(pos, byteObj, 0, lth);
                }
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("mmf {0} exists already.", FileName));
            }
        }
        
        public void SaveMMF(Socket sock)
        {
            // Serialize Type 받음
            SType = (SerializeType)SerializationUtil.Deserialize(SendReceive.Receive(sock), typeof(SerializeType));

            // 데이터 길이 받음
            long byteCnt = (long)SerializationUtil.Deserialize(SendReceive.Receive(sock), typeof(long));

            // 묶음 개수 받음
            int bunchCnt = (int)SerializationUtil.Deserialize(SendReceive.Receive(sock), typeof(int));

            // using 쓰면 안됨, isClosed = true로 바뀜
            Memory = MemoryMappedFile.CreateNew(FileName, byteCnt);

            long position = 0;

            for (int i = 0; i < bunchCnt; i++)
            {
                byte[] received = SendReceive.Receive(sock);

                using (var wt = Memory.CreateViewAccessor(position, received.Length))
                {
                    int pos = 0;
                    wt.WriteArray(pos, received, 0, received.Length);
                    position += received.Length;
                }
            }
        }
        
        public object LoadMMF()
        {
            if (FileName == null) throw new Exception("Loading unavailable MMF is failed.");

            object res = null;

            Thread t = new Thread(() =>
            {
                try
                {
                    using (var mmf = MemoryMappedFile.OpenExisting(FileName))
                    {
                        using (var stream = mmf.CreateViewStream())
                        {
                            using (BinaryReader br = new BinaryReader(stream))
                            {
                                byte[] rt = br.ReadBytes((int)stream.Length);
                                res = SerializationUtil.Deserialize(rt, DataType, SType);
                            }
                        }
                    }
                }
                catch
                {
                    throw new Exception(string.Format("mmf {0} does not exists.", FileName));
                }
            });

            t.Start();

            if (!t.Join(TimeSpan.FromSeconds(30)))
            {
                t.Abort();
                throw new Exception(string.Format("Deadlock occured on mmf {0}", FileName));
            }

            return res;
        }

        public static bool Exists(string fileName)
        {
            try
            {
                using (var mmf = MemoryMappedFile.OpenExisting(fileName)) { }
                return true;
            }
            catch
            {
                return false;
            }            
        }        
    }

    public class MemoryMappedFileUtil
    {
        public static MemoryMappedFile SaveMMF<T>(T obj, string fileName, bool isReuse)
        {
            MemoryMappedFile mmf;

            bool isExists = false;

            try
            {
                using (var item = MemoryMappedFile.OpenExisting(fileName)) { }

                isExists = true;

                if (isReuse) mmf = MemoryMappedFile.OpenExisting(fileName);
                else throw new Exception("There is file " + fileName + "already");

                return mmf;
            }
            catch(Exception e)
            {
                if (!isReuse && isExists) throw e;
                
                try
                {
                    byte[] byteObj = SerializationUtil.SerializeBinary(obj);

                    mmf = MemoryMappedFile.CreateNew(fileName, byteObj.Length);

                    int lth = byteObj.Length;

                    using (var wt = mmf.CreateViewAccessor(0, lth))
                    {
                        int pos = 0;
                        wt.WriteArray(pos, byteObj, 0, lth);
                    }

                    return mmf;
                }
                catch (Exception e2)
                {
                    throw e2;
                }
            }
        }

        public static MemoryMappedFile SaveMMF(string name, Socket sock)
        {
            long byteCnt = SendReceive.Receive<long>(sock);
            int bunchCnt = SendReceive.Receive<int>(sock);

            // using 쓰면 안됨, isClosed = true로 바뀜
            var mmf = MemoryMappedFile.CreateNew(name, byteCnt);

            long position = 0;

            for (int i = 0; i < bunchCnt; i++)
            {
                byte[] received = SendReceive.Receive(sock);

                using (var wt = mmf.CreateViewAccessor(position, received.Length))
                {
                    int pos = 0;
                    wt.WriteArray(pos, received, 0, received.Length);
                    position += received.Length;
                }
            }

            return mmf;            
        }

        public static T LoadMMF<T>(string fileName)
        {
            T res = default(T);

            Thread t = new Thread(() =>
            {
                try
                {
                    using (var mmf = MemoryMappedFile.OpenExisting(fileName))
                    {
                        using (var stream = mmf.CreateViewStream())
                        {
                            using (BinaryReader br = new BinaryReader(stream))
                            {
                                byte[] rt = br.ReadBytes((int)stream.Length);
                                res = (T)SerializationUtil.DeserializeBinary(rt);
                            }
                        }
                    }
                }
                catch
                {
                    throw new Exception(string.Format("mmf {0} does not exists.", fileName));
                }
            });

            t.Start();

            if (!t.Join(TimeSpan.FromSeconds(30)))
            {
                t.Abort();
                throw new Exception(string.Format("Deadlock occured on mmf {0}", fileName));
            }

            return res;
        }
    }
}
