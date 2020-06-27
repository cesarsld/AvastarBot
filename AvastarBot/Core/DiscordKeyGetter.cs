using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace AvastarBot
{
    public class DiscordKeyGetter
    {
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
