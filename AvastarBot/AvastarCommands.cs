using System;
using System.Threading.Tasks;
using System.Globalization;
using System.Numerics;
using Discord;
using Discord.Commands;
using Newtonsoft.Json.Linq;
namespace AvastarBot
{
    public class AvastarCommands : ModuleBase
    {
        public AvastarCommands()
        {
        }
        public static string Capitalise(string str)
        {
            var newStr = "";
            bool space = false;
            for (int i = 0; i < str.Length; i++)
            {
                if (i == 0)
                {
                    newStr += str[0].ToString().ToUpper();
                    continue;
                }
                if (space && str[i] != ' ')
                {
                    space = false;
                    newStr += str[i].ToString().ToUpper();
                }
                else if (str[i] == ' ')
                {
                    newStr += str[i];
                    space = true;
                }
                else
                    newStr += str[i];
            }
            if (newStr == "Background Color")
                return "Bg Color";
            return newStr;
        }

        public string ReturnTraitOfRarity(JObject ava, JObject dict, string rarity)
        {
            var returnStr = "";
            var gender = (string)ava["attributes"][0]["value"];
            for (int i = 7; i < 19; i++)
            {
                var traitType = (string)ava["attributes"][i]["trait_type"];
                traitType = traitType.Replace('_', ' ');
                traitType = Capitalise(traitType);
                foreach (var trait in dict[traitType])
                {
                    if (!trait.HasValues)
                        continue;
                    if ((string)trait["name"] == (string)ava["attributes"][i]["value"] &&
                        (gender.ToLower() == ((string)trait["gender"]).ToLower() || ((string)trait["gender"]).ToLower() == "any"))
                    {
                        if ((string)trait["rarity"] == rarity)
                        {
                            returnStr += " -     " + (string)trait["name"] + $" ({trait["gene"]})" + "\n";
                        }
                        break;
                    }
                }
            }
            if (returnStr.Length == 0)
                return "None";
            return returnStr;
        }

        public int[] ReturnTraitDisparity(JObject ava, JObject dict)
        {
            var disparity = new int[5] { 0, 0, 0, 0, 0 };
            var gender = (string)ava["attributes"][0]["value"];
            for (int i = 7; i < 19; i++)
            {
                var traitType = (string)ava["attributes"][i]["trait_type"];
                traitType = traitType.Replace('_', ' ');
                traitType = Capitalise(traitType);
                foreach (var trait in dict[traitType])
                {
                    if (!trait.HasValues)
                        continue;
                    if ((string)trait["name"] == (string)ava["attributes"][i]["value"] &&
                        (gender.ToLower() == ((string)trait["gender"]).ToLower() || ((string)trait["gender"]).ToLower() == "any"))
                    {
                        switch ((string)trait["rarity"])
                        {
                            case "Common":
                                disparity[0]++;
                                break;
                            case "Uncommon":
                                disparity[1]++;
                                break;
                            case "Rare":
                                disparity[2]++;
                                break;
                            case "Epic":
                                disparity[3]++;
                                break;
                            case "Legendary":
                                disparity[4]++;
                                break;
                        }
                        break;
                    }
                }
            }
            return disparity;
        }

        [Command("avastar", RunMode = RunMode.Async)]
        [Alias("ava", "star")]
        public async Task PostAvastarData(int id, string opt = "", string max = "")
        {
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
                return;
            var metadataJson = JObject.Parse(metadatastr);
            var traitJson = JObject.Parse(DiscordKeyGetter.GetFileData("data/create-traits-nosvg.json"));
            var embed = new EmbedBuilder().WithTitle("Avastar #" + id.ToString()).
                WithUrl("https://avastars.io/avastar/" + id.ToString());
            if (Context.Channel.Id == 664598104695242782 || Context.Channel.Id == 706908035540320300 || max.Length > 0)
               embed.WithImageUrl("https://avastars.io/media/" + id.ToString());
            else
                embed.WithThumbnailUrl("https://avastars.io/media/" + id.ToString());
            var rarity = (string)metadataJson["attributes"][6]["value"];
            switch (rarity)
            {
                case "Common":
                    embed.WithColor(new Color(13, 120, 249));
                    break;
                case "Uncommon":
                    embed.WithColor(new Color(29, 226, 132));
                    break;
                case "Rare":
                    embed.WithColor(new Color(251, 161, 22));
                    break;
                case "Epic":
                    embed.WithColor(new Color(99, 68, 197));
                    break;
                case "Legendary":
                    embed.WithColor(new Color(252, 42, 78));
                    break;
            }
            var disp = ReturnTraitDisparity(metadataJson, traitJson);
            embed.WithDescription($"Score : {metadataJson["attributes"][5]["value"]}\nTrait distribution : <:iconCommon:723497539571154964> {disp[0]} <:iconUncommon:723497171395018762> {disp[1]} <:iconRare:723497171919306813> {disp[2]} <:iconEpic:723497171957317782> {disp[3]} <:iconLegendary:723497171147685961> {disp[4]}");
            if (opt.ToLower().StartsWith("leg"))
                embed.AddField("<:iconLegendary:723497171147685961> Legendary traits:", ReturnTraitOfRarity(metadataJson, traitJson, "Legendary"));
            if (opt.ToLower().StartsWith("epi"))
                embed.AddField("<:iconEpic:723497171957317782> Epic traits:", ReturnTraitOfRarity(metadataJson, traitJson, "Epic"));
            if (opt.ToLower().StartsWith("rare"))
                embed.AddField("<:iconRare:723497171919306813> Rare traits:", ReturnTraitOfRarity(metadataJson, traitJson, "Rare"));
            if (opt.ToLower().StartsWith("com"))
                embed.AddField("<:iconCommon:723497539571154964> Common traits:", ReturnTraitOfRarity(metadataJson, traitJson, "Common"));
            if (opt.ToLower().StartsWith("unc"))
                embed.AddField("<:iconUncommon:723497171395018762> Uncommon traits:", ReturnTraitOfRarity(metadataJson, traitJson, "Uncommon"));
            //embed.WithDescription($"Score : {metadataJson["attributes"][5]["value"]}\nTrait disparity : {disp[0]} <:iconCommon:723497539571154964> {disp[1]} <:iconUncommon:723497171395018762>   {disp[2]} <:iconRare:723497171919306813>   {disp[3]} <:iconEpic:723497171957317782>   {disp[4]} <:iconLegendary:723497171147685961>");
            await ReplyAsync(embed: embed.Build());
        }

        [Command("remaining")]
        [Alias("Remain", "remainder", "rem")]
        public async Task GetRemainingAvastarsOfSeries()
        {
            var totalSupply = await Blockchain.ChainWatcher.GetAvastarCount();
            var remainder = 5000 - (totalSupply - 200) % 5000;
            var series = ((totalSupply - 200) / 5000) + 1;
            var embed = new EmbedBuilder().WithColor(Color.Red).WithTitle($"{remainder} Avastars remain to mint for series {series}!");
            embed.WithUrl("https://avastars.io/").WithDescription("Keep Scrolling! ⏬⏬⏬");
            await ReplyAsync(embed: embed.Build());
        }
        //public async Task GetHelp()
        //{
        //    var embed = new Embed
        //}
    }
}
