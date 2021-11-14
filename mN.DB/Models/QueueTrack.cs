using System;

namespace mN.DB.Models
{
    public class QueueTrack
    {
        public ulong Id { get; set; }
        public MikuMusic MikuMusic { get; set; }

        public int Position { get; set; }
        public string TrackString { get; set; }
        public ulong AddedBy { get; set; }
        public DateTimeOffset AddedAt { get; set; }

        public QueueTrack() { }

        public QueueTrack(CurrentTrack current, int position)
        {
            this.Id = current.Id;
            this.MikuMusic = current.MikuMusic;
            this.Position = position;
            this.TrackString = current.TrackString;
            this.AddedBy = current.AddedBy;
            this.AddedAt = current.AddedAt;
        }
    }
}
