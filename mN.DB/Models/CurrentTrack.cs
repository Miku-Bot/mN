using System;

namespace mN.DB.Models
{
    public class CurrentTrack
    {
        public ulong Id { get; set; }
        public MikuMusic MikuMusic { get; set; }
        public string TrackString { get; set; }
        public ulong AddedBy { get; set; }
        public DateTimeOffset AddedAt { get; set; }

        public CurrentTrack() { }

        public CurrentTrack(QueueTrack queueTrack)
        {
            this.Id = queueTrack.Id;
            this.MikuMusic = queueTrack.MikuMusic;
            this.TrackString = queueTrack.TrackString;
            this.AddedBy = queueTrack.AddedBy;
            this.AddedAt = queueTrack.AddedAt;
        }
    }
}
