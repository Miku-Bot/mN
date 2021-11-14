using System;
using System.Collections.Generic;

namespace mN.DB.Models
{
    public class MikuMusic
    {
        public ulong Id { get; set; }
        public MikuGuild MikuGuild { get; set; }
        //public ulong UsedChannelId { get; set; }

        public ConnectionState ConnectionState { get; set; }
        public MusicOptions MusicOptions { get; set; }
        public PlayState PlayState { get; set; }

        public CurrentTrack CurrentTrack { get; set; }
        public List<QueueTrack> QueueTracks { get; set; }
    }

    [Flags]
    public enum MusicOptions
    {
        RepeatOnce = 1,
        RepeatAll = 2,
        Shuffle = 4
    }

    public enum PlayState
    {
        Stopped,
        Paused,
        Playing
    }

    public enum ConnectionState
    {
        Connected,
        Disconnected
    }
}
