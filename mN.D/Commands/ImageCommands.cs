using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using MeekMoe.Images;
using mN.NekoAPI;
using System.Threading.Tasks;

namespace mN.D.Commands
{
    [SlashCommandGroup("images", "Image commands")]
    public class ImageCommands : ApplicationCommandModule
    {
        private readonly MeekMoeImagesClient _meekMoeImages;
        private readonly NekoApiClient _nekoApiClient;

        public ImageCommands(MeekMoeImagesClient meekMoeImages,
            NekoApiClient nekoApiClient)
        {
            this._meekMoeImages = meekMoeImages;
            this._nekoApiClient = nekoApiClient;
        }

        [SlashCommand("voca", "Get a random Vocaloid (and older 'loid) images!")]
        public async Task MeekMoeAsync(InteractionContext ctx, [Option("loid", "Random Project Diva (loading screen) image")]MeekMoeChoice choice = MeekMoeChoice.ProjectDiva)
        {
            var img = await _meekMoeImages.GetMoeApiImageAsync((Endpoint)choice);
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent(img.Url));
        }

        [SlashCommand("neko", "Get a random neko image")]
        public async Task NekoImageAsync(InteractionContext ctx)
        {
            var img = await _nekoApiClient.GetNekoApiImageAsync(ImageType.Neko);
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent(img.Message));
        }

        public enum MeekMoeChoice
        {
            [ChoiceName("Project Diva")]
            ProjectDiva,
            [ChoiceName("Kagamine Rin")]
            KagamineRin,
            [ChoiceName("Otomachi Una")]
            OtomachiUna,
            [ChoiceName("GUMI")]
            Gumi,
            [ChoiceName("Megurine Luka")]
            MegurineLuka,
            [ChoiceName("IA")]
            IA,
            [ChoiceName("Fukase")]
            Fukase,
            [ChoiceName("Hatsune Miku")]
            HatsuneMiku,
            [ChoiceName("Kagamine Len")]
            KagamineLen,
            [ChoiceName("KAITO")]
            Kaito,
            [ChoiceName("Kasane Teto")]
            KasaneTeto,
            [ChoiceName("MEIKO")]
            Meiko,
            [ChoiceName("Yuzuki Yukari")]
            YuzukiYukari,
            [ChoiceName("SFA2 Miki")]
            SFA2Miki,
            [ChoiceName("Lily")]
            Lily,
            [ChoiceName("Mayu")]
            Mayu,
            [ChoiceName("Lapis Aoki")]
            AokiLapis,
            [ChoiceName("Zola")]
            Zola
        }
    }
}
