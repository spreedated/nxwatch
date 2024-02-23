#pragma warning disable S6605

using Discord;
using Discord.WebSocket;
using DiscordBot.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace DiscordBot
{
    public class Bot : IDisposable
    {
        private CancellationTokenSource cts;
        private IMessageChannel primaryChannel;
        private bool subscribedToGlobalCommands;

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

        public event EventHandler Connected;
        public event EventHandler ConnectedToPrimaryChannel;
        public event EventHandler FullyInitialized;
        public event EventHandler ConnectionError;

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

        public async Task SubscribeToGlobalCommands(Type staticClassWithCommands)
        {
            if (subscribedToGlobalCommands)
            {
                return;
            }

            foreach (MethodInfo m in staticClassWithCommands.GetMethods().Where(x => x.GetCustomAttributes<BotSlashCommandAttribute>().Any()))
            {
                BotSlashCommandAttribute attr = m.GetCustomAttribute<BotSlashCommandAttribute>();

                if (attr == null || !attr.IsValid())
                {
                    continue;
                }

                SlashCommandBuilder globalCommand = new();
                globalCommand.WithName(attr.Name).WithDescription(attr.Description);
                await this.Client.CreateGlobalApplicationCommandAsync(globalCommand.Build());
            }

            this.Client.SlashCommandExecuted += async (t) =>
            {
                foreach (MethodInfo m in staticClassWithCommands.GetMethods().Where(x => x.GetCustomAttributes<BotSlashCommandAttribute>().Any() && x.GetCustomAttribute<BotSlashCommandAttribute>().Triggers.Any(x => x.Equals(t.CommandName, StringComparison.InvariantCultureIgnoreCase))))
                {
                    await Task.Run(() => m.Invoke(null, [t]));
                }
            };

            this.subscribedToGlobalCommands = true;
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
