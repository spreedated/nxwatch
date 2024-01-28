using NxBrewWindowsServiceReporter.Logic;
using NxBrewWindowsServiceReporter.Models;
using System.Threading.Tasks;

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
            SendMail sm = new();
            sm.AddRange(RuntimeStorage.SendViaEmail);
            await sm.Send();

            RuntimeStorage.SendViaEmail.Clear();
        }
    }
}
