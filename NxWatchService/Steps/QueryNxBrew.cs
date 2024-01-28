using NxBrewWindowsServiceReporter.Logic;
using NxBrewWindowsServiceReporter.Models;
using Scraper.Models;
using Serilog;
using System.Linq;
using System.Threading.Tasks;

namespace NxBrewWindowsServiceReporter.Steps
{
    internal class QueryNxBrew : Step
    {
        public QueryNxBrew() : base()
        {
            base.Id = 3;
            base.Name = "Query NxBrew";
            base.IsActive = true;
        }

        public override async Task Processor()
        {
            RuntimeStorage.SendViaEmail ??= [];

            using (Scraper.Scraper scraper = new())
            {
                foreach (SwitchGame s in (await scraper.GetGamesFromPage()).Except(RuntimeStorage.GameHistoryList, new SwitchGameComparer()))
                {
                    Log.Information($" |> New game found \"{s.Name}\" - Posted at: {s.NxDate} - Found it on \"{s.Date}\" -> Link: \"{s.Link}\"");
                    RuntimeStorage.SendViaEmail.Add(s);
                    RuntimeStorage.GameHistoryList.Add(s);
                }
            }
        }
    }
}
