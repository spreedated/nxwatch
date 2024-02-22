using ContainerService.Logic;
using ContainerService.Models;
using Scraper.Models;
using System.Linq;
using System.Threading.Tasks;

namespace ContainerService.Steps
{
    internal class SendToChannel : Step
    {
        public SendToChannel() : base()
        {
            base.Id = 2;
            base.Name = "Send Games to Channel";
            base.ContinueOnError = false;
            base.IsActive = true;
        }

        public override async Task Processor()
        {
            foreach (SwitchGame g in RuntimeStorage.GamesToProcess.OrderByDescending(x => x.NxDate))
            {
                await RuntimeStorage.DiscordBot.SendText($"New game found: {g.Name} - Posted at: {g.NxDate} - Found it on: {g.Date} -> Link: {g.Link}");
            }
        }
    }
}
