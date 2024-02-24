using ContainerService.Logic;
using ContainerService.Models;
using Database;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ContainerService.Steps
{
    internal class SetupFilesAndIntegrity : Step
    {
        public SetupFilesAndIntegrity() : base()
        {
            base.Id = 1;
            base.Name = "Create and connect the bot";
            base.ContinueOnError = false;
            base.IsActive = true;
        }

        public override async Task Processor()
        {
            if (!IsDirectoryWritable(RuntimeStorage.ConfigurationHandler.RuntimeConfiguration.WorkingDir))
            {
                base.SetError(new AccessViolationException("WorkDir is not writable!"));
                return;
            }

            if (!File.Exists(RuntimeStorage.ConfigurationHandler.RuntimeConfiguration.DatabasePath))
            {
                using (IDatabaseConnection conn = new DatabaseConnection(RuntimeStorage.ConfigurationHandler.RuntimeConfiguration.DatabasePath))
                {
                    await conn.CreateBlankDatabase();
                }
            }

            if (!IsFileWritable(RuntimeStorage.ConfigurationHandler.RuntimeConfiguration.DatabasePath))
            {
                base.SetError(new AccessViolationException("games.db is not accessable!"));
            }
        }

        public static bool IsFileWritable(string filepath)
        {
            List<bool> gates = [];

            using (FileStream fs = File.Open(filepath, FileMode.Open))
            {
                gates.Add(fs.CanRead);
                gates.Add(fs.CanWrite);
            }

            return gates.TrueForAll(x => x);
        }

        public static bool IsDirectoryWritable(string dirPath)
        {
            try
            {
                File.Create(Path.Combine(dirPath, Path.GetRandomFileName()), 1, FileOptions.DeleteOnClose)?.Dispose();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
