using DSharpPlus;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.EventArgs;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.ButtonCommands;
using DSharpPlus.SlashCommands.EventArgs;
using MeekMoe.Images;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using mN.D.Commands;
using mN.D.Extensions;
using mN.DB;
using mN.NekoAPI;
using System;
using System.Threading.Tasks;
using Weeb.net;

namespace mN.D
{
    public class Bot
    {
        private DiscordClient _client { get; set; }
        private SlashCommandsExtension _slash { get; set; }

        private LavalinkExtension _lavalink { get; set; }
        private WeebClient _weebClient { get; set; }

        public Bot(string token)
        {
            _client = new DiscordClient(new DiscordConfiguration
            {
                MinimumLogLevel = LogLevel.Debug,
                Token = token
            });
            _weebClient = new("Miku Discord bot", "idk lol");
            var sp = new ServiceCollection()
                .AddSingleton(new MeekMoeImagesClient())
                .AddSingleton(new NekoApiClient())
                .AddSingleton(_weebClient)
                .AddDbContext<MikuContext>(ServiceLifetime.Transient, ServiceLifetime.Transient)
                .BuildServiceProvider();
            _client.VoiceStateUpdated += StageInviteHandle;
            _client.UseInteractivity();
            _lavalink = _client.UseLavalink();
            _slash = _client.UseSlashCommands(new()
            {
                Services = sp
            });
            _slash.RegisterCommands<ImageCommands>(483279257431441410);
            _slash.RegisterCommands<ActionCommands>(483279257431441410);
            _slash.RegisterCommands<MusicCommands>(483279257431441410);
            _slash.RegisterCommands<TestCommands>(483279257431441410);
            _slash.EnableButtonCommands();
            _slash.SlashCommandErrored += SlashError;
            _slash.SlashCommandExecuted += SlashExec;
        }

        private async Task StageInviteHandle(DiscordClient sender, VoiceStateUpdateEventArgs e)
        {
            if (e.After.IsSuppressed)
            {
                await e.Channel.UpdateCurrentUserVoiceStateAsync(false);
            }
        }

        private Task SlashExec(SlashCommandsExtension sender, SlashCommandExecutedEventArgs e)
        {
            Console.WriteLine("Did exec:\n" + e.Context.CommandName);
            return Task.CompletedTask;
        }

        private Task SlashError(SlashCommandsExtension sender, SlashCommandErrorEventArgs e)
        {
            Console.WriteLine("Big slash oof:\n" + e.Exception);
            return Task.CompletedTask;
        }

        public async Task RunsAsync(string weebToken)
        {
            await this._weebClient.Authenticate(weebToken, Weeb.net.TokenType.Wolke);
            //var tags = await this._weebClient.GetTypesAsync();
            await this._client.ConnectAsync();
            var node = await this._lavalink.ConnectAsync(new());
            node.PlaybackFinished += PlaybackFinishedHandler;
        }

        private async Task PlaybackFinishedHandler(LavalinkGuildConnection sender, TrackFinishEventArgs e)
        {
            switch (e.Reason)
            {
                case TrackEndReason.Finished:
                    await sender.PlayNextSongAsync();
                    break;
                case TrackEndReason.LoadFailed:
                    break;
                case TrackEndReason.Stopped:
                    break;
                case TrackEndReason.Replaced:
                    break;
                case TrackEndReason.Cleanup:
                    break;
                default:
                    break;
            }
        }

        public Task DisconnectAsync()
            => this._client.DisconnectAsync();
    }
}
