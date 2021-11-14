using System;
using System.Threading.Tasks;

namespace mN.Test
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            //var db = new MikuContext();
            //db.Add(new MikuGuild(483279257431441410));
            //await db.SaveChangesAsync();
            //var theGuild = await db.MikuGuilds.Include(x => x.MikuMusic).ThenInclude(x => x.QueueTracks).Include(x => x.MikuMusic).ThenInclude/(x /=> x.CurrentTrack).FirstOrDefaultAsync(x => x.Id == 483279257431441410);
            //Console.WriteLine(theGuild.Id);

            var time = 483279257431441410 >> 22;
            var ts = new DateTimeOffset(2015, 01, 01, 0, 0, 0, TimeSpan.FromSeconds(0));
            var hm = ts + TimeSpan.FromMilliseconds(time);
            await Task.Delay(1);
        }
    }
}
