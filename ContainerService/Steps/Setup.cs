#pragma warning disable S2583
#pragma warning disable S6561
#pragma warning disable S6605

using ContainerService.Logic;
using ContainerService.Models;
using Database;
using Discord;
using Discord.WebSocket;
using Scraper.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
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

            await AddBotCommands();
        }

        private static async Task AddBotCommands()
        {
            foreach (MethodInfo m in typeof(BotSlashCommands).GetMethods().Where(x => x.GetCustomAttributes<BotSlashCommandAttribute>().Any()))
            {
                BotSlashCommandAttribute attr = m.GetCustomAttribute<BotSlashCommandAttribute>();

                if (attr == null || !attr.IsValid())
                {
                    continue;
                }

                SlashCommandBuilder globalCommand = new();
                globalCommand.WithName(attr.Name).WithDescription(attr.Description);
                await RuntimeStorage.DiscordBot.Client.CreateGlobalApplicationCommandAsync(globalCommand.Build());
                
                Log.Information($"Added command \"{attr.Name}\" as global command");
            }

            RuntimeStorage.DiscordBot.Client.SlashCommandExecuted += async (t) =>
            {
                foreach(MethodInfo m in typeof(BotSlashCommands).GetMethods().Where(x => x.GetCustomAttributes<BotSlashCommandAttribute>().Any() && x.GetCustomAttribute<BotSlashCommandAttribute>().Triggers.Any(x => x.Equals(t.CommandName, StringComparison.InvariantCultureIgnoreCase))))
                {
                    await Task.Run(() => m.Invoke(null, [t]));
                }
            };
        }

        public static class BotSlashCommands
        {
            [BotSlashCommand(["ping"],"ping","Replies with pong!")]
            public static async Task Ping(SocketSlashCommand cmd)
            {
                await cmd.RespondAsync("Pong!");
            }

            [BotSlashCommand(["list"], "list", "List all games in the database")]
            public static async Task List(SocketSlashCommand cmd)
            {
                StringBuilder s = new();
                using (IDatabaseConnection conn = new DatabaseConnection(RuntimeStorage.ConfigurationHandler.RuntimeConfiguration.DatabasePath))
                {
                    foreach (SwitchGame g in (await conn.ReadAllGames()).Reverse())
                    {
                        s.Append($"Game:\t**{g.Name}**\n\n");
                    }

                    if (s.Length >= 1900)
                    {
                        s.Remove(1900, s.Length - 1900);
                    }

                    s.Append($"\n\n\t---\nFound {await conn.GetGamesCount()} games in the database");
                }

                await cmd.RespondAsync(s.ToString()?.TrimEnd('\n'));
            }

            [BotSlashCommand(["count"], "count", "Number of games listed in Database")]
            public static async Task Count(SocketSlashCommand cmd)
            {
                using (IDatabaseConnection conn = new DatabaseConnection(RuntimeStorage.ConfigurationHandler.RuntimeConfiguration.DatabasePath))
                {
                    await cmd.RespondAsync($"{await conn.GetGamesCount()} games in the database");
                }
            }

            [BotSlashCommand(["uptime"], "uptime", "Shows the bots uptime")]
            public static async Task Uptime(SocketSlashCommand cmd)
            {
                TimeSpan uptime = DateTime.Now - RuntimeStorage.StartTime;
                await cmd.RespondAsync($"Running for {uptime.Days} days and {uptime.Hours}:{uptime.Minutes}:{uptime.Seconds}");
            }

            [BotSlashCommand(["latest"], "latest", "Shows the latest game")]
            public static async Task Latest(SocketSlashCommand cmd)
            {
                using (IDatabaseConnection conn = new DatabaseConnection(RuntimeStorage.ConfigurationHandler.RuntimeConfiguration.DatabasePath))
                {
                    SwitchGame latest = await conn.GetLatest();
                    await cmd.RespondAsync($"Name: {latest.Name}\n{latest.Link}");
                }
            }
        }

        [AttributeUsage(AttributeTargets.Method)]
        public class BotSlashCommandAttribute : Attribute, IValidatableObject
        {
            public string Name { get; init; }
            public string Description { get; init; }
            public string[] Triggers { get; init; }
            public BotSlashCommandAttribute(string[] triggers, string name, string desc)
            {
                this.Triggers = triggers;
                this.Name = name;
                this.Description = desc;
            }

            public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            {
                if (string.IsNullOrEmpty(this.Name))
                {
                    yield return new ValidationResult("Name cannot be null or empty");
                }

                if (string.IsNullOrEmpty(this.Description))
                {
                    yield return new ValidationResult("Description cannot be null or empty");
                }

                if (this.Triggers == null || this.Triggers.Length <= 0)
                {
                    yield return new ValidationResult("No Triggers set");
                }
            }

            public bool IsValid()
            {
                return !this.Validate(new(this)).Any();
            }
        }
    }
}
