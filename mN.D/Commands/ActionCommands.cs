using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System.Threading.Tasks;
using Weeb.net;

namespace mN.D.Commands
{
    public class ActionCommands : ApplicationCommandModule
    {
        private readonly WeebClient _weebClient;

        public ActionCommands(WeebClient weebClient)
        {
            this._weebClient = weebClient;
        }

        [SlashCommand("action", "Do something to someone")]
        public async Task DoActionAsync(InteractionContext ctx, 
            [Option("Type", "The action you wann do")] ActionChoice actionChoice, 
            [Option("Target","Person of your choice")] DiscordUser target)
        {
            var image = await _weebClient.GetRandomAsync(actionChoice.ToString().ToLower(), new[] { "" });
            string actionText = "";
            switch (actionChoice) 
            {
                case ActionChoice.Hug:
                    actionText = "hugged";
                    break;
                case ActionChoice.Kiss:
                    actionText = "kissed";
                    break;
                case ActionChoice.Lick:
                    actionText = "licked";
                    break;
                case ActionChoice.Pat:
                    actionText = "patted";
                    break;
                case ActionChoice.Poke:
                    actionText = "pokes";
                    break;
                case ActionChoice.Slap:
                    actionText = "slaps";
                    break;
            }
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"{ctx.User.Mention} {actionText} {target.Mention}" +
                $"\n" + image.Url));
        }

        public enum ActionChoice
        {
            [ChoiceName("Hug")]
            Hug,
            [ChoiceName("Kiss")]
            Kiss,
            [ChoiceName("Lick")]
            Lick,
            [ChoiceName("Pat")]
            Pat,
            [ChoiceName("Poke")]
            Poke,
            [ChoiceName("Slap")]
            Slap
        }
    }
}
