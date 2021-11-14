using DSharpPlus;
using DSharpPlus.Lavalink;
using DSharpPlus.SlashCommands;
using mN.D.Attributes;
using mN.D.Extensions;
using mN.DB;
using System.Threading.Tasks;

namespace mN.D.Commands
{
    [IsRegisteredGuild]
    public class TestCommands : ApplicationCommandModule
    {
        private readonly MikuContext _mikuContext;

        public TestCommands(MikuContext mikuContext)
        {
            this._mikuContext = mikuContext;
        }

        [SlashCommand("test", "testing :)")]
        public async Task TestAsync(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            var song = await ctx.AddSongsAsync("never gona give you up", LavalinkSearchType.Youtube);
            var stuff = await ctx.GetMikuMusicAsync();
        }
    }
}
