using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Scraper.Models
{
    public record SwitchGame : IValidatableObject
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

        [JsonIgnore()]
        public bool IsInDB { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrEmpty(this.Name))
            {
                yield return new ValidationResult("Name is required.", [nameof(this.Name)]);
            }

            if (string.IsNullOrEmpty(this.Link))
            {
                yield return new ValidationResult("Link is required.", [nameof(this.Link)]);
            }

            if (this.Date == default)
            {
                yield return new ValidationResult("Date is required.", [nameof(this.Date)]);
            }

            if (this.NxDate == default)
            {
                yield return new ValidationResult("NxDate is required.", [nameof(this.NxDate)]);
            }
        }
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
