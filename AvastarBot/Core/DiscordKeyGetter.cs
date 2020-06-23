using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace AvastarBot
{
    public class DiscordKeyGetter
    {
        private static readonly string keyPath = "BotKey/AvastarBotKey.txt"; // server
        private static readonly string dbUrlPath = "DbKey/DbKey.txt";
        public static string GetKey()
        {
            if (File.Exists(keyPath))
            {
                using (StreamReader sr = new StreamReader(keyPath, Encoding.UTF8))
                {
                    string key = sr.ReadToEnd();
                    return key;
                }
            }
            else
            {
                return "";
            }
        }
        public static string GetDBUrl()
        {
            if (File.Exists(dbUrlPath))
            {
                using (StreamReader sr = new StreamReader(dbUrlPath, Encoding.UTF8))
                {
                    string key = sr.ReadToEnd();
                    return key;
                }
            }
            else return "";
        }
        public static string GetFileData(string file)
        {
            if (File.Exists(file))
            {
                using (StreamReader sr = new StreamReader(file, Encoding.UTF8))
                {
                    string key = sr.ReadToEnd();
                    return key;
                }
            }
            else return "";
        }
    }
}
