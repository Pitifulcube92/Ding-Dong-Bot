using System;

namespace Ding_Dong_Discord_Bot
{
    class Program
    {
        static void Main(string[] args)
        {
            Bot initBot = new Bot();
            initBot.RunAsync().GetAwaiter().GetResult();
            Console.WriteLine("Bot is initiated!");
        }
    }
}
