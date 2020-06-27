using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;
using MongoDB.Bson;
namespace AvastarBot.Mongo
{
    public class AvaUBObject
    {
        public long id;
        public List<string> ub2List;
        public List<string> ub3List;
        public AvaUBObject(long avaId, List<UB2Object> list)
        {
            id = avaId;
            ub2List = list.Select(ub => ub.id.ToString()).ToList();
            ub3List = null;
        }

        public static async Task GenerateAvaUbList()
        {
            var avaList = await AvastarObject.GetAvaList();
            var ub2TotalList = await UB2Object.GetUb2List();
            var avaUBList = new List<AvaUBObject>();
            foreach (var ava in avaList)
            {
                ava.traits.Remove("background_color");
                ava.traits.Remove("backdrop");
                var gender = ava.Gender;
                var ub2List = new List<UB2Object>();
                var kp = ava.traits.ToList();
                for (int i = 0; i < kp.Count - 1; i++)
                {
                    for (int j = i + 1; j < kp.Count; j++)
                    {
                        var combo = ub2TotalList.Where(c => c.Trait1Name == kp[i].Key && c.Trait2Name == kp[j].Key && c.Trait1Type == kp[i].Value && c.Trait2Type == kp[j].Value).FirstOrDefault();
                        ub2List.Add(combo);
                    }
                }
                var ub2Many = ub2List.Where(ub2 => ub2.Match.Count > 1).ToList();
                ub2List = ub2List.Where(ub2 => ub2.Match.Count == 1).ToList();
                foreach (var ubs in ub2Many)
                {
                    bool uniqueGender = true;
                    foreach (var token in ubs.Match)
                    {
                        if (avaList.Where(a => a.id == token).FirstOrDefault().Gender == gender)
                        {
                            uniqueGender = false;
                            break;
                        }
                    }
                    if (uniqueGender)
                    {
                        ubs.Match = new List<long>() { ava.id };
                        ub2List.Add(ubs);
                    }
                }
                avaUBList.Add(new AvaUBObject(ava.id, ub2List));
            }
            var ub3TotalList = await UB3Object.GetUb3List();
            foreach (var ava in avaList)
            {
                var gender = ava.Gender;
                var ub3List = new List<UB3Object>();
                var kp = ava.traits.ToList();
                for (int i = 0; i < kp.Count - 2; i++)
                {
                    for (int j = i + 1; j < kp.Count - 1; j++)
                    {
                        for (int k = j + 1; k < kp.Count; k++)
                        {
                            var combo = ub3TotalList.Where(c => c.Trait1Name == kp[i].Key && c.Trait2Name == kp[j].Key && c.Trait3Name == kp[k].Key && c.Trait1Type == kp[i].Value && c.Trait2Type == kp[j].Value && c.Trait3Type == kp[k].Value).FirstOrDefault();
                            ub3List.Add(combo);
                        }
                    }
                }
                var ub3Many = ub3List.Where(ub3 => ub3.Match.Count > 1).ToList();
                ub3List = ub3List.Where(ub3 => ub3.Match.Count == 1).ToList();
                foreach (var ubs in ub3Many)
                {
                    bool uniqueGender = true;
                    foreach (var token in ubs.Match)
                    {
                        if (avaList.Where(a => a.id == token).FirstOrDefault().Gender == gender)
                        {
                            uniqueGender = false;
                            break;
                        }
                    }
                    if (uniqueGender)
                    {
                        ubs.Match = new List<long>() { ava.id };
                        ub3List.Add(ubs);
                    }
                    avaUBList.Where(a => a.id == ava.id).FirstOrDefault().ub3List = ub3List.Select(ub => ub.id.ToString()).ToList();
                }
            }
            var collec = DatabaseConnection.GetDb().GetCollection<AvaUBObject>("AvaUbCollection");
            await collec.InsertManyAsync(avaUBList);
        }
    }
}
