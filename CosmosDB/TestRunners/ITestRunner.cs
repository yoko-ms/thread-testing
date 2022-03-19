// ----------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------

using System.Collections.Generic;
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
        /// <param name="repeatCount">Number of execution count.</param>
        /// <returns>A <see cref="Task"/> running the test.</returns>
        Task Run(int repeatCount);

        /// <summary>
        /// Gets the list of test results.
        /// </summary>
        /// <returns></returns>
        List<TestRunSummary> Result();
    }
}
