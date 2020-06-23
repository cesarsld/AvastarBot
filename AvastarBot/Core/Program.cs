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
            RunBot();
        }
        static void RunBot()
        {
            while (true)
            {
                try
                {
                    new Bot().RunAsync().GetAwaiter().GetResult();
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
