using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Scraper.Models
{
    public record SwitchGame
    {
        [JsonProperty("categories")]
        public string[] Categories { get; set; }

        [JsonProperty("date")]
        public DateTime Date { get; set; }

        [JsonProperty("link")]
        public string Link { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("nxdate")]
        public DateTime NxDate { get; set; }
    }

    public class SwitchGameComparer : EqualityComparer<SwitchGame>
    {
        public override bool Equals(SwitchGame x, SwitchGame y)
        {
            return x.Name == y.Name 
                && x.NxDate == y.NxDate
                && x.Link == y.Link;
        }

        public override int GetHashCode([DisallowNull] SwitchGame obj)
        {
            return (obj.Name + obj.Link + obj.NxDate.ToString()).GetHashCode();
        }
    }
}
