using ContainerService.Logic;
using ContainerService.Models;
using Database;
using Serilog;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ContainerService.Steps
{
    internal class Setup : Step
    {
        public Setup() : base()
        {
            base.Id = 0;
            base.Name = "Create and connect the bot";
            base.ContinueOnError = false;
            base.IsActive = true;
        }

        public override async Task Processor()
        {
            if (!RuntimeStorage.IsBotActive)
            {
                base.SetError(new ArgumentException("Bot is set to offline"));
                return;
            }

            if (string.IsNullOrEmpty(RuntimeStorage.ConfigurationHandler.RuntimeConfiguration.BotToken))
            {
                base.SetError(new ArgumentException("No token set :("));
                return;
            }

            if (RuntimeStorage.DiscordBot != null && RuntimeStorage.DiscordBot.IsConnected)
            {
                return;
            }

            bool isError = false;

            RuntimeStorage.DiscordBot = new(RuntimeStorage.ConfigurationHandler.RuntimeConfiguration.BotToken, RuntimeStorage.ConfigurationHandler.RuntimeConfiguration.ChannelId);
            RuntimeStorage.DiscordBot.Connected += (o, e) => Log.Information("Bot connected :)");
            RuntimeStorage.DiscordBot.FullyInitialized += (o, e) => Log.Information("Bot fully initialized :)");
            RuntimeStorage.DiscordBot.ConnectionError += (o, e) => isError = true;
            RuntimeStorage.DiscordBot.CommandTriggered += (o, e) => Log.Information($"Triggered {e.Command} for user \"{e.Username}\"");

            await RuntimeStorage.DiscordBot.Connect();

            if (isError)
            {
                base.SetError(new ArgumentException("Could not connect to Discord. Please check your configuration and try again."));
            }

            if (!File.Exists(RuntimeStorage.ConfigurationHandler.RuntimeConfiguration.DatabasePath))
            {
                using (DatabaseConnection conn = new(RuntimeStorage.ConfigurationHandler.RuntimeConfiguration.DatabasePath))
                {
                    await conn.CreateBlankDatabase();
                }
            }
        }
    }
}
