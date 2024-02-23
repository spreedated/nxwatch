#pragma warning disable S6561

using Database;
using Discord.WebSocket;
using DiscordBot.Attributes;
using Scraper.Models;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContainerService.Logic
{
    public static class BotSlashCommands
    {
        [BotSlashCommand(["ping"], "ping", "Replies with pong!")]
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
}
