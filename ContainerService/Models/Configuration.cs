using Newtonsoft.Json;
using System;
using System.IO;

namespace ContainerService.Models
{
    internal class Configuration
    {
        [JsonIgnore]
        public string RootDir { get; } = Path.Combine(Environment.CurrentDirectory);

        [JsonIgnore]
        public string WorkingDir
        {
            get
            {
                return Path.Combine(this.RootDir, "work");
            }
        }

        [JsonIgnore]
        public string ConfigDir
        {
            get
            {
                return Path.Combine(this.RootDir, "config");
            }
        }

        [JsonProperty("startservicetime")]
        public TimeSpan StartServiceTime { get; internal set; } = new TimeSpan(6, 0, 0);

        [JsonProperty("endservicetime")]
        public TimeSpan EndServiceTime { get; internal set; } = new TimeSpan(22, 0, 0);

        [JsonProperty("interval")]
        public TimeSpan TimeInterval { get; internal set; } = new TimeSpan(4, 0, 0);

        [JsonProperty("runday")]
        public int Runday { get; set; } = 32;

        [JsonProperty("spamThisChannel")]
        public ulong ChannelId { get; set; }

        [JsonProperty("botToken")]
        public string BotToken { get; set; }

        [JsonIgnore]
        public string DatabasePath
        {
            get
            {
                return Path.Combine(this.WorkingDir, "games.db");
            }
        }
    }
}