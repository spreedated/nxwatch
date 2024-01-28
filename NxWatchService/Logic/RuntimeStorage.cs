using neXn.Lib.ConfigurationHandler;
using NxBrewWindowsServiceReporter.Models;
using Scraper.Models;
using System.Collections.Generic;

namespace NxBrewWindowsServiceReporter.Logic
{
    public static class RuntimeStorage
    {
        internal static List<Step> StepList { get; } = [];
        internal static List<SwitchGame> GameHistoryList { get; set; }
        internal static List<SwitchGame> SendViaEmail { get; set; }
        internal static ConfigurationHandler<Configuration> ConfigurationHandler { get; set; }
    }
}