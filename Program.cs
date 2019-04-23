using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using System;

namespace SignalRCoreAppServer
{
    public class Program
    {
        private const string Port = "Port";
        private const int DefaultPort = 5050;

        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args).UseKestrel(KestrelConfig)
            .UseStartup<Startup>();

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
