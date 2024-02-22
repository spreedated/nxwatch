namespace DiscordBot.EventArgs
{
    public class CommandTriggeredEventArgs : System.EventArgs
    {
        public string Command { get; init; }
        public string Username { get; init; }

        #region Constructor
        public CommandTriggeredEventArgs(string command, string username)
        {
            this.Command = command;
            this.Username = username;
        }
        #endregion
    }
}
