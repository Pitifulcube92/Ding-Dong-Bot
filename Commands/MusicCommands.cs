using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Threading.Tasks;
using DSharpPlus.Lavalink;
using System.Linq;
using DSharpPlus;
using System.Collections.Generic;
using Ding_Dong_Discord_Bot.Utitlity;
using DSharpPlus.Lavalink.EventArgs;

namespace Ding_Dong_Discord_Bot.Commands
{ 
    public class MusicCommands : BaseCommandModule
    {
        LavalinkExtension laval;
        LavalinkNodeConnection node;
        LavalinkGuildConnection conn;
        SongQueue queue = new SongQueue();

        [Command("join")]
        [Description("Makes the bot join the specified channel")]
        public async Task TJoinVC(CommandContext ctx_)
        {
            //Check if there is a connection to lavalink...
            var Laval = ctx_.Client.GetLavalink();
            if (!Laval.GetIdealNodeConnection().IsConnected)
            {
                await ctx_.RespondAsync("Lava link connection is not established");
                return;
            }
            var currentChannel = ctx_.Member.VoiceState.Channel;
            //Check is node is connected to a VC...
            var node = Laval.ConnectedNodes.First();
            if (currentChannel.Type != ChannelType.Voice)
            {
                await ctx_.RespondAsync("Not a valid voice Channel");
                return;
            }

            await node.Value.ConnectAsync(currentChannel);
            //await ctx_.RespondAsync($"Join {currentChannel.Name}!");
        }
        [Command("leave")]
        [Description("Makes the bot leave the specified channel")]
        public async Task TLeaveVC(CommandContext ctx_)
        {
            var currentChannel = ctx_.Member.VoiceState.Channel;
            //Check if there is a connection to lavalink...
            var laval = ctx_.Client.GetLavalink();
            if (!laval.GetIdealNodeConnection().IsConnected)
            {
                await ctx_.RespondAsync("Lava link connection is not established");
                return;
            }
            //Check if node is connected to VC...
            var node = laval.ConnectedNodes.First();
            if (currentChannel.Type != ChannelType.Voice)
            {
                await ctx_.RespondAsync("Not a valid voice Channel");
                return;
            }
            //Check if lavalink is connected...
            var conn = node.Value.GetGuildConnection(currentChannel.Guild);
            if (conn == null)
            {
                await ctx_.RespondAsync("Lavalink is not Connected");
                return;
            }
           await ClearSongList();
           await conn.DisconnectAsync();
        }
        [Command("play")]
        [Description("plays songs in voice chat")]
        public async Task TPlay(CommandContext ctx_)
        {
            //join and check if bot is in a channel
            await TJoinVC(ctx_);
            if (ctx_.Member.VoiceState == null || ctx_.Member.VoiceState.Channel == null)
            {             
                await ctx_.RespondAsync("Not in a Channel");
                return;
            }
            //check connections
            await EstablishConnection(ctx_);
            if (queue.ReturnTrack().Count > 0)
            {
                await conn.PlayAsync(queue.ReturnTrack().Peek());
                await ctx_.RespondAsync($"Now playing: **{queue.ReturnTrack().Peek().Title}**");
                conn.PlaybackFinished += Player_PlaybackFinished;
            }
            else
            {
                await ctx_.RespondAsync("There is no tracks in the queue!");
                return;
            }
        }
        [Command("add")]
        [Description("Adds a track to the song list")]
        public async Task TAddtrack(CommandContext ctx_, [RemainingText] string search_)
        {
            //check if the bot is in the correct channel/connection
            if (ctx_.Member.VoiceState == null || ctx_.Member.VoiceState.Channel == null)
            {
                await ctx_.RespondAsync("Not in a Channel");
                return;
            }
            await EstablishConnection(ctx_);
            var loadresult = await node.Rest.GetTracksAsync(search_);

            if (loadresult.LoadResultType == LavalinkLoadResultType.LoadFailed || loadresult.LoadResultType == LavalinkLoadResultType.NoMatches)
            {
                await ctx_.RespondAsync($"Track search failed for {search_}.");
                return;
            }
            //Add track to the 
            var track = loadresult.Tracks.First();
            queue.AddTrack(track);
            await ctx_.RespondAsync($"**{track.Title}** has been added");
        }
        [Command("stop")]
        [Description("Pause current song in voice chat")]
        public async Task TStop(CommandContext ctx_)
        {
            if (ctx_.Member.VoiceState == null || ctx_.Member.VoiceState.Channel == null)
            {
                await ctx_.RespondAsync("You are not in a voice Channel");
                return;
            }
            await EstablishConnection(ctx_);
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

            await conn.StopAsync();
        }
        [Command("clean")]
        [Description("Clears the song list")]
        public async Task TClearSonglist(CommandContext ctx_)
        {
            await TStop(ctx_);
            await ClearSongList();
            await new DiscordMessageBuilder()
                    .WithContent("Song List has been cleaned!")
                    .SendAsync(ctx_.Channel);
        }
        [Command("queue")]
        [Description("Displays the list of song in the playback")]
        public async Task TListqueuedtracks(CommandContext ctx_)
        {
            if (queue.ReturnTrack().Count > 0)
            {
                await new DiscordMessageBuilder()
                    .WithContent("**Here are the songs listed:**")
                    .SendAsync(ctx_.Channel);
                foreach (LavalinkTrack t in queue.ReturnTrack())
                {
                    await new DiscordMessageBuilder()
                          .WithContent($"- {t.Title}")
                          .SendAsync(ctx_.Channel);
                }
            }
            else
            {
                await new DiscordMessageBuilder()
                    .WithContent("No songs are currently queued!")
                    .SendAsync(ctx_.Channel);
            }
        }
        private async Task PlayNext()
        {
            queue.RemoveTrack();
            await conn.PlayAsync(queue.ReturnTrack().Peek());
            await new DiscordMessageBuilder()
                  .WithContent($"Now playing: **{queue.ReturnTrack().Peek().Title}**")
                  .SendAsync(conn.Channel);
        }
        private async Task Player_PlaybackFinished(LavalinkGuildConnection con_, TrackFinishEventArgs e_)
        {
            await Task.Delay(500);
            await PlayNext();
        }
        private async Task ClearSongList()
        {
            if (conn != null)
            {
                queue.ClearTracks();
                await Task.CompletedTask;
            }
        }
        private async Task EstablishConnection(CommandContext ctx_)
        {
            laval = ctx_.Client.GetLavalink();
            node = laval.ConnectedNodes.Values.First();
            conn = node.GetGuildConnection(ctx_.Member.VoiceState.Guild);
            await Task.CompletedTask;
        }

    }
}
