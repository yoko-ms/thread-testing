// ----------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------

using System.Text;
using System.Threading;

namespace TestThreading.CosmosDB.TestRunners
{
    public class TestRunStatus
    {
        /// <summary>
        /// Current timestmap
        /// </summary>
        public long Timestamp;

        /// <summary>
        /// the number of thread pool threads that currently exist.
        /// </summary>
        public int ThreadCount;

        /// <summary>
        /// the number of work items that are currently queued to be processed.
        /// </summary>
        public long PendingWorkItemCount;

        /// <summary>
        /// the number of work items that have been processed so far.
        /// </summary>
        public long CompletedWorkItemCount;

        /// <summary>
        /// the number of Cosmos client API call started
        /// </summary>
        public int StartedCalls;

        /// <summary>
        /// the number of Cosmos client API call completed.
        /// </summary>
        public int CompletedCalls;

        /// <summary>
        /// the number of operation cancelled calls.
        /// </summary>
        public int CancelledCalls;

        /// <summary>
        /// the number of calls timed out.
        /// </summary>
        public int TimedoutCalls;

        /// <summary>
        /// the number of retried calls.
        /// </summary>
        public int NumberOfRetries;

        /// <summary>
        /// the number of failures.
        /// </summary>
        public int NumberOfFailures;

        /// <summary>
        /// Clone the current status information.
        /// </summary>
        /// <returns>A cloned <see cref="TestRunStatus"/> object.</returns>
        public TestRunStatus Clone()
        {
            TestRunStatus cloned = new TestRunStatus();
            cloned.Timestamp = this.Timestamp;

            cloned.ThreadCount = ThreadPool.ThreadCount;
            cloned.PendingWorkItemCount = ThreadPool.PendingWorkItemCount;
            cloned.CompletedWorkItemCount = ThreadPool.CompletedWorkItemCount;

            cloned.StartedCalls = this.StartedCalls;
            cloned.CompletedCalls = this.CompletedCalls;
            cloned.CancelledCalls = this.CancelledCalls;
            cloned.TimedoutCalls = this.TimedoutCalls;
            cloned.NumberOfRetries = this.NumberOfRetries;
            cloned.NumberOfFailures = this.NumberOfFailures;

            return cloned;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(this.Timestamp)
                .Append(",")
                .Append(this.ThreadCount)
                .Append(",")
                .Append(this.PendingWorkItemCount)
                .Append(",")
                .Append(this.CompletedWorkItemCount)
                .Append(",")
                .Append(this.StartedCalls)
                .Append(",")
                .Append(this.CompletedCalls)
                .Append(",")
                .Append(this.CancelledCalls)
                .Append(",")
                 .Append(this.TimedoutCalls)
                .Append(",")
                .Append(this.NumberOfRetries)
                .Append(",")
                .Append(this.NumberOfFailures);

            return sb.ToString();
        }
    }
}
