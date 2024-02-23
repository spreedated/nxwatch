using ContainerService.Models;
using neXn.Lib.DiscordBot;
using neXn.Lib.ConfigurationHandler;
using Scraper.Models;
using System;
using System.Collections.Generic;

namespace ContainerService.Logic
{
    internal static class RuntimeStorage
    {
        internal static DateTime StartTime { get; set; }
        internal static List<Step> StepList { get; } = [];
        internal static ConfigurationHandler<Configuration> ConfigurationHandler { get; set; }
        internal static Bot DiscordBot { get; set; }
        internal static List<SwitchGame> GamesToProcess { get; set; } = [];
        internal static bool IsBotActive { get; set; } = true;
    }
}
