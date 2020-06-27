using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;
using MongoDB.Bson;
namespace AvastarBot.Mongo
{
    public class UB3Object
    {
        public ObjectId id;
        public string Trait1Name;
        public string Trait1Type;

        public string Trait2Name;
        public string Trait2Type;

        public string Trait3Name;
        public string Trait3Type;

        public List<long> Match;
        public UB3Object(string name1, string name2, string name3, string type1, string type2, string type3, long avaId)
        {
            id = ObjectId.GenerateNewId();
            Trait1Name = name1;
            Trait2Name = name2;
            Trait3Name = name3;
            Trait1Type = type1;
            Trait2Type = type2;
            Trait3Type = type3;
            Match = new List<long>();
            Match.Add(avaId);
        }

        public static async Task<List<UB3Object>> GetUB3CombosForId(int id)
        {
            var ub3List = new List<UB3Object>();
            var ub3Collec = DatabaseConnection.GetDb().GetCollection<UB3Object>("UB3Collection");
            var ava = await AvastarObject.GetAva(id);
            ava.traits.Remove("background_color");
            ava.traits.Remove("backdrop");
            var kp = ava.traits.ToList();
            for (int i = 0; i < kp.Count - 2; i++)
            {
                for (int j = i + 1; j < kp.Count - 1; j++)
                {
                    for (int k = j + 1; k < kp.Count; k++)
                    {
                        var combo = (await ub3Collec.FindAsync(c => c.Trait1Name == kp[i].Key && c.Trait2Name == kp[j].Key && c.Trait3Name == kp[k].Key && c.Trait1Type == kp[i].Value && c.Trait2Type == kp[j].Value && c.Trait3Type == kp[k].Value)).FirstOrDefault();
                        ub3List.Add(combo);
                    }
                }
            }
            ub3List = ub3List.Where(ub3 => ub3.Match.Count == 1).ToList();
            return ub3List;
        }

        private bool CheckCombo(string n1, string n2, string n3, string t1, string t2, string t3)
        {
            return Trait1Name == n1 && Trait2Name == n2 && Trait3Name == n3
                && Trait1Type == t1 && Trait2Type == t2 && Trait3Type == t3;
        }

        public static async Task GenerateUB3List()
        {
            var ub3List = new List<UB3Object>();
            var list = await AvastarObject.GetSeriesList();
            foreach (var ava in list)
            {
                ava.traits.Remove("background_color");
                ava.traits.Remove("backdrop");
                var kp = ava.traits.ToList();
                for (int i = 0; i < kp.Count - 2; i++)
                {
                    for (int j = i + 1; j < kp.Count - 1; j++)
                    {
                        for (int k = j + 1; k < kp.Count; k++)
                        {
                            var combo = ub3List.FirstOrDefault(c => c.CheckCombo(kp[i].Key, kp[j].Key, kp[k].Key, kp[i].Value, kp[j].Value, kp[k].Value));
                            if (combo == null)
                                ub3List.Add(new UB3Object(kp[i].Key, kp[j].Key, kp[k].Key, kp[i].Value, kp[j].Value, kp[k].Value, ava.id));
                            else
                                combo.Match.Add(ava.id);
                        }
                    }
                }
            }
            var ub3Collec = DatabaseConnection.GetDb().GetCollection<UB3Object>("UB3Collection");
            await ub3Collec.InsertManyAsync(ub3List);
        }

        //TODO improve function to prevent 220 db calls...
        public static async Task UpdateUb3List(AvastarObject ava)
        {
            var ub3Collec = DatabaseConnection.GetDb().GetCollection<UB3Object>("UB3Collection");
            ava.traits.Remove("background_color");
            ava.traits.Remove("backdrop");
            var kp = ava.traits.ToList();
            for (int i = 0; i < kp.Count - 2; i++)
            {
                for (int j = i + 1; j < kp.Count - 1; j++)
                {
                    for (int k = j + 1; k < kp.Count; k++)
                    {
                        var combo = (await ub3Collec.FindAsync(c => c.Trait1Name == kp[i].Key && c.Trait2Name == kp[j].Key && c.Trait3Name == kp[k].Key && c.Trait1Type == kp[i].Value && c.Trait2Type == kp[j].Value && c.Trait3Type == kp[k].Value)).FirstOrDefault();

                        if (combo == null)
                        {
                            await ub3Collec.InsertOneAsync(new UB3Object(kp[i].Key, kp[j].Key, kp[k].Key, kp[i].Value, kp[j].Value, kp[k].Value, ava.id));
                        }
                        else
                        {
                            combo.Match.Add(ava.id);
                            var update = Builders<UB3Object>.Update.Set(c => c.Match, combo.Match);
                            await ub3Collec.FindOneAndUpdateAsync(c => c.id == combo.id, update);
                        }
                    }
                }
            }
        }
    }

}
