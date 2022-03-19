// ----------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace TestThreading.CosmosDB.TestRunners
{
    public abstract class TestRunnerBase : ITestRunner
    {
        private List<TestRunSummary> summaryList;

        /// <inheritdoc/>
        public List<TestRunSummary> Result()
        {
            return this.summaryList;
        }

        /// <inheritdoc/>
        Task ITestRunner.Run(int repeatCount)
        {
            summaryList = new List<TestRunSummary>();

            return Task.Run(() =>
            {
                CosmosDbTestUtils.IncrementTaskRunner();

                for (int i = 0; i < repeatCount; i++)
                {
                    TestRunSummary summary = new TestRunSummary();
                    Stopwatch counter = Stopwatch.StartNew();
                    try
                    {
                        OnExecuteTest();
                        summary.Result = TestRunSummary.TestResult.SUCCEEDED;
                    }
                    catch (Exception ex)
                    {
                        summary.Exception = ex;
                        summary.Result = TestRunSummary.TestResult.FAILED;
                    }

                    counter.Stop();
                    summary.Duration = counter.ElapsedMilliseconds;
                    this.summaryList.Add(summary);

                    CosmosDbTestUtils.DelayTask();
                }

                CosmosDbTestUtils.DecrementTaskRunner();
            });
        }

        public abstract void OnExecuteTest();
    }
}
