﻿using ContainerService.Models;
using DiscordBot;
using neXn.Lib.ConfigurationHandler;
using Scraper.Models;
using System.Collections.Generic;

namespace ContainerService.Logic
{
    internal static class RuntimeStorage
    {
        internal static List<Step> StepList { get; } = [];
        internal static ConfigurationHandler<Configuration> ConfigurationHandler { get; set; }
        internal static Bot DiscordBot { get; set; }
        internal static List<SwitchGame> GamesToProcess { get; set; } = [];
        internal static bool IsBotActive { get; set; } = true;
    }
}
