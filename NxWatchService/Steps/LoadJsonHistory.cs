using Newtonsoft.Json;
using NxBrewWindowsServiceReporter.Logic;
using NxBrewWindowsServiceReporter.Models;
using Scraper.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace NxBrewWindowsServiceReporter.Steps
{
    internal class LoadJsonHistory : Step
    {
        public LoadJsonHistory() : base()
        {
            base.Id = 1;
            base.Name = "Load JSON History";
            base.IsActive = true;
        }

        public override async Task Processor()
        {
            string filepath = Path.Combine(Environment.CurrentDirectory, "history.json");

            if (!File.Exists(filepath))
            {
                using (FileStream fs = File.Create(filepath))
                {
                    using (StreamWriter w = new(fs))
                    {
                        await w.WriteAsync("[]");
                    }
                }
            }

            using (FileStream fs = File.Open(filepath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (StreamReader r = new(fs))
                {
                    RuntimeStorage.GameHistoryList = JsonConvert.DeserializeObject<List<SwitchGame>>(await r.ReadToEndAsync());
                }
            }
        }
    }
}
