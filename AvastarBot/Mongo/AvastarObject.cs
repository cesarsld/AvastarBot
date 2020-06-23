using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using MongoDB.Bson;
using Newtonsoft.Json.Linq;
namespace AvastarBot.Mongo
{
    public class AvastarObject
    {
        public int id;
        public string Gender;
        public int Score;
        public Dictionary<string, string> traits;
        public AvastarObject()
        {
        }

        public static async Task CreateAva(int id)
        {
            var ava = new AvastarObject();
            ava.id = id;
            string metadatastr = "";
            using (System.Net.WebClient wc = new System.Net.WebClient())
            {
                try
                {
                    metadatastr = await wc.DownloadStringTaskAsync("https://avastars.io/metadata/" + id.ToString());
                }
                catch (Exception e)
                {
                    Logger.LogInternal(e.Message);
                }
            }
            if (metadatastr.StartsWith("Invalid"))
                return ;
            var metadataJson = JObject.Parse(metadatastr);
            ava.id = id;
            ava.Gender = (string)metadataJson["attributes"][0]["value"];
            ava.Score = (int)metadataJson["attributes"][5]["value"];
            for (int i = 7; i < 19; i++)
            {
                ava.traits.Add((string)metadataJson["attributes"][i]["trait_type"], (string)metadataJson["attributes"][i]["value"]);
            }
            var collec = DatabaseConnection.GetDb().GetCollection<AvastarObject>("AvastarCollection");
            await collec.InsertOneAsync(ava);
        }
    }
}
