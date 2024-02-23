using ContainerService.Logic;
using ContainerService.Models;
using Database;
using Scraper.Models;
using Serilog;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ContainerService.Steps
{
    internal class QueryNxBrew : Step
    {
        public QueryNxBrew() : base()
        {
            base.Id = 2;
            base.Name = "Query NxBrew Website";
            base.ContinueOnError = false;
            base.IsActive = true;
        }

        public override async Task Processor()
        {
            using (Scraper.Scraper scraper = new())
            {
                using (IDatabaseConnection conn = new DatabaseConnection(RuntimeStorage.ConfigurationHandler.RuntimeConfiguration.DatabasePath))
                {
                    IEnumerable<SwitchGame> games = await scraper.GetGamesFromPage();

                    if (!games.Any())
                    {
                        Log.Information("No new games found");
                        return;
                    }

                    foreach (SwitchGame s in (games).Except(await conn.ReadAllGames(), new SwitchGameComparer()))
                    {
                        Log.Information($" |> New game found \"{s.Name}\" - Posted at: {s.NxDate} - Found it on \"{s.Date}\" -> Link: \"{s.Link}\"");
                        RuntimeStorage.GamesToProcess.Add(s);
                    }
                }
            }
        }
    }
}
