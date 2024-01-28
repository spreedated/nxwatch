using Newtonsoft.Json;
using NxBrewWindowsServiceReporter.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace NxBrewWindowsServiceReporter.Steps
{
    internal class Setup : Step
    {
        public Setup() : base()
        {
            base.Id = 0;
            base.Name = "Setup first run";
            base.ContinueOnError = false;
            base.IsActive = true;
        }

        public override async Task Processor()
        {
            string emailFile = Path.Combine(Environment.CurrentDirectory, "email.json");

            if (!File.Exists(emailFile))
            {
                using (FileStream fs = File.Create(emailFile))
                {
                    using (StreamWriter s = new(fs))
                    {
                        await s.WriteAsync(JsonConvert.SerializeObject(new MailCredentials()));
                    }
                }
            }
        }
    }
}
