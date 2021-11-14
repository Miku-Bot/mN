using DSharpPlus.Lavalink;
using Microsoft.EntityFrameworkCore;
using mN.DB;
using mN.DB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mN.D.Extensions
{
    public static class MikuContextExtensions
    {
        /// <summary>
        /// Get MikuMusic via the ID (and DbContext)
        /// </summary>
        /// <param name="context">DB Context</param>
        /// <param name="guildId">Guild ID</param>
        /// <returns>MikuMusic of the Guild</returns>
        public static async Task<MikuMusic> GetMikuMusicAsync(this MikuContext context, ulong guildId)
        {
            var theGuild = await context.MikuGuilds.Include(x => x.MikuMusic)
                .ThenInclude(x => x.QueueTracks)
                .Include(x => x.MikuMusic)
                .ThenInclude(x => x.CurrentTrack)
                .FirstOrDefaultAsync(x => x.Id == guildId);
            return theGuild.MikuMusic;
        }

        /// <summary>
        /// Get the next song from DB, sets the "CurrentSong" property to whats to be played next
        /// </summary>
        /// <param name="context">Miku DB context</param>
        /// <param name="guildId">Guild ID</param>
        /// <returns>To be played LavalinkTrack</returns>
        public static async Task<LavalinkTrack> GetNextSongAsync(this MikuContext context, ulong guildId)
        {
            var music = await context.GetMikuMusicAsync(guildId);
            var queueEmpty = music.QueueTracks.Count == 0;

            if (music.MusicOptions.HasFlag(MusicOptions.RepeatOnce))
                return LavalinkUtilities.DecodeTrack(music.CurrentTrack.TrackString);

            else if (music.MusicOptions.HasFlag(MusicOptions.RepeatAll | MusicOptions.Shuffle))
            {
                if (queueEmpty)
                    return LavalinkUtilities.DecodeTrack(music.CurrentTrack.TrackString);

                var rng = new Random();
                var randomEntry = music.QueueTracks[rng.Next(0, music.QueueTracks.Count)];
                music.QueueTracks.Remove(randomEntry);
                music.QueueTracks.Add(new QueueTrack(music.CurrentTrack, music.QueueTracks.Count));
                var newOrder = ReorderQueue(music.QueueTracks);
                music.QueueTracks.AddRange(newOrder);
                music.CurrentTrack = new(randomEntry);
            }

            else if (music.MusicOptions.HasFlag(MusicOptions.RepeatAll))
            {
                if (queueEmpty)
                    return LavalinkUtilities.DecodeTrack(music.CurrentTrack.TrackString);

                var nextEntry = music.QueueTracks[0];
                music.QueueTracks.Remove(nextEntry);
                music.QueueTracks.Add(new QueueTrack(music.CurrentTrack, music.QueueTracks.Count));
                var newOrder = ReorderQueue(music.QueueTracks);
                music.QueueTracks.AddRange(newOrder);
                music.CurrentTrack = new(nextEntry);
            }

            else if (music.MusicOptions.HasFlag(MusicOptions.Shuffle))
            {
                if (queueEmpty)
                    return default;
                var rng = new Random();
                var randomEntry = music.QueueTracks[rng.Next(0, music.QueueTracks.Count)];
                music.QueueTracks.Remove(randomEntry);
                var newOrder = ReorderQueue(music.QueueTracks);
                music.QueueTracks.AddRange(newOrder);
                music.CurrentTrack = new(randomEntry);
            }

            //No Flag
            else
            {
                if (queueEmpty)
                    return default;
                var nextEntry = music.QueueTracks[0];
                music.QueueTracks.Remove(nextEntry);
                var newOrder = ReorderQueue(music.QueueTracks);
                music.QueueTracks.AddRange(newOrder);
                music.CurrentTrack = new(nextEntry);
            }
            await context.SaveChangesAsync();
            return LavalinkUtilities.DecodeTrack(music.CurrentTrack.TrackString);
        }

        private static List<QueueTrack> ReorderQueue(List<QueueTrack> tracks)
        {
            var tempList = tracks.ToList();
            tracks.Clear();
            for (int i = 0; i < tempList.Count; i++)
            {
                tempList[i].Position = i;
            }
            return tempList;
        }

        /// <summary>
        /// Add a LavalinkTrack to the DB
        /// </summary>
        /// <param name="context">Miku DB context</param>
        /// <param name="guildId">Guild ID</param>
        /// <param name="userId">User ID why submitted</param>
        /// <param name="track">The Lavalink track</param>
        /// <param name="skipSave">Do not save after adding stuff</param>
        /// <returns>DB queue object</returns>
        public static async Task<QueueTrack> AddTrackToQueueAsync(this MikuContext context, ulong guildId, ulong userId , LavalinkTrack track)
        {
            var music = await context.GetMikuMusicAsync(guildId);
            var newEntry = new QueueTrack
            {
                Id = guildId,
                Position = music.QueueTracks.Count,
                MikuMusic = music,
                TrackString = track.TrackString,
                AddedAt = DateTimeOffset.UtcNow,
                AddedBy = userId
            };
            music.QueueTracks.Add(newEntry);
            await context.SaveChangesAsync();

            return newEntry;
        }

        /// <summary>
        /// Add multiple LavalinkTracks to the queue!
        /// </summary>
        /// <param name="context">MikuContext of the Guild</param>
        /// <param name="guildId">GuildID</param>
        /// <param name="userId">User ID of the peront hat added the Tracks</param>
        /// <param name="tracks">The LavalinkTracks</param>
        /// <returns>The list of added Tracks</returns>
        public static async Task<IEnumerable<QueueTrack>> AddTracksToQueueAsync(this MikuContext context, ulong guildId, ulong userId, params LavalinkTrack[] tracks)
        {
            var music = await context.GetMikuMusicAsync(guildId);
            var added = new List<QueueTrack>();
            foreach (var item in tracks)
            {
                var newEntry = new QueueTrack
                {
                    Id = guildId,
                    Position = music.QueueTracks.Count,
                    MikuMusic = music,
                    TrackString = item.TrackString,
                    AddedAt = DateTimeOffset.UtcNow,
                    AddedBy = userId
                };
                music.QueueTracks.Add(newEntry);
            }
            await context.SaveChangesAsync();

            return added;
        }
    }
}
