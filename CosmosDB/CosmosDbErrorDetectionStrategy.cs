// ----------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using TestThreading.CosmosDB.TestRunners;
using TestThreading.RetryPolicy;

namespace TestThreading.CosmosDB
{
    /// <summary>
    /// Cosmos DB error detection strategy.
    /// </summary>
    public class CosmosDbErrorDetectionStrategy : ITransientErrorDetectionStrategy
    {
        /// <summary>
        /// List of messages that should be retried on
        /// </summary>
        private static readonly List<string> RetriableMessages = new List<string> { "One of the specified inputs is invalid" };

        /// <summary>
        /// returns whether an exception is a transient exception. If it is, it can be retried.
        /// </summary>
        /// <param name="caughtException">The caught exception</param>
        /// <returns>If the exception can be retried</returns>
        public bool IsTransient(Exception caughtException)
        {
            bool shouldBeRetried = false;
            Exception exception = caughtException;
            AggregateException aggregateException = caughtException as AggregateException;

            while (aggregateException?.InnerException != null)
            {
                exception = aggregateException.InnerException;
                aggregateException = exception as AggregateException;
            }

            if (exception is TaskCanceledException || exception is OperationCanceledException)
            {
                shouldBeRetried = true;
            }

            CosmosDbStorageException documentDbStorageException = exception as CosmosDbStorageException;

            if (documentDbStorageException != null && documentDbStorageException.StatusCode == HttpStatusCode.ServiceUnavailable)
            {
                shouldBeRetried = true;
            }

            Exception innerException = documentDbStorageException?.InnerException;

            if ((innerException is TaskCanceledException) || (innerException is OperationCanceledException) || (innerException is HttpRequestException))
            {
                shouldBeRetried = true;
            }

            if (!shouldBeRetried)
            {
                shouldBeRetried = RetriableMessages.Any(r => exception.Message.Contains(r));
            }

            return shouldBeRetried;
        }
    }
}
