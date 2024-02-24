using ContainerService.Logic;
using ContainerService.Models;
using Database;
using System.Threading.Tasks;

namespace ContainerService.Steps
{
    internal class SaveToDatabase : Step
    {
        public SaveToDatabase() : base()
        {
            base.Id = 4;
            base.Name = "Save processed games to Database";
            base.ContinueOnError = false;
            base.IsActive = true;
        }

        public override async Task Processor()
        {
            if (RuntimeStorage.GamesToProcess.Count <= 0)
            {
                return;
            }

            using (IDatabaseConnection conn = new DatabaseConnection(RuntimeStorage.ConfigurationHandler.RuntimeConfiguration.DatabasePath))
            {
                await conn.SaveGames(RuntimeStorage.GamesToProcess);
            }

            RuntimeStorage.GamesToProcess.Clear();
        }
    }
}
