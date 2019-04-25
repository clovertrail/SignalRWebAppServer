using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using NLog.Web;
using Microsoft.Extensions.Logging;
using System;

namespace SignalRCoreAppServer
{
    public class Program
    {
        private const string Port = "Port";
        private const int DefaultPort = 5050;

        public static void Main(string[] args)
        {
            var logger = NLog.Web.NLogBuilder.ConfigureNLog("NLog.config").GetCurrentClassLogger();
            try
            {
                logger.Debug("init main");
                CreateWebHostBuilder(args).Build().Run();
            }
            catch (Exception exception)
            {
                //NLog: catch setup errors
                logger.Error(exception, "Stopped program because of exception");
                throw;
            }
            finally
            {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                NLog.LogManager.Shutdown();
            }
        }

        private static void ConfigureNLog(ILoggingBuilder logging)
        {
            logging.ClearProviders();
            logging.SetMinimumLevel(LogLevel.Information);
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args).UseKestrel(KestrelConfig)
            .UseStartup<Startup>()
            .ConfigureLogging(logging => ConfigureNLog(logging))
            .UseNLog();

        public static readonly Action<WebHostBuilderContext, KestrelServerOptions> KestrelConfig =
            (context, options) =>
            {
                var config = context.Configuration;
                // After we apply Nginx, all of requests will map to the same port.
                if (!int.TryParse(config[Port], out var port))
                {
                    port = DefaultPort;
                }
                options.ListenAnyIP(port);
            };
    }
}
