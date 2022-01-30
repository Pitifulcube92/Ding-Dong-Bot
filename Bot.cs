using System.Text;
using System.IO;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;
using DSharpPlus.CommandsNext;
using Newtonsoft.Json;
using Ding_Dong_Discord_Bot.Commands;
using DSharpPlus.Net;
using DSharpPlus.Lavalink;

namespace Ding_Dong_Discord_Bot
{
    class Bot
    {
        //[Client object]
        public DiscordClient Client { get; private set; }
        //[Commands object] 
        public CommandsNextExtension Commands { get; private set; }

        public async Task RunAsync()
        {
            //Create string to read json config...
            var json = string.Empty;
            using (FileStream fs = File.OpenRead("botConfig.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = sr.ReadToEnd();
            ConfigJson configjson = JsonConvert.DeserializeObject<ConfigJson>(json);

            //Set the bots configuration...
            DiscordConfiguration config = new DiscordConfiguration
            {
                Token = configjson.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                MinimumLogLevel = Microsoft.Extensions.Logging.LogLevel.Debug
            };
            Client = new DiscordClient(config);
            Client.Ready += OnClientReady;

            //Set the Command Configuration...
            CommandsNextConfiguration commConfig = new CommandsNextConfiguration
            {
                StringPrefixes = new string[] { configjson.Prefix },
                EnableMentionPrefix = true,
                EnableDms = false
            };
            //Set Endpoint connection...
            ConnectionEndpoint endpoint = new ConnectionEndpoint
            {
                Hostname = "127.0.0.1",
                Port = 2333
            };
            //Set Lavalink Configuration...
            LavalinkConfiguration lavalinkConfig = new LavalinkConfiguration
            {
                Password = "youshallnotpass",
                RestEndpoint = endpoint,
                SocketEndpoint = endpoint
            };

            var lavalink = Client.UseLavalink();

            //Await next Command...
            Commands = Client.UseCommandsNext(commConfig);
            Commands.RegisterCommands<BasicCommands>();
            Commands.RegisterCommands<MusicCommands>();
            await Client.ConnectAsync();
            await lavalink.ConnectAsync(lavalinkConfig);
            await Task.Delay(-1);
        }

        //  
        private Task OnClientReady(DiscordClient x_, ReadyEventArgs y_)
        {
            return Task.CompletedTask;
        }
    }
}
