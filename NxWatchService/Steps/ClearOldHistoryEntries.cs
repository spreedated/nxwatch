#pragma warning disable S6561

using NxBrewWindowsServiceReporter.Logic;
using NxBrewWindowsServiceReporter.Models;
using System;
using System.Threading.Tasks;

namespace NxBrewWindowsServiceReporter.Steps
{
    internal class ClearOldHistoryEntries : Step
    {
        public ClearOldHistoryEntries() : base()
        {
            base.Id = 2;
            base.Name = "Clear old history entries";
            base.IsActive = true;
        }

        public override async Task Processor()
        {
            await Task.Factory.StartNew(() =>
                RuntimeStorage.GameHistoryList.RemoveAll(x => (DateTime.Now - x.Date).TotalDays >= 90)
            );
        }
    }
}
