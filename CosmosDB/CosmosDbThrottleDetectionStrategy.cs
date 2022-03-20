// ----------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------

using System;
using TestThreading.RetryPolicy;

namespace TestThreading.CosmosDB
{
    /// <summary>
    /// Cosmos DB request got throttled detection strategy.
    /// </summary>
    public class CosmosDbThrottleDetectionStrategy : ITransientErrorDetectionStrategy
    {
        /// <summary>
        /// returns whether an exception is a throttled exception. If it is, it can be retried.
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

            CosmosDbStorageException cosmosDbStorageException = exception as CosmosDbStorageException;

            if (cosmosDbStorageException != null && cosmosDbStorageException.StatusCode == (System.Net.HttpStatusCode)429)
            {
                shouldBeRetried = true;
            }

            return shouldBeRetried;
        }
    }
}
