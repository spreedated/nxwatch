using ByteSizeLib;
using ContainerService.Logic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Context;
using Serilog.Events;
using System;
using System.IO;
using System.Text;

namespace ContainerService
{
    internal static class Program
    {
        public static readonly string LogFilePath = Path.Combine(Environment.CurrentDirectory, "logs", "logfile.log");
#if DEBUG
        internal readonly static LogEventLevel level = LogEventLevel.Verbose;
#else
        internal readonly static LogEventLevel level = LogEventLevel.Information;
#endif

        public static void Main(string[] args)
        {
            RuntimeStorage.ConfigurationHandler = new(new(Path.Combine(Environment.CurrentDirectory, "config", "config.json")) { CreateOnNothing = true, OverrideOnInvalid = true });
            RuntimeStorage.ConfigurationHandler.Load();

            if (!Directory.Exists(RuntimeStorage.ConfigurationHandler.RuntimeConfiguration.WorkingDir))
            {
                Directory.CreateDirectory(RuntimeStorage.ConfigurationHandler.RuntimeConfiguration.WorkingDir);
            }

            HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
            //builder.Logging.ClearProviders();
            builder.Logging.AddSerilog();
            builder.Services.AddHostedService<Worker>();

            IHost host = builder.Build();
            host.Run();
        }

        public static void CreateLoggingObject()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(LogFilePath, encoding: Encoding.UTF8, rollOnFileSizeLimit: true, fileSizeLimitBytes: (long)ByteSize.FromMegaBytes(1.0d).Bytes, restrictedToMinimumLevel: level)
                .WriteTo.Console()
                .WriteTo.Debug(0)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("version", typeof(Worker).Assembly.GetName().Version)
                .CreateLogger();
#if DEBUG
            LogContext.PushProperty("isDebug", true);
#endif
        }
    }
}
