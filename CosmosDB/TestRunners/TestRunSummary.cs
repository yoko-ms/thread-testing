// ----------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------

using System;

namespace TestThreading.CosmosDB.TestRunners
{
    /// <summary>
    /// Summary of test result.
    /// </summary>
    public class TestRunSummary
    {
        public enum TestResult : int
        {
            FAILED,
            SUCCEEDED
        }

        /// <summary>
        /// Test duration in milli-seconds.
        /// </summary>
        public long Duration { get; set; }

        /// <summary>
        /// Test result (OK or Failed)
        /// </summary>
        public TestResult Result { get; set; }

        /// <summary>
        /// Exception inforamtion if occurs.
        /// </summary>
        public Exception Exception { get; set; }
    }
}
