using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;

namespace NxBrewWindowsServiceReporter.Models
{
    internal class Configuration
    {
        [JsonIgnore]
        public string RootDir { get; } = @"C:\neXnServices\" + Assembly.GetExecutingAssembly().GetName().Name;
        public string WorkingDir
        {
            get
            {
                return Path.Combine(this.RootDir, "work");
            }
        }

        [JsonIgnore]
        public string ArchiveDir
        {
            get
            {
                return Path.Combine(this.RootDir, "archive");
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
        public TimeSpan StartServiceTime { get; internal set; } = new TimeSpan(8, 0, 0);

        [JsonProperty("endservicetime")]
        public TimeSpan EndServiceTime { get; internal set; } = new TimeSpan(22, 0, 0);

        [JsonProperty("interval")]
        public TimeSpan TimeInterval { get; internal set; } = new TimeSpan(4, 0, 0);

        [JsonProperty("runday")]
        public int Runday { get; set; } = 32;
    }
}