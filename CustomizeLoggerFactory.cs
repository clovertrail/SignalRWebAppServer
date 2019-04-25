using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Web;
using Serilog;
using System;

namespace Microsoft.Azure.SignalR.PerfTest.AppServer
{
    public class CustomizeLoggerFactory : ILoggerFactory
    {
        private ILoggerFactory _loggerFactory;
        private volatile bool _disposed;

        protected virtual bool CheckDisposed() => _disposed;

        public CustomizeLoggerFactory()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging(config =>
            {
                config.AddConsole();
                //config.AddSerilog();
                //config.AddFilter("Microsoft.AspNetCore.SignalR", LogLevel.Debug);
                //config.AddFilter("Microsoft.AspNetCore.Http.Connections", LogLevel.Debug);
                config.SetMinimumLevel(LogLevel.Information);
            });
            var serviceProvider = serviceCollection.BuildServiceProvider();
            _loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        }

        public void AddProvider(ILoggerProvider provider)
        {
            _loggerFactory.AddProvider(provider);
        }

        public Extensions.Logging.ILogger CreateLogger(string categoryName)
        {
            if (CheckDisposed())
            {
                throw new ObjectDisposedException(nameof(LoggerFactory));
            }
            return _loggerFactory.CreateLogger(categoryName);
        }

        public Extensions.Logging.ILogger CreateLogger<T>()
        {
            if (CheckDisposed())
            {
                throw new ObjectDisposedException(nameof(LoggerFactory));
            }
            return _loggerFactory.CreateLogger<T>();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
            }
        }
    }
}
