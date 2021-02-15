using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools
{
    public class MongoDBTest
    {
        public static void Test()
        {
            MongoCRUD mDB = new MongoCRUD("KKHDB");
            mDB.InsertRecord<Person>("users", new Person() { FirstName = "Kyohyeon", LastName = "Kim" });

            Console.ReadLine();
        }
    }

    public class Person
    {
        [BsonId]
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class MongoCRUD
    {
        IMongoDatabase DB;

        public MongoCRUD(string dbName)
        {
            var client = new MongoClient();
            DB = client.GetDatabase(dbName);
        }

        public void InsertRecord<T>(string tableName, T record)
        {
            var collection = DB.GetCollection<T>(tableName);
            collection.InsertOne(record);
        }
    }
}
