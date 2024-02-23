#pragma warning disable S6605

using Discord;
using Discord.WebSocket;
using DiscordBot.EventArgs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DiscordBot
{
    public class Bot : IDisposable
    {
        private CancellationTokenSource cts;
        private IMessageChannel primaryChannel;

        public DiscordSocketClient Client { get; private set; }
        public string Token { get; init; }
        public bool IsReady { get; private set; }
        public bool IsConnected
        {
            get
            {
                return this.Client?.ConnectionState == ConnectionState.Connected;
            }
        }
        public ulong PrimaryChannelId { get; init; }
        public int ConnectionRetries { get; private set; } = 10;
        public List<BotCommand> Commands { get; } = [];

        public event EventHandler Connected;
        public event EventHandler ConnectedToPrimaryChannel;
        public event EventHandler FullyInitialized;
        public event EventHandler ConnectionError;
        public event EventHandler<CommandTriggeredEventArgs> CommandTriggered;

        #region Constructor
        public Bot(string token, ulong primaryChannelId)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentException("Invalid token", nameof(token));
            }

            if (primaryChannelId == default)
            {
                throw new ArgumentException("Invalid channelId", nameof(primaryChannelId));
            }

            this.Token = token;
            this.PrimaryChannelId = primaryChannelId;
        }
        #endregion

        private static DiscordSocketConfig GetDefaultConfig()
        {
            return new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
            };
        }

        private Task OnReady()
        {
            while (this.Client.ConnectionState != ConnectionState.Connected)
            {
                Task.Delay(100);
            }

            this.Connected?.Invoke(this, System.EventArgs.Empty);

            this.primaryChannel = (IMessageChannel)this.Client.GetChannel(this.PrimaryChannelId);

            if (this.primaryChannel == null)
            {
                throw new InvalidOperationException("Cannot get primary channel");
            }

            this.ConnectedToPrimaryChannel?.Invoke(this, System.EventArgs.Empty);

            this.IsReady = true;
            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(SocketMessage message)
        {
            if (message.Channel.Id != this.PrimaryChannelId || !this.Commands.Exists(x => x.Triggers.Any(x => x.Contains(message.Content.Split()[0], StringComparison.InvariantCultureIgnoreCase))))
            {
                return;
            }

            List<Task> tasks = [];

            foreach (BotCommand cmd in this.Commands.Where(x => x.Triggers.Any(x => x.Contains(message.Content.Split()[0], StringComparison.InvariantCultureIgnoreCase))))
            {
                tasks.Add(Task.Run(() => { cmd.Command.Invoke(message); }));

                this.CommandTriggered?.Invoke(this, new(cmd.Name, message.Author.Username));
            }

            await Task.WhenAll(tasks);
        }

        public async Task Connect()
        {
            if (this.Client != null || this.IsConnected || this.IsReady)
            {
                return;
            }

            this.cts = new();
            this.Client = new(GetDefaultConfig());
            this.Client.Ready += this.OnReady;

            await this.Client.LoginAsync(TokenType.Bot, this.Token);
            await this.Client.StartAsync();

            int retries = this.ConnectionRetries;

            await Task.Run(() =>
            {
                while (!this.IsReady)
                {
                    if (this.cts.IsCancellationRequested)
                    {
                        this.ConnectionError?.Invoke(this, System.EventArgs.Empty);
                        return;
                    }

                    Thread.Sleep(1000);
                    retries--;

                    if (retries <= 0)
                    {
                        this.cts.Cancel();
                    }
                }
            }, this.cts.Token);

            this.cts?.Dispose();

            this.Client.MessageReceived += this.MessageReceivedAsync;

            this.FullyInitialized?.Invoke(this, System.EventArgs.Empty);
        }

        public async Task Disconnect()
        {
            if (this.Client == null)
            {
                return;
            }

            await this.Client.StopAsync();
            this.IsReady = false;
        }

        public async Task SendFile(Stream stream, string filename, string text)
        {
            if (this.primaryChannel == null || string.IsNullOrEmpty(filename) || string.IsNullOrEmpty(text))
            {
                return;
            }

            await this.primaryChannel.SendFileAsync(stream, filename, text);
        }

        public async Task SendText(string text)
        {
            if (this.primaryChannel == null || string.IsNullOrEmpty(text))
            {
                return;
            }

            await this.primaryChannel.SendMessageAsync(text, allowedMentions: AllowedMentions.All);
        }

        #region Dispose
        protected virtual void Dispose(bool disposing)
        {
            this.IsReady = false;
            this.Client?.Dispose();
        }

        public void Dispose()
        {
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
