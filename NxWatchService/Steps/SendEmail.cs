using NxBrewWindowsServiceReporter.Logic;
using NxBrewWindowsServiceReporter.Models;
using System.Threading.Tasks;
using Mailsend;
using System.IO;
using System;

namespace NxBrewWindowsServiceReporter.Steps
{
    internal class SendEmail : Step
    {
        public SendEmail() : base()
        {
            base.Id = 4;
            base.Name = "SendEmail";
            base.IsActive = true;
        }

        public override async Task Processor()
        {
            SendMail sm = new(Path.Combine(Environment.CurrentDirectory, "email.json"), "markus.wackermann@gmail.com");
            sm.AddRange(RuntimeStorage.SendViaEmail);
            await sm.Send();

            RuntimeStorage.SendViaEmail.Clear();
        }
    }
}
