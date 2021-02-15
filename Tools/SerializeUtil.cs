using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Newtonsoft.Json;

namespace Tools
{
    public enum SerializeType { Binary = 0, Json = 1 }

    public static class SerializationUtil
    {
        public static byte[] Serialize(object obj, SerializeType t = SerializeType.Binary)
        {
            if (t == SerializeType.Binary) return SerializeBinary(obj);
            else return SerializeJson(obj);
        }

        public static object Deserialize(byte[] byteData, Type tp, SerializeType t = SerializeType.Binary)
        {
            if (t == SerializeType.Binary) return DeserializeBinary(byteData);
            else return SerializeJson(byteData);
        }

        public static byte[] SerializeJson(object obj)
        {
            string str = JsonConvert.SerializeObject(obj);
            byte[] res = Encoding.UTF8.GetBytes(str);
            return res;
        }

        public static object DeserializeJson(byte[] byteData, Type tp)
        {
            string str = Encoding.UTF8.GetString(byteData);
            object obj = JsonConvert.DeserializeObject(str, tp);
            return obj;
        }

        public static byte[] SerializeBinary(object obj)
        {
            if (!CheckSerializable(obj))
            { return null; }
            MemoryStream stream = new MemoryStream();
            stream.Position = 0;
            BinaryFormatter bf = new BinaryFormatter();
            //bf.Binder = new AlwaysBrandNewDeserializationBinder(); 사용안하기로함
            bf.Serialize(stream, obj);
            return stream.ToArray();
        }

        public static object DeserializeBinary(byte[] byteData)
        {
            MemoryStream stream = new MemoryStream();
            stream.SetLength(byteData.Length + Int64.Parse(Math.Pow(2, 20).ToString()));
            foreach (byte b in byteData)
            {
                stream.WriteByte(b);
            }
            stream.Position = 0;
            BinaryFormatter bf = new BinaryFormatter();
            //bf.Binder = new AlwaysBrandNewDeserializationBinder(); 사용안하기로함
            return bf.Deserialize(stream);
        }

        public static bool CheckSerializable(object obj)
        {
            Type t = obj.GetType();
            var serializable = t.GetCustomAttributes(typeof(SerializableAttribute), false);
            if (!t.IsSerializable && serializable.Count() == 0)
            { return false; }
            else
            { return true; }
        }

        private class AlwaysBrandNewDeserializationBinder : System.Runtime.Serialization.SerializationBinder
        {
            public override Type BindToType(string assemblyName, string typeName)
            {
                Type typeToDeserialize = null;
                assemblyName = System.Reflection.Assembly.GetExecutingAssembly().FullName;
                typeToDeserialize = Type.GetType(string.Format(@"{0}, {1}", typeName, assemblyName));
                return typeToDeserialize;
            }
        }
    }
}
