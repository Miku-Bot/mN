using System.Collections.Generic;

namespace mN.DB.Models
{
    public class MikuGuild
    {
        public ulong Id { get; set; }
        public MikuMusic MikuMusic { get; set; }

        /// <summary>
        /// Don't use this!
        /// </summary>
        public MikuGuild() { }

        /// <summary>
        /// Use this!
        /// </summary>
        /// <param name="id">Guild ID</param>
        public MikuGuild(ulong id)
        {
            this.Id = id;
            this.MikuMusic = new MikuMusic()
            {
                Id = id,
                MikuGuild = this,
                QueueTracks = new List<QueueTrack>()
            };
        }
    }
}
