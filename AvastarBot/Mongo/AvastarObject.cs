using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using MongoDB.Bson;
using System.Linq;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
namespace AvastarBot.Mongo
{
    public class AvastarObject {
        public int id;
        public string Gender;
        public int Score;
        public string Owner;
        public Dictionary<string, string> traits;
        public Dictionary<string, string> TraitsRarity;
        public Dictionary<string, int> RarityDistribution;

        public static async Task<AvastarObject> GetAva(int id) {
            var collec = DatabaseConnection.GetDb().GetCollection<AvastarObject>("AvastarCollection");
            var ava = await collec.FindAsync(a => a.id == id);
            return ava.FirstOrDefault();
        }

        public static async Task<int> GetAvaCount() {
            var collec = DatabaseConnection.GetDb().GetCollection<AvastarObject>("AvastarCollection");
            return (int)(await collec.CountDocumentsAsync(a => true));
        }

        public static async Task<List<AvastarObject>> GetAvaList() {
            var collec = DatabaseConnection.GetDb().GetCollection<AvastarObject>("AvastarCollection");
            return (await collec.FindAsync(a => true)).ToList();
        }

        public static async Task<List<AvastarObject>> GetSerieList(int series) {
            var collec = DatabaseConnection.GetDb().GetCollection<AvastarObject>("AvastarCollection");
            return (await collec.FindAsync(a => a.id > 199 + 5000 * (series - 1) && a.id < 200 + 5000 * series)).ToList();
        }

        public static async Task<List<AvastarObject>> GetSeriesList() {
            var collec = DatabaseConnection.GetDb().GetCollection<AvastarObject>("AvastarCollection");
            return (await collec.FindAsync(a => a.id > 199)).ToList();
        }

        public static async Task UpdateUBs(int id) {
            var collec = DatabaseConnection.GetDb().GetCollection<AvastarObject>("AvastarCollection");
            var ava = (await collec.FindAsync(a => a.id == id)).FirstOrDefault();
            var ub2List = await UB2Object.UpdateUb2List(ava);
            var ub3List = await UB3Object.UpdateUb3List(ava);
            await AvaUBObject.UpdateAvaUbList(ava, ub2List, ub3List);
        }

        public static async Task UpdateOwner(int id, string newOwner) {
            var collec = DatabaseConnection.GetDb().GetCollection<AvastarObject>("AvastarCollection");
            var update = Builders<AvastarObject>.Update.Set(a => a.Owner, newOwner);
            await collec.UpdateOneAsync(a => a.id == id, update);
        }

        public static async Task MigrateAll() {
            for (int i = 669; i < 11187; i++) {
                try {
                    await MigrateAva(i);
                    Console.WriteLine($"Migration for #{i} done");
                }
                catch (Exception e) {
                    Console.WriteLine(e.Message);
                    return;
                }
            }
        }

        public static async Task FinalSync() {
            var collec = DatabaseConnection.GetDb().GetCollection<AvastarObject>("AvastarCollection");
            for (int i = 0; i < 11187; i++) {
                var update = Builders<AvastarObject>.Update.Set(a => a.Owner, await Blockchain.ChainWatcher.GetOwnerOf(i));
                await collec.UpdateOneAsync(a => a.id == i, update);
            }
        }

        public static async Task MigrateAva(int id) {
            var oldAva = await AvastarObject.GetAva(id);
            var ava = new AvastarObject();
            ava.id = oldAva.id;
            ava.Gender = oldAva.Gender;
            ava.Score = oldAva.Score;
            ava.traits = oldAva.traits;
            ava.Owner = "";// await Blockchain.ChainWatcher.GetOwnerOf(id);
            ava.TraitsRarity = new Dictionary<string, string>();
            var traitJson = JObject.Parse(DiscordKeyGetter.GetFileData("app/create-traits-nosvg.json"));
            foreach (var pair in ava.traits) {
                var traitType = pair.Key;
                var traitName = pair.Value;
                var traitRarity = "";
                var traitTypeName = AvastarCommands.Capitalise(traitType.Replace('_', ' '));
                foreach (var trait in traitJson[traitTypeName]) {
                    if (trait.Type == JTokenType.Null)
                        continue;
                    if ((string)trait["name"] == traitName) {
                        traitRarity = (string)trait["rarity"];
                        break;
                    }
                }
                ava.TraitsRarity.Add(traitType, traitRarity);
            }
            ava.RarityDistribution = new Dictionary<string, int>();
            ava.RarityDistribution.Add("Common", 0);
            ava.RarityDistribution.Add("Uncommon", 0);
            ava.RarityDistribution.Add("Rare", 0);
            ava.RarityDistribution.Add("Epic", 0);
            ava.RarityDistribution.Add("Legendary", 0);
            foreach (var pair in ava.TraitsRarity) {
                switch (pair.Value) {
                    case "Common":
                        ava.RarityDistribution["Common"]++;
                        break;
                    case "Uncommon":
                        ava.RarityDistribution["Uncommon"]++;
                        break;
                    case "Rare":
                        ava.RarityDistribution["Rare"]++;
                        break;
                    case "Epic":
                        ava.RarityDistribution["Epic"]++;
                        break;
                    case "Legendary":
                        ava.RarityDistribution["Legendary"]++;
                        break;
                }
            }
            var collec = DatabaseConnection.GetDb().GetCollection<AvastarObject>("AvastarCollection");
            await collec.ReplaceOneAsync(a => a.id == id, ava);
        }

        public static async Task CreateAva(int id) {
            var ava = new AvastarObject();
            ava.id = id;
            string metadatastr = "";
            using (System.Net.WebClient wc = new System.Net.WebClient()) {
                try {
                    metadatastr = await wc.DownloadStringTaskAsync("https://avastars.io/metadata/" + id.ToString());
                }
                catch (Exception e) {
                    Console.WriteLine(e.Message);
                }
            }
            if (metadatastr.StartsWith("Invalid"))
                return;
            var traitJson = JObject.Parse(DiscordKeyGetter.GetFileData("app/create-traits-nosvg.json"));
            var metadataJson = JObject.Parse(metadatastr);
            ava.id = id;
            ava.Gender = (string)metadataJson["attributes"][0]["value"];
            ava.Score = (int)metadataJson["attributes"][5]["value"];
            ava.traits = new Dictionary<string, string>();
            for (int i = 7; i < 19; i++) {
                ava.traits.Add((string)metadataJson["attributes"][i]["trait_type"], (string)metadataJson["attributes"][i]["value"]);
            }
            var disp = AvastarCommands.ReturnTraitDisparity(metadataJson, traitJson);
            ava.Owner = "";
            ava.RarityDistribution = new Dictionary<string, int>();
            ava.RarityDistribution.Add("Common", disp[0]);
            ava.RarityDistribution.Add("Uncommon", disp[1]);
            ava.RarityDistribution.Add("Rare", disp[2]);
            ava.RarityDistribution.Add("Epic", disp[3]);
            ava.RarityDistribution.Add("Legendary", disp[4]);
            ava.FillTraitDictionary(traitJson, metadataJson);
            var collec = DatabaseConnection.GetDb().GetCollection<AvastarObject>("AvastarCollection");
            await collec.InsertOneAsync(ava);
            var ub2List = await UB2Object.UpdateUb2List(ava);
            var ub3List = await UB3Object.UpdateUb3List(ava);
            await AvaUBObject.UpdateAvaUbList(ava, ub2List, ub3List);
        }

        public void FillTraitDictionary(JObject traitJson, JObject metadataJson) {
            TraitsRarity = new Dictionary<string, string>();
            for (int i = 7; i < 19; i++) {
                var traitType = (string)metadataJson["attributes"][i]["trait_type"];
                var traitName = (string)metadataJson["attributes"][i]["value"];
                var traitRarity = "";
                var traitTypeName = AvastarCommands.Capitalise(traitType.Replace('_', ' '));
                foreach (var trait in traitJson[traitTypeName]) {
                    if (trait.Type == JTokenType.Null)
                        continue;
                    if ((string)trait["name"] == traitName) {
                        traitRarity = (string)trait["rarity"];
                        break;
                    }
                }
                TraitsRarity.Add(traitType, traitRarity);
            }
        }
    }
}
