using System;
using System.Collections.Generic;
using System.Text;
using  DSharpPlus.Lavalink;

namespace Ding_Dong_Discord_Bot.Utitlity
{
    class SongQueue
    {
        private readonly Queue<LavalinkTrack> playlist = new Queue<LavalinkTrack>();
        public bool isPlaying;
        public void AddTrack(LavalinkTrack track_)
        {
            playlist.Enqueue(track_);
        }
        public void RemoveTrack()
        {
            playlist.Dequeue();
        }
        public void ClearTracks()
        {
            playlist.Clear();
        }

        public Queue<LavalinkTrack> ReturnTrack()
        {
            return playlist;
        } 
    }
}
