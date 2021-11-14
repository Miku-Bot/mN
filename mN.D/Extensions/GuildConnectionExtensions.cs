using DSharpPlus.Lavalink;
using Microsoft.EntityFrameworkCore;
using mN.DB;
using mN.DB.Models;
using mN.D.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.SlashCommands;

namespace mN.D.Extensions
{
    public static class GuildConnectionExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        public static async Task<bool> DisconnectLavalinkGuildConnectionAsync(this LavalinkGuildConnection connection)
        {
            //TODO: Dispose the context!
            var db = new MikuContext();
            var theGuild = await db.MikuGuilds.Include(x => x.MikuMusic)
                .ThenInclude(x => x.QueueTracks)
                .Include(x => x.MikuMusic)
                .ThenInclude(x => x.CurrentTrack)
                .FirstOrDefaultAsync(x => x.Id == connection.Guild.Id);
            try
            {
                await connection.DisconnectAsync();
                theGuild.MikuMusic.ConnectionState = ConnectionState.Disconnected;
                theGuild.MikuMusic.PlayState = 0;
                theGuild.MikuMusic.MusicOptions = 0;
                theGuild.MikuMusic.CurrentTrack = null;
                theGuild.MikuMusic.QueueTracks.Clear();
                await db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        public static async Task<bool> PlayNextSongAsync(this LavalinkGuildConnection connection)
        {
            var db = new MikuContext();
            try
            {
                var next = await db.GetNextSongAsync(connection.Guild.Id);
                var theGuild = await db.MikuGuilds.Include(x => x.MikuMusic)
                    .ThenInclude(x => x.QueueTracks)
                    .Include(x => x.MikuMusic)
                    .ThenInclude(x => x.CurrentTrack)
                    .FirstOrDefaultAsync(x => x.Id == connection.Guild.Id);
                if (next != null)
                {
                    await connection.PlayAsync(next);
                    theGuild.MikuMusic.PlayState = PlayState.Playing;
                    await db.SaveChangesAsync();
                    return true;
                }
                else
                {
                    theGuild.MikuMusic.PlayState = PlayState.Stopped;
                    await db.SaveChangesAsync();
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
        }

        //Pause
        public static async Task<bool> PauseAsync(this LavalinkGuildConnection connection)
        {
            var db = new MikuContext();
            var mm = await db.GetMikuMusicAsync(connection.Guild.Id);
            mm.PlayState = PlayState.Paused;
            await connection.PauseAsync();
            await db.SaveChangesAsync();
            return true;
        }

        //Resume
        public static async Task<bool> ResumeAsync(this LavalinkGuildConnection connection)
        {
            var db = new MikuContext();
            var mm = await db.GetMikuMusicAsync(connection.Guild.Id);
            mm.PlayState = PlayState.Playing;
            await connection.ResumeAsync();
            await db.SaveChangesAsync();
            return true;
        }

        //Stop
        public static async Task<bool> StopAsync(this LavalinkGuildConnection connection)
        {
            var db = new MikuContext();
            var mm = await db.GetMikuMusicAsync(connection.Guild.Id);
            mm.PlayState = PlayState.Stopped;
            await connection.StopAsync();
            await db.SaveChangesAsync();
            return true;
        }
    }
}
