// This file is part of the ArmoniK project
// 
// Copyright (C) ANEO, 2021-2022.
//   W. Kirschenmann   <wkirschenmann@aneo.fr>
//   J. Gurhem         <jgurhem@aneo.fr>
//   D. Dubuc          <ddubuc@aneo.fr>
//   L. Ziane Khodja   <lzianekhodja@aneo.fr>
//   F. Lemaitre       <flemaitre@aneo.fr>
//   S. Djebbar        <sdjebbar@aneo.fr>
//   J. Fonseca        <jfonseca@aneo.fr>
//   D. Brasseur       <dbrasseur@aneo.fr>
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

using ArmoniK.Api.gRPC.V1;
using ArmoniK.DevelopmentKit.Client.Symphony;
using ArmoniK.DevelopmentKit.Common;
using ArmoniK.Samples.HtcMockSymphonyClient;
using ArmoniK.Samples.HtcMockSymphonyLike.Client;

using Google.Protobuf.WellKnownTypes;

using Htc.Mock.Core;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;

namespace Armonik.Samples.HtcMockSymphony.Client
{
  internal class Program
  {
    private static IConfiguration   _configuration;
    private static ILogger<Program> _logger;

    private static void Main(string[] args)
    {
      Console.WriteLine("Hello Armonik HtcMock SymphonyLike Sample !");


      var armonikWaitClient = Environment.GetEnvironmentVariable("ARMONIK_DEBUG_WAIT_CLIENT");
      if (!string.IsNullOrEmpty(armonikWaitClient))
      {
        var armonikDebugWaitClient = int.Parse(armonikWaitClient);

        if (armonikDebugWaitClient > 0)
        {
          Console.WriteLine($"Debug: Sleep {armonikDebugWaitClient} seconds");
          Thread.Sleep(armonikDebugWaitClient * 1000);
        }
      }

      var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                                              .AddJsonFile("appsettings.json",
                                                           true,
                                                           true)
                                              .AddEnvironmentVariables();

      _configuration = builder.Build();

      Log.Logger = new LoggerConfiguration().MinimumLevel.Override("Microsoft",
                                                                   LogEventLevel.Information)
                                            .Enrich.FromLogContext()
                                            .WriteTo.Console()
                                            .CreateLogger();


      var factory = new LoggerFactory(new[]
                                      {
                                        new SerilogLoggerProvider(new LoggerConfiguration().ReadFrom.Configuration(_configuration)
                                                                                           .CreateLogger()),
                                      });

      _logger = factory.CreateLogger<Program>();

      var client = new ArmonikSymphonyClient(_configuration,
                                             factory);

      //get environment variable
      var _ = Environment.GetEnvironmentVariable("ARMONIK_DEBUG_WAIT_TASK");

      _logger.LogInformation("Configure taskOptions");
      var taskOptions = new TaskOptions
                        {
                          MaxDuration = new Duration
                                        {
                                          Seconds = 300,
                                        },
                          MaxRetries           = 5,
                          Priority             = 1,
                          EngineType           = EngineType.Symphony.ToString(),
                          ApplicationVersion   = "2.0.0",
                          ApplicationName      = "ArmoniK.Samples.HtcMockSymphonyPackage",
                          ApplicationNamespace = "ArmoniK.Samples.HtcMockSymphony.Packages",
                        };

      var sessionService = client.CreateSession(taskOptions);

      _logger.LogInformation($"New session created : {sessionService}");

      var runConfiguration = new RunConfiguration(new TimeSpan(0,
                                                               0,
                                                               0,
                                                               0,
                                                               100),
                                                  100,
                                                  1,
                                                  1,
                                                  4);

      var htcClient = new HtcMockSymphonyClient(sessionService,
                                                factory.CreateLogger<Htc.Mock.Client>());

      _logger.LogInformation("Running Small HtcMock SymphonyLike test, 1 execution");
      ClientSeqExec(htcClient,
                    runConfiguration,
                    1);
    }

    /// <summary>
    ///   First test to run nRun times sequentially with the given runConfiguration
    /// </summary>
    /// <param name="client">Symphony Like Client</param>
    /// <param name="runConfiguration"> Configuration for the run</param>
    /// <param name="nRun"> Number of runs</param>
    private static void ClientSeqExec(HtcMockSymphonyClient client,
                                      RunConfiguration      runConfiguration,
                                      int                   nRun)
    {
      var sw = Stopwatch.StartNew();
      for (var i = 0; i < nRun; i++)
      {
        client.Start(runConfiguration);
      }

      var elapsedMilliseconds = sw.ElapsedMilliseconds;
      var stat = new SimpleStats
                 {
                   ElapsedTime = elapsedMilliseconds,
                   Test        = "SeqExec",
                   NRun        = nRun,
                 };
      Console.WriteLine("JSON Result : " + stat.ToJson());
    }
  }
}
