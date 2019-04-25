using Microsoft.AspNetCore.Builder;
using Microsoft.Azure.SignalR.PerfTest.AppServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Serilog;
using System;

namespace SignalRCoreAppServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            useLocalSignalR =
                Environment.GetEnvironmentVariable("useLocalSignalR") == null ||
                Environment.GetEnvironmentVariable("useLocalSignalR") == "" ||
                Environment.GetEnvironmentVariable("useLocalSignalR") == "false" ? false : true;

            Console.BackgroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine($"use local signalr: {useLocalSignalR}");
            Console.BackgroundColor = ConsoleColor.Black;
        }

        public IConfiguration Configuration { get; }
        private bool useLocalSignalR = false;

        private void ConfigureLog()
        {
            var extLogFile = Configuration["LogFile"];
            var logFile = String.IsNullOrEmpty(extLogFile) ? "app.log" : extLogFile;
            Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .WriteTo
                    .File(logFile,
                          shared : true, // allow other process to read the file during writing
                          rollingInterval: RollingInterval.Day)
                    .CreateLogger();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            //ConfigureLog();
            services.AddMvc();
            var connectionCount = Configuration.GetValue<int>("ConnectionNumber");
            Log.Information($"Connection count: {connectionCount}");
            if (useLocalSignalR)
            {
                services.AddSignalR().AddMessagePackProtocol();
            }   
            else
            {
                services.AddSignalR().AddMessagePackProtocol().AddAzureSignalR(option =>
                {
                    option.AccessTokenLifetime = TimeSpan.FromDays(1);
                    option.ConnectionCount = connectionCount;
                });
            }
            //services.Replace(ServiceDescriptor.Singleton(typeof(ILoggerFactory), typeof(CustomizeLoggerFactory)));
            /*
            services.AddLogging(logging =>
            {
                logging.AddConsole();
                logging.AddSerilog();
                logging.AddFilter("Microsoft.AspNetCore.SignalR", LogLevel.Debug);
                logging.AddFilter("Microsoft.AspNetCore.Http.Connections", LogLevel.Debug);
                logging.SetMinimumLevel(LogLevel.Debug);
            });
            */
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseMvc();
            app.UseFileServer();
            if (useLocalSignalR)
                app.UseSignalR(routes =>
                {
                    routes.MapHub<BenchHub>("/signalrbench");
                });
            else
                app.UseAzureSignalR(routes =>
                {
                    routes.MapHub<BenchHub>("/signalrbench");
                });

        }
    }
}
