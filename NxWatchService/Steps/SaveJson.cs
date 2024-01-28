using Newtonsoft.Json;
using NxBrewWindowsServiceReporter.Logic;
using NxBrewWindowsServiceReporter.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace NxBrewWindowsServiceReporter.Steps
{
    internal class SaveJson : Step
    {
        public SaveJson() : base()
        {
            base.Id = 5;
            base.Name = "Save JSON File";
            base.IsActive = true;
        }

        public override async Task Processor()
        {
            string filepath = Path.Combine(Environment.CurrentDirectory, "history.json");

            using (FileStream fs = File.Open(filepath, FileMode.Truncate, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                using (StreamWriter w = new(fs))
                {
                    await w.WriteAsync(JsonConvert.SerializeObject(RuntimeStorage.GameHistoryList, Formatting.Indented));
                }
            }
        }
    }
}
