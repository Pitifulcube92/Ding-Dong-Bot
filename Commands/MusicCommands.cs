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
        public async Task JoinVC(CommandContext ctx_)
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
        public async Task LeaveVC(CommandContext ctx_)
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

            await conn.DisconnectAsync();
            await ctx_.RespondAsync($"Left: {currentChannel.Name}");
        }

        [Command("play"), Priority(0)]
        [Description("plays a song in voice chat")]
        public async Task Play(CommandContext ctx_, [RemainingText] string search_)
        {
            await JoinVC(ctx_);
            if (ctx_.Member.VoiceState == null || ctx_.Member.VoiceState.Channel == null)
            {             
                await ctx_.RespondAsync("Not in a Channel");
                return;
            }
            if (laval == null || node == null || conn == null)
            {
                laval = ctx_.Client.GetLavalink();
                node = laval.ConnectedNodes.Values.First();
                conn = node.GetGuildConnection(ctx_.Member.VoiceState.Guild);
            }
            //search for track
            var loadresult = await node.Rest.GetTracksAsync(search_);

            if (loadresult.LoadResultType == LavalinkLoadResultType.LoadFailed || loadresult.LoadResultType == LavalinkLoadResultType.NoMatches)
            {
                await ctx_.RespondAsync($"Track search failed for {search_}.");
                return;
            }
            //add track to song queue
            queue.AddTrack(loadresult.Tracks.First());
        
            if (queue.ReturnTrack().Count >= 0)
            { 
                await conn.PlayAsync(queue.ReturnTrack().Peek());
                await ctx_.RespondAsync($"Now playing {queue.ReturnTrack().Peek().Title}");
            }
            else
                await ctx_.RespondAsync("There is no tracks in the qeueu!");
            conn.PlaybackFinished += Player_PlaybackFinished;
        }

        [Command("add")]
        [Description("Adds a track to the queue")]
        public async Task Addtrack(CommandContext ctx_, [RemainingText] string search_)
        {
            //check if the bot is in the correct bot 
            if (ctx_.Member.VoiceState == null || ctx_.Member.VoiceState.Channel == null)
            {
                await ctx_.RespondAsync("Not in a Channel");
                return;
            }
            if (laval == null || node == null || conn == null)
            {
              laval = ctx_.Client.GetLavalink();
              node = laval.ConnectedNodes.Values.First();
              conn = node.GetGuildConnection(ctx_.Member.VoiceState.Guild);
            }
            var loadresult = await node.Rest.GetTracksAsync(search_);

            if (loadresult.LoadResultType == LavalinkLoadResultType.LoadFailed || loadresult.LoadResultType == LavalinkLoadResultType.NoMatches)
            {
                await ctx_.RespondAsync($"Track search failed for {search_}.");
                return;
            }
            var track = loadresult.Tracks.First();
            queue.AddTrack(track);
            await ctx_.RespondAsync($"{track.Title} has been added");
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

       private async Task PlayNext()
       {
            queue.RemoveTrack();
            await conn.PlayAsync(queue.ReturnTrack().Peek());          
       }

       private async Task Player_PlaybackFinished(LavalinkGuildConnection con_, TrackFinishEventArgs e_)
       {
            await Task.Delay(500);
            await PlayNext();
        }
    }
}
