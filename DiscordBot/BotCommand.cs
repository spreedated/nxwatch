using Discord.WebSocket;
using System;

namespace DiscordBot
{
    public class BotCommand
    {
        public string Name { get; init; }
        public string[] Triggers { get; init; }
        public Action<SocketMessage> Command { get; init; }

        #region Constructor
        public BotCommand(string name, string[] triggers, Action<SocketMessage> command)
        {
            this.Name = name;
            this.Triggers = triggers;
            this.Command = command;
        }
        #endregion
    }
}
