using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Threading.Tasks;
using DSharpPlus.Lavalink;
using System.Linq;
using DSharpPlus;

namespace Ding_Dong_Discord_Bot.Commands
{
    public class MusicCommands : BaseCommandModule
    {
        [Command("join")]
        [Description("Makes the bot join the specified channel")]
        public async Task JoinVC(CommandContext ctx_, DiscordChannel channel_)
        {
            //Check if there is a connection to lavalink...
            var Laval = ctx_.Client.GetLavalink();
            if (!Laval.GetIdealNodeConnection().IsConnected)
            {
                await ctx_.RespondAsync("Lava link connection is not established");
                return;
            }
            //Check is node is connected to a VC...
            var node = Laval.ConnectedNodes.First();
            if (channel_.Type != ChannelType.Voice)
            {
                await ctx_.RespondAsync("Not a valid voice Channel");
                return;
            }

            await node.Value.ConnectAsync(channel_);
            await ctx_.RespondAsync($"Join {channel_.Name}!");
        }

        [Command("leave")]
        [Description("Makes the bot leave the specified channel")]
        public async Task LeaveVC(CommandContext ctx_, DiscordChannel channel_)
        {
            //Check if there is a connection to lavalink...
            var laval = ctx_.Client.GetLavalink();
            if (!laval.GetIdealNodeConnection().IsConnected)
            {
                await ctx_.RespondAsync("Lava link connection is not established");
                return;
            }
            //Check if node is connected to VC...
            var node = laval.ConnectedNodes.First();
            if (channel_.Type != ChannelType.Voice)
            {
                await ctx_.RespondAsync("Not a valid voice Channel");
                return;
            }
            //Check if lavalink is connected...
            var conn = node.Value.GetGuildConnection(channel_.Guild);
            if (conn == null)
            {
                await ctx_.RespondAsync("Lavalink is not Connected");
                return;
            }

            await conn.DisconnectAsync();
            await ctx_.RespondAsync($"Left: {channel_.Name}");
        }

        [Command("play")]
        [Description("plays a song in voice chat")]
        public async Task Play(CommandContext ctx_, [RemainingText] string search_)
        {
            if (ctx_.Member.VoiceState == null || ctx_.Member.VoiceState.Channel == null)
            {
                await ctx_.RespondAsync("You are not in a voice Channel");
                return;
            }

            LavalinkExtension laval = ctx_.Client.GetLavalink();
            LavalinkNodeConnection node = laval.ConnectedNodes.Values.First();
            LavalinkGuildConnection conn = node.GetGuildConnection(ctx_.Member.VoiceState.Guild);
            if (conn == null)
            {
                await ctx_.RespondAsync("lavalink is not connected");
                return;
            }

            var loadresult = await node.Rest.GetTracksAsync(search_);

            if (loadresult.LoadResultType == LavalinkLoadResultType.LoadFailed || loadresult.LoadResultType == LavalinkLoadResultType.NoMatches)
            {
                await ctx_.RespondAsync($"Track search failed for {search_}.");
                return;
            }

            var track = loadresult.Tracks.First();
            await conn.PlayAsync(track);
            await ctx_.RespondAsync($"Now playing {track.Title}");
        }

        [Command("pause")]
        [Description("Pause current song in voice chat")]
        public async Task Pause(CommandContext ctx_)
        {
            if (ctx_.Member.VoiceState == null || ctx_.Member.VoiceState.Channel == null)
            {
                await ctx_.RespondAsync("You are not in a voice Channel");
                return;
            }

            LavalinkExtension laval = ctx_.Client.GetLavalink();
            LavalinkNodeConnection node = laval.ConnectedNodes.Values.First();
            LavalinkGuildConnection conn = node.GetGuildConnection(ctx_.Member.VoiceState.Guild);
            if (conn == null)
            {
                await ctx_.RespondAsync("lavalink is not connected");
                return;
            }

            if (conn.CurrentState.CurrentTrack == null)
            {
                await ctx_.RespondAsync("There is no tracks loaded");
                return;
            }

            await conn.PauseAsync();
        }
    }
}
