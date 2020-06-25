using System;
using Discord;
using System.Threading;
using System.Numerics;

namespace AvastarBot
{
    public class Program
    {
        public static bool IsRelease = false;
        static void Main(string[] args)
        {
            Mongo.DatabaseConnection.DatabaseName = "AvastarDatabase";
            if (args.Length == 3)
                IsRelease = true;
            RunBot(token: args[0], mongo_url: args[1]);
        }
        static void RunBot(string token, string mongo_url)
        {
            while (true)
            {
                try
                {
                    new Bot().RunAsync(token, mongo_url).GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    Logger.Log(new LogMessage(LogSeverity.Error, ex.ToString(), "Unexpected Exception", ex));
                    Console.WriteLine(ex.ToString());
                }
                Thread.Sleep(1000);
                break;
            }
        }
    }
}
