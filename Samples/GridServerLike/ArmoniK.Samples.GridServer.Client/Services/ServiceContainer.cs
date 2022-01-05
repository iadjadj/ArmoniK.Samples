using System;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;

namespace ArmoniK.Samples.GridServer.Client.Services
{
    public class ServiceContainer
    {
        private readonly IConfiguration configuration_;
        private readonly ILogger<ServiceContainer> logger_;

        public ServiceContainer()
        {
          var builder = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory())
                 .AddJsonFile("appsettings.json",
                              true,
                              true)
                 .AddEnvironmentVariables();


          var Configuration = builder.Build();

          Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft",
                    LogEventLevel.Information)
                .ReadFrom.Configuration(Configuration)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateBootstrapLogger();

            var logProvider = new SerilogLoggerProvider(Log.Logger);
            var factory = new LoggerFactory(new[] { logProvider });

            logger_ = factory.CreateLogger<ServiceContainer>();
        }

        public double ComputeSquare(double a)
        {
            logger_.LogInformation($"Enter in function : ComputeSquare");

            double res = a * a;

            return res;
        }

        public int ComputeCube(int a)
        {
            logger_.LogInformation($"Enter in function : ComputeCube");
            int value = a * a * a;

            return value;
        }

        public int ComputeDivideByZero(int a)
        {
            logger_.LogInformation($"Enter in function : ComputeDivideByZero");
            int value = a / 0;

            return value;
        }

        public double Add(double value1, double value2)
        {
            logger_.LogInformation($"Enter in function : Add");
            return value1 + value2;
        }

        public double AddGenerateException(double value1, double value2)
        {
            throw new NotImplementedException("Fake Method to generate an NotYetImplementedException");
        }
    }
}
