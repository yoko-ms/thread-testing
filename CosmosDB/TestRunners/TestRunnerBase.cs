// ----------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
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
        Task ITestRunner.Run(EventWaitHandle stopEventHandler)
        {
            summaryList = new List<TestRunSummary>();

            return Task.Run(() =>
            {
                int testCount = 1;
                while (!stopEventHandler.WaitOne(10))
                {
                    TestRunSummary summary = new TestRunSummary();
                    Stopwatch counter = Stopwatch.StartNew();
                    try
                    {
                        OnExecuteTest(testCount);
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
                    ++testCount;
                }
            });
        }

        public abstract void OnExecuteTest(int repeatCount);
    }
}
