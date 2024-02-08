using ByteSizeLib;
using NxBrewWindowsServiceReporter.Logic;
using Serilog;
using Serilog.Context;
using Serilog.Events;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.ServiceProcess;
using System.Text;

namespace NxBrewWindowsServiceReporter
{
    internal class ServiceEntry : ServiceBase
    {
        public static readonly string LogFilePath = @$"C:\ServiceLogs\{Assembly.GetExecutingAssembly().GetName().Name}\logs\logfile.log";
#if DEBUG
        internal readonly static LogEventLevel level = LogEventLevel.Verbose;
#else
        internal readonly static LogEventLevel level = LogEventLevel.Information;
#endif

        public ServiceEntry()
        {
            base.ServiceName = Assembly.GetExecutingAssembly().GetName().Name;
            base.EventLog.Log = "Application";

            base.CanStop = true;
            base.CanShutdown = true;
            base.CanPauseAndContinue = true;
        }

        public static void CreateLoggingObject()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(LogFilePath, encoding: Encoding.UTF8, rollOnFileSizeLimit: true, fileSizeLimitBytes: (long)ByteSize.FromMegaBytes(1.0d).Bytes, restrictedToMinimumLevel: level)
                .WriteTo.Debug(0)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("version", typeof(ServiceEntry).Assembly.GetName().Version)
                .CreateLogger();
#if DEBUG
            LogContext.PushProperty("isDebug", true);
#endif
        }

        protected override void OnStart(string[] args)
        {
            base.OnStart(args);

            CreateLoggingObject();
            RuntimeStorage.ConfigurationHandler = new(new(Path.Combine($"C:\\ServiceLogs\\{Assembly.GetExecutingAssembly().GetName().Name}\\config", "config.json")) { CreateOnNothing = true, OverrideOnInvalid = true });
            RuntimeStorage.ConfigurationHandler.Load();

            try
            {
                Engine.Initialize();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Error - Shutting down service... Bye");
                this.Stop();
            }
        }

        protected override void OnStop()
        {
            base.OnStop();
            Log.Information("Service stopped");
        }

        protected override void OnPause()
        {
            base.OnPause();
            Log.Information("Service paused");
        }

        public void StartDebugging()
        {
            this.OnStart(null);
        }
    }
}
