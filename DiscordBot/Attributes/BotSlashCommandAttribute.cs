using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DiscordBot.Attributes
{
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
