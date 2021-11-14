using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Lavalink;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.DependencyInjection;
using mN.DB;
using mN.DB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mN.D.Extensions
{
    public static class InteractionContextExtensions
    {
        /// <summary>
        /// Get info (might be bad to add stuff with this)
        /// </summary>
        /// <param name="interCtx">InteractionContext, make sure its in a Guild!</param>
        /// <returns>MikuMusic object of Guild</returns>
        public static Task<MikuMusic> GetMikuMusicAsync(this InteractionContext interCtx)
        {
            var dbCtx = interCtx.Services.GetService<MikuContext>();
            return dbCtx.GetMikuMusicAsync(interCtx.Guild.Id);
        }

        /// <summary>
        /// Add a song from either YouTube or SoundCloud to the DB
        /// </summary>
        /// <param name="interCtx">InteractionContext</param>
        /// <param name="query">Searchterm</param>
        /// <param name="searchType">Where to search</param>
        /// <returns>DB queue object of the added song</returns>
        /// <remarks>Return in "default" if there was an error</remarks>
        public static async Task<QueueTrack> AddSongsAsync(this InteractionContext interCtx, string query, LavalinkSearchType searchType)
        {
            var ll = interCtx.Client.GetLavalink();
            var interactivity = interCtx.Client.GetInteractivity();
            var db = interCtx.Services.GetService<MikuContext>();
            var songsResult = await ll.GetIdealNodeConnection().Rest.GetTracksAsync(query, searchType);
            if (songsResult.LoadResultType == LavalinkLoadResultType.NoMatches)
            {
                var dmsg = new DiscordWebhookBuilder();
                dmsg.WithContent("No results found :c");
                await interCtx.EditResponseAsync(dmsg);
            }
            else if (songsResult.LoadResultType == LavalinkLoadResultType.SearchResult)
            {
                var dmsg = new DiscordWebhookBuilder();
                dmsg.WithContent("Found results!");
                var options = new List<DiscordSelectComponentOption>();
                foreach (var item in songsResult.Tracks.Take(15))
                {
                    var title = item.Title.Length > 100 ? $"{item.Title.Substring(0, 97)}..." : item.Title;
                    var artist = item.Author.Length > 50? $"{item.Author.Substring(0, 47)}..." : item.Author;
                    var duration = item.Length.TotalHours != 0 ? item.Length.ToString(@"mm\:ss") : item.Length.ToString(@"hh\:mm\:ss");
                    options.Add(new($"{title}", item.Identifier, $"[{duration}] by {artist}"));
                }

                var selectMenu = new DiscordSelectComponent("addSongSearchSelect", "Select a result!", options);
                dmsg.AddComponents(selectMenu);
                var msg = await interCtx.EditResponseAsync(dmsg);

                var selectResult = await interactivity.WaitForSelectAsync(msg, interCtx.User, selectMenu.CustomId);
                await selectResult.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                if (selectResult.TimedOut || selectResult.Result.Values.Length != 1)
                    return default;

                var selection = songsResult.Tracks.First(x => x.Identifier == selectResult.Result.Values[0]);
                var dbResult = await db.AddTrackToQueueAsync(interCtx.Guild.Id, interCtx.User.Id, selection);
                return dbResult;
            }
            return default;
        }

        /// <summary>
        /// Adds one or multiple songs to the DB
        /// </summary>
        /// <param name="interCtx">InteractionContext</param>
        /// <param name="query">URL</param>
        /// <returns>List of added DB queue object</returns>
        /// <remarks>Returns an array with a single "default" if there was an error or cancel</remarks>
        public static async Task<IEnumerable<QueueTrack>> AddSongsAsync(this InteractionContext interCtx, Uri query)
        {
            var ll = interCtx.Client.GetLavalink();
            var interactivity = interCtx.Client.GetInteractivity();
            var db = interCtx.Services.GetService<MikuContext>();
            var songsResult = await ll.GetIdealNodeConnection().Rest.GetTracksAsync(query);
            if (songsResult.LoadResultType == LavalinkLoadResultType.LoadFailed)
            {
                var dmsg = new DiscordWebhookBuilder();
                dmsg.WithContent("Failed to load the song :c\n" +
                    "Exception:\n" +
                    $"{songsResult.Exception.Message}");
                await interCtx.EditResponseAsync(dmsg);
            }
            else if (songsResult.LoadResultType == LavalinkLoadResultType.TrackLoaded)
            {
                var selection = songsResult.Tracks.First();
                var dbResult = await db.AddTrackToQueueAsync(interCtx.Guild.Id, interCtx.User.Id, selection);
                return new[] { dbResult };
            }
            else if (songsResult.LoadResultType == LavalinkLoadResultType.PlaylistLoaded)
            {
                var dmsg = new DiscordWebhookBuilder();
                dmsg.WithContent("Its a playlist!");
                if (songsResult.PlaylistInfo.SelectedTrack != -1)
                    dmsg.AddComponents(new DiscordButtonComponent(ButtonStyle.Primary, "single", "The song"));

                dmsg.AddComponents(new DiscordButtonComponent(ButtonStyle.Secondary, "all", "The entire playlist"));
                dmsg.AddComponents(new DiscordButtonComponent(ButtonStyle.Primary, "cancel", "Cancel"));

                var msg = await interCtx.EditResponseAsync(dmsg);
                var buttonResult = await interactivity.WaitForButtonAsync(msg, interCtx.User);
                await buttonResult.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                if (buttonResult.TimedOut || buttonResult.Result.Id == "cancel")
                    return new QueueTrack[] { default };

                else if (buttonResult.Result.Id == "single")
                {
                    var selection = songsResult.Tracks.ElementAt(songsResult.PlaylistInfo.SelectedTrack);
                    var dbResult = await db.AddTrackToQueueAsync(interCtx.Guild.Id, interCtx.User.Id, selection);
                    return new[] { dbResult };
                }

                else if (buttonResult.Result.Id == "all")
                {
                    IEnumerable<QueueTrack> additions = await db.AddTracksToQueueAsync(interCtx.Guild.Id, interCtx.User.Id, songsResult.Tracks.ToArray());
                    await db.SaveChangesAsync();
                    return additions;
                }
            }
            return new QueueTrack[] { default };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="interCtx"></param>
        /// <returns></returns>
        public static async Task<LavalinkGuildConnection> GetLavalinkGuildConnectionAsync(this InteractionContext interCtx, DiscordChannel chn)
        {
            var db = interCtx.Services.GetService<MikuContext>();
            var lava = interCtx.Client.GetLavalink();

            var oldCon = lava.GetGuildConnection(chn.Guild);
            if (oldCon != null)
            {
                if(oldCon.Channel.Id == chn.Id)
                {
                    return oldCon;
                }
            }

            var node = lava.GetIdealNodeConnection();
            var meek = await db.GetMikuMusicAsync(interCtx.Guild.Id);

            try
            {
                var connection = await node.ConnectAsync(chn);
                meek.ConnectionState = ConnectionState.Connected;
                await db.SaveChangesAsync();
                return connection;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }
    }
}
