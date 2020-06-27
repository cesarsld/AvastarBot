using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;
using MongoDB.Bson;
namespace AvastarBot.Mongo
{
    public class UB2Object
    {
        public ObjectId id;
        public string Trait1Name;
        public string Trait1Type;

        public string Trait2Name;
        public string Trait2Type;

        public List<long> Match;
        public UB2Object(string name1, string name2, string type1, string type2, long avaId)
        {
            id = ObjectId.GenerateNewId();
            Trait1Name = name1;
            Trait2Name = name2;
            Trait1Type = type1;
            Trait2Type = type2;
            Match = new List<long>();
            Match.Add(avaId);
        }

        private bool CheckCombo(string n1, string n2, string t1, string t2)
        {
            return Trait1Name == n1 && Trait2Name == n2 && Trait1Type == t1 && Trait2Type == t2;
        }

        public static async Task GenerateUB2List()
        {
            var ub2List = new List<UB2Object>();
            var list = await AvastarObject.GetSeriesList();
            foreach (var ava in list)
            {
                ava.traits.Remove("background_color");
                ava.traits.Remove("backdrop");
                var kp = ava.traits.ToList();
                for (int i = 0; i < kp.Count - 1; i++)
                {
                    for (int j = i + 1; j < kp.Count; j++)
                    {
                        var combo = ub2List.FirstOrDefault(c => c.CheckCombo(kp[i].Key, kp[j].Key, kp[i].Value, kp[j].Value));
                        if (combo == null)
                            ub2List.Add(new UB2Object(kp[i].Key, kp[j].Key, kp[i].Value, kp[j].Value, ava.id));
                        else
                            combo.Match.Add(ava.id);
                    }
                }
            }
            var ub2Collec = DatabaseConnection.GetDb().GetCollection<UB2Object>("UB2Collection");
            await ub2Collec.InsertManyAsync(ub2List);
        }

        public static async Task UpdateUb2List(AvastarObject ava)
        {
            var ub2Collec = DatabaseConnection.GetDb().GetCollection<UB2Object>("UB2Collection");
            ava.traits.Remove("background_color");
            ava.traits.Remove("backdrop");
            var kp = ava.traits.ToList();
            for (int i = 0; i < kp.Count - 1; i++)
            {
                for (int j = i + 1; j < kp.Count; j++)
                {
                    var combo = (await ub2Collec.FindAsync(c => c.Trait1Name == kp[i].Key && c.Trait2Name == kp[j].Key && c.Trait1Type == kp[i].Value && c.Trait2Type == kp[j].Value)).FirstOrDefault();

                    if (combo == null)
                    {
                        await ub2Collec.InsertOneAsync(new UB2Object(kp[i].Key, kp[j].Key, kp[i].Value, kp[j].Value, ava.id));
                    }
                    else
                    {
                        combo.Match.Add(ava.id);
                        var update = Builders<UB2Object>.Update.Set(c => c.Match, combo.Match);
                        await ub2Collec.FindOneAndUpdateAsync(c => c.id == combo.id, update);
                    }
                }
            }
        }
    }
}
