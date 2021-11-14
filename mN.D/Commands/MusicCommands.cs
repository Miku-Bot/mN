using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using DSharpPlus.SlashCommands;
using mN.D.Attributes;
using mN.D.Extensions;
using mN.DB;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace mN.D.Commands
{
    [SlashCommandGroup("music", "Music commands!")]
    [IsRegisteredGuild]
    public class MusicCommands : ApplicationCommandModule
    {
        private readonly MikuContext _mikuContext;

        public MusicCommands(MikuContext mikuContext)
        {
            this._mikuContext = mikuContext;
        }

        [SlashCommand("join", "Join your voice channel")]
        public async Task JoinAsync(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            var con = await ctx.GetLavalinkGuildConnectionAsync(ctx.Member.VoiceState?.Channel);
            if (con == null)
                return;

            var response = new DiscordWebhookBuilder();
            response.WithContent("Helo c:");
            await ctx.EditResponseAsync(response);
        }

        [SlashCommand("play", "Play a song")]
        public async Task PlayAsync(InteractionContext ctx, [Option("Query", "URL or searchterm")] string query)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            var con = await ctx.GetLavalinkGuildConnectionAsync(ctx.Member.VoiceState?.Channel);
            var mm = await ctx.GetMikuMusicAsync();
            var isUri = Uri.TryCreate(query, UriKind.RelativeOrAbsolute, out var theUri);
            if (isUri)
            {
                var tracks = await ctx.AddSongsAsync(theUri);
                if (mm.PlayState == DB.Models.PlayState.Stopped)
                    await con.PlayNextSongAsync();
            }
            else
            {
                //searchHere
            }
            var response = new DiscordWebhookBuilder();
            response.WithContent("Playing c:");
            await ctx.EditResponseAsync(response);
        }

        [SlashCommand("skip", "Skip the current song")]
        public async Task SkipAsync(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            var con = await ctx.GetLavalinkGuildConnectionAsync(ctx.Member.VoiceState?.Channel);
            await con.PlayNextSongAsync();
            var response = new DiscordWebhookBuilder();
            response.WithContent("Skipped c:");
            await ctx.EditResponseAsync(response);
        }

        [SlashCommand("leave", "Leaves your voice channel")]
        public async Task LeaveAsync(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            var con = await ctx.GetLavalinkGuildConnectionAsync(ctx.Member.VoiceState?.Channel);
            await con.DisconnectLavalinkGuildConnectionAsync();
            var response = new DiscordWebhookBuilder();
            response.WithContent("Bai c:");
            await ctx.EditResponseAsync(response);
        }
    }
}
