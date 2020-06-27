using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using MongoDB.Bson;
using System.Linq;
using MongoDB.Driver;
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

        public static async Task<int> GetAvaCount()
        {
            var collec = DatabaseConnection.GetDb().GetCollection<AvastarObject>("AvastarCollection");
            return (int)(await collec.CountDocumentsAsync(a => true));
        }

        public static async Task<List<AvastarObject>> GetAvaList()
        {
            var collec = DatabaseConnection.GetDb().GetCollection<AvastarObject>("AvastarCollection");
            return (await collec.FindAsync(a => true)).ToList();
        }

        public static async Task<List<AvastarObject>> GetSerieList(int series)
        {
            var collec = DatabaseConnection.GetDb().GetCollection<AvastarObject>("AvastarCollection");
            return (await collec.FindAsync(a => a.id > 199 + 5000 * (series - 1) && a.id < 200 + 5000 * series)).ToList();
        }

        public static async Task<List<AvastarObject>> GetSeriesList()
        {
            var collec = DatabaseConnection.GetDb().GetCollection<AvastarObject>("AvastarCollection");
            return (await collec.FindAsync(a => a.id > 199)).ToList();
        }

        public static async Task UpdateUBs(int id)
        {
            var collec = DatabaseConnection.GetDb().GetCollection<AvastarObject>("AvastarCollection");
            var ava = (await collec.FindAsync(a => a.id == id)).FirstOrDefault();
            await UB2Object.UpdateUb2List(ava);
            await UB3Object.UpdateUb3List(ava);
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

                    Console.WriteLine(e.Message);
                }
            }
            if (metadatastr.StartsWith("Invalid"))
                return ;
            var metadataJson = JObject.Parse(metadatastr);
            ava.id = id;
            ava.Gender = (string)metadataJson["attributes"][0]["value"];
            ava.Score = (int)metadataJson["attributes"][5]["value"];
            ava.traits = new Dictionary<string, string>();
            for (int i = 7; i < 19; i++)
            {
                ava.traits.Add((string)metadataJson["attributes"][i]["trait_type"], (string)metadataJson["attributes"][i]["value"]);
            }
            var collec = DatabaseConnection.GetDb().GetCollection<AvastarObject>("AvastarCollection");
            await collec.InsertOneAsync(ava);
            await UB2Object.UpdateUb2List(ava);
            await UB3Object.UpdateUb3List(ava);
        }
    }
}
