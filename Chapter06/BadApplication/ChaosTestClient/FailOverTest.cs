﻿using System;
using System.Fabric;
using System.Fabric.Testability.Scenario;
using System.Threading;
using System.Threading.Tasks;


namespace ChaosTestClient
{
    class FailOverTest
    {
        static int Main(string[] args)
        {
            string clusterConnection = "localhost:19000";
            Uri serviceName = new Uri("fabric:/BadApplication/BadStateful");
            Console.WriteLine("Starting Chaos Test Scenario...");
            try
            {
                RunFailoverTestScenarioAsync(clusterConnection, serviceName).Wait();
            }
            catch (AggregateException ae)
            {
                Console.WriteLine("Chaos Test Scenario did not complete: ");
                foreach (Exception ex in ae.InnerExceptions)
                {
                    if (ex is FabricException)
                    {
                        Console.WriteLine("HResult: {0} Message: {1}", ex.HResult, ex.Message);
                    }
                }
                return -1;
            }
            Console.WriteLine("Chaos Test Scenario completed.");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
            return 0;
        }

        static async Task RunFailoverTestScenarioAsync(string clusterConnection, Uri serviceName)
        {
            TimeSpan maxClusterStabilizationTimeout = TimeSpan.FromSeconds(180);
            PartitionSelector randomPartitionSelector = PartitionSelector.RandomOf(serviceName);
            
            FabricClient fabricClient = new FabricClient(clusterConnection);

            TimeSpan timeToRun = TimeSpan.FromMinutes(60);
            FailoverTestScenarioParameters scenarioParameters = new FailoverTestScenarioParameters(
                randomPartitionSelector,
                timeToRun,
                maxClusterStabilizationTimeout);
            
            FailoverTestScenario failoverScenario = new FailoverTestScenario(fabricClient, scenarioParameters);

            try
            {
                await failoverScenario.ExecuteAsync(CancellationToken.None);


            }
            catch (AggregateException ae)
            {
                throw ae.InnerException;
            }
        }
    }
}
