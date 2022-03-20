// ----------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TestThreading.CosmosDB.TestRunners
{
    /// <summary>
    /// Interface for test runner task.
    /// </summary>
    public interface ITestRunner
    {
        /// <summary>
        /// Execute the test.
        /// </summary>
        /// <param name="stopEventHandler">Event to signal the end of test.</param>
        /// <returns>A <see cref="Task"/> running the test.</returns>
        Task Run(EventWaitHandle stopEventHandler);

        /// <summary>
        /// Gets the list of test results.
        /// </summary>
        /// <returns></returns>
        List<TestRunSummary> Result();
    }
}
