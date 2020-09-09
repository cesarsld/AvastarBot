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

        public static async Task<List<AvaUBObject>> GetAvaUbList()
        {
            var collec = DatabaseConnection.GetDb().GetCollection<AvaUBObject>("AvaUbCollection");
            return (await collec.FindAsync(a => true)).ToList();
        }

        public static async Task<AvaUBObject> GetAvaUbObjectById(long id)
        {
            var collec = DatabaseConnection.GetDb().GetCollection<AvaUBObject>("AvaUbCollection");
            return (await collec.FindAsync(a => a.id == id)).FirstOrDefault();
        }

        public static async Task GenerateAvaUbList()
        {
            var avaList = await AvastarObject.GetSeriesList();
            var ub2TotalList = await UB2Object.GetUb2List();
            var avaUBList = new List<AvaUBObject>();
            //Ub2 List Generation
            var count = avaList.Count;
            var index = 0;
            foreach (var ava in avaList)
            {
                Console.WriteLine($"{index} out of {count}");
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
                        if (token != ava.id && avaList.Where(a => a.id == token).FirstOrDefault().Gender == gender)
                        {
                            uniqueGender = false;
                            break;
                        }
                    }
                    if (uniqueGender)
                        ub2List.Add(ubs);
                }
                avaUBList.Add(new AvaUBObject(ava.id, ub2List));
                Console.SetCursorPosition(0, 2 - 1);
                index++;
            }
            index = 0;
            var ub3TotalList = await UB3Object.GetUb3List();
            //Generating Ub3 list
            foreach (var ava in avaList)
            {
                Console.WriteLine($"{index} out of {count}");
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
                        if (token != ava.id && avaList.Where(a => a.id == token).FirstOrDefault().Gender == gender)
                        {
                            uniqueGender = false;
                            break;
                        }
                    }
                    if (uniqueGender)
                        ub3List.Add(ubs);
                    avaUBList.Where(a => a.id == ava.id).FirstOrDefault().ub3List = ub3List.Select(ub => ub.id.ToString()).ToList();
                }
                Console.SetCursorPosition(0, 3 - 1);
                index++;
            }
            var collec = DatabaseConnection.GetDb().GetCollection<AvaUBObject>("AvaUbCollection");
            await collec.InsertManyAsync(avaUBList);
        }

        public static async Task UpdateAvaUbList(AvastarObject ava, List<UB2Object> ub2List, List<UB3Object> ub3List)
        {
            var ub2Copy = ub2List;
            var ub3Copy = ub3List;
            var avaList = await AvastarObject.GetSeriesList();
            var avaUbList = await GetAvaUbList();
            var gender = ava.Gender;
            //Computuing UB2s
            var ub2Many = ub2List.Where(ub2 => ub2.Match.Count > 1).ToList();
            ub2List = ub2List.Where(ub2 => ub2.Match.Count == 1).ToList();
            foreach (var ubs in ub2Many)
            {
                bool uniqueGender = true;
                foreach (var token in ubs.Match)
                {
                    if (token != ava.id && avaList.Where(a => a.id == token).FirstOrDefault().Gender == gender)
                    {
                        uniqueGender = false;
                        break;
                    }
                }
                if (uniqueGender)
                    ub2List.Add(ubs);
            }
            var avaUbObj = new AvaUBObject(ava.id, ub2List);
            var collec = DatabaseConnection.GetDb().GetCollection<AvaUBObject>("AvaUbCollection");
            await collec.InsertOneAsync(avaUbObj);
            avaUbList.Add(avaUbObj);
            foreach (var ub in ub2List)
                ub2Copy.Remove(ub);
            // ub2Copy contains all combos that are duplicates
            // get all ID in each combo, find only the ones that are same gender
            // access avaubdata and remove ID of combo
            try
            {
                foreach (var ub in ub2Copy)
                {
                    foreach (var id in ub.Match)
                    {
                        var tempAva = avaList.Where(a => a.id == id).FirstOrDefault();
                        if (tempAva == null)
                            continue;
                        if (tempAva.Gender == ava.Gender)
                        {
                            var tempAvaUb = avaUbList.Where(a => a.id == id).FirstOrDefault();
                            if (tempAvaUb == null)
                                continue;
                            if (tempAvaUb.ub2List.Remove(ub.id.ToString()))
                            {
                                var update = Builders<AvaUBObject>.Update.Set(a => a.ub2List, tempAvaUb.ub2List);
                                await collec.UpdateOneAsync(a => a.id == tempAvaUb.id, update);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            //Computing UB3s
            try
            {
                var ub3Many = ub3List.Where(ub3 => ub3.Match.Count > 1).ToList();
                ub3List = ub3List.Where(ub3 => ub3.Match.Count == 1).ToList();
                foreach (var ubs in ub3Many)
                {
                    bool uniqueGender = true;
                    foreach (var token in ubs.Match)
                    {
                        if (token != ava.id && avaList.Where(a => a.id == token).FirstOrDefault().Gender == gender)
                        {
                            uniqueGender = false;
                            break;
                        }
                    }
                    if (uniqueGender)
                        ub3List.Add(ubs);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            avaUbObj.ub3List = ub3List.Select(a => a.id.ToString()).ToList();
            var ub3Update = Builders<AvaUBObject>.Update.Set(a => a.ub3List, avaUbObj.ub3List);
            await collec.UpdateOneAsync(a => a.id == ava.id, ub3Update);
            foreach (var ub in ub3List)
                ub3Copy.Remove(ub);
            foreach (var ub in ub3Copy)
            {
                foreach (var id in ub.Match)
                {
                    var tempAva = avaList.Where(a => a.id == id).FirstOrDefault();
                    if (tempAva == null)
                        continue;
                    if (tempAva.Gender == ava.Gender)
                    {
                        var tempAvaUb = avaUbList.Where(a => a.id == id).FirstOrDefault();
                        if (tempAvaUb == null)
                            continue;
                        if (tempAvaUb.ub3List.Remove(ub.id.ToString()))
                        {
                            var update = Builders<AvaUBObject>.Update.Set(a => a.ub3List, tempAvaUb.ub3List);
                            await collec.UpdateOneAsync(a => a.id == tempAvaUb.id, update);
                        }
                    }
                }
            }
        }
    }
}
