// ----------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace TestThreading.CosmosDB.TestRunners
{
    class CosmosDbTestRunner
    {
        private long logCount;
        private Timer timer;
        private DateTime runStartTime;
        private List<TestRunStatus> runStatusList;

        public void Run()
        {
            // Start timer to gathers runner statistics.
            this.logCount = 0;
            this.runStatusList = new List<TestRunStatus>();
            this.runStartTime = DateTime.Now;
            this.timer = new Timer(this.TimerCheck, null, 10, 50);

            // TaskWaitQueue.Instance.Start();

            CosmosDbTestUtils.InitializeCosmosDbForTesting()
                .CreateContainerIfNotExists(CosmosDbConfiguration.CpimCosmosDatabase, CosmosDbConfiguration.TestContainerName, String.Empty)
                .CreateContainerIfNotExists(
                    CosmosDbConfiguration.CpimCosmosDatabase,
                    CosmosDbConfiguration.TenantMetadataContainerName,
                    CosmosDbConfiguration.TenantMetadataContainerPartitionKeyPath)
                .CreateContainerIfNotExists(
                    CosmosDbConfiguration.CpimCosmosDatabase,
                    CosmosDbConfiguration.PolicyContainerName,
                    CosmosDbConfiguration.PolicyContainerPartitionKeyPath)
                .CreateContainerIfNotExists(
                    CosmosDbConfiguration.CpimCosmosDatabase,
                    CosmosDbConfiguration.CredentialContainerName,
                    CosmosDbConfiguration.CredentialContainerPartitionKeyPath);

            List<ITestRunner> runners = new List<ITestRunner>();
            for (int i = 0; i < 10; i++)
            {
                runners.Add(new TestCredentialDocument());
                runners.Add(new TestPolicyDocument());
                runners.Add(new TestQueryDb());
                runners.Add(new TestSearchAndSelectProperties());
                runners.Add(new TestTenantMetadata());
            }

            List<Task> taskList = new List<Task>();
            foreach (ITestRunner runner in runners)
            {
                taskList.Add(runner.Run(CosmosDbTestUtils.TestStopEvent));
                Thread.Sleep(50);
            }

            // run test for 1 minutes
            for (int i = 0; i < 60; i++)
            {
                Thread.Sleep(1000);
            }

            // signals stop
            CosmosDbTestUtils.TestStopEvent.Set();
            Task.WaitAll(taskList.ToArray());

            this.timer.Dispose();
            this.SaveResultToFile();
        }

        private void SaveResultToFile()
        {
            string folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string today = DateTime.Now.ToString("yyyy-MM-dd");

            string file = $"{folder}\\cosmos-test-{today}.csv";
            if (File.Exists(file))
            {
                int index = 0;
                do
                {
                    file = $"{folder}\\cosmos-test-{today}-{index.ToString("00")}.csv";
                    index++;
                } while (File.Exists(file));
            }

            this.SaveResultToFile(file);
        }

        private void SaveResultToFile(string filePath)
        {
            using (StreamWriter outFile = new StreamWriter(filePath))
            {
                outFile.WriteLine("Timestamp,ThreadCount,PendingWorkItemCount,CompletedWorkItemCount,StartedCalls,CompletedCalls,CancelledCalls,TimedoutCalls,NumberOfRetries,NumberOfFailures");

                long lastTimestamp = 0;
                foreach (TestRunStatus status in this.runStatusList)
                {
                    if (status.StartedCalls == 0)
                    {
                        lastTimestamp = status.Timestamp;
                    }
                    else
                    {
                        status.Timestamp -= lastTimestamp;
                        outFile.WriteLine(status.ToString());
                    }
                }
            }
        }

        private void TimerCheck(object stateInfo)
        {
            long elapsedTime = (long)DateTime.Now.Subtract(this.runStartTime).TotalMilliseconds;
            TestRunStatus status = CosmosDbTestUtils.TestRunStatus.Clone();
            status.Timestamp = elapsedTime;
            this.runStatusList.Add(status);

            long elapsedCount = (elapsedTime / 1000);
            if (this.logCount < elapsedCount)
            {
                Console.WriteLine($"{DateTime.Now.ToString("h:mm:ss.fff")} {status.ToString()}");
                this.logCount = elapsedCount;
            }
        }
    }
}
