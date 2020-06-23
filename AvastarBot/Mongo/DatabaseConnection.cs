using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
namespace AvastarBot.Mongo
{
    class DatabaseConnection
    {
        private static MongoClient Client;
        private static IMongoDatabase Database;
        public static string DatabaseName;

        private static void SetupConnection()
        {
            var connectionString = DiscordKeyGetter.GetDBUrl();

            Client = new MongoClient(connectionString);
            Database = Client.GetDatabase(DatabaseName);
        }

        public static IMongoDatabase GetDb()
        {
            if (Client == null)
            {
                SetupConnection();
            }
            return Database;
        }

    }
}