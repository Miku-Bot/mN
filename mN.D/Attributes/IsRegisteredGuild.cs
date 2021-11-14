using DSharpPlus.SlashCommands;
using Microsoft.EntityFrameworkCore;
using mN.DB;
using mN.DB.Models;
using System.Threading.Tasks;

namespace mN.D.Attributes
{
    public class IsRegisteredGuild : SlashCheckBaseAttribute
    {
        public override async Task<bool> ExecuteChecksAsync(InteractionContext ctx)
        {
            var db = (MikuContext)ctx.Services.GetService(typeof(MikuContext));
            if (await db.MikuGuilds.AnyAsync(x => x.Id == ctx.Guild.Id))
                return true;
            var newGuild = new MikuGuild(ctx.Guild.Id);
            db.MikuGuilds.Add(newGuild);
            await db.SaveChangesAsync();
            return true;
        }
    }
}
