// ----------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;

namespace TestThreading.CosmosDB
{
    /// <summary>
    /// Utility class for <see cref="ICosmosDBClient" /> interface.
    /// </summary>
    public static class CosmosDbClientUtils
    {
        /// <summary>
        /// Max items that will be held in memory for Cosmos DB query.
        /// </summary>
        public const int MaxBufferedItemCount = 10000;

        /// <summary>Runs the task on background thread synchronously.</summary>
        /// Note: We use async / await inside the lambda for Task.Run since these
        /// calls are I/O bound and we do not want to block the background thread
        /// that is spawned by Task.Run
        /// <typeparam name="T">Data type name</typeparam>
        /// <param name="asyncFunc">The asynchronous function to execute.</param>
        /// <param name="maxRequestWaitTime">The maximum cumulative time the client should wait for the request to finish.</param>
        /// <returns>The return of the async function</returns>
        public static T RunTaskOnBackgroundThreadSync<T>(Func<CancellationToken, Task<T>> asyncFunc, TimeSpan maxRequestWaitTime)
        {
            using (var cts = GetCancellationTokenSourceForDbOps(maxRequestWaitTime))
            {
                try
                {
                    var syncTask = Task.Run(async () => await asyncFunc.Invoke(cts.Token));
                    if (syncTask.Wait(maxRequestWaitTime))
                    {
                        return syncTask.Result;
                    }

                    // if we ran out of time, quit and throw operation canceled exception
                    throw new OperationCanceledException();
                }
                catch (Exception ex)
                {
                    CosmosDbStorageException.ProcessCosmosDbExceptionThrown(ex);
                }
                finally
                {
                    cts.Cancel();
                }
            }

            return default(T);
        }

        /// <summary>Runs the task on background thread synchronously.</summary>
        /// Note: We use async / await inside the lambda for Task.Run since these
        /// calls are I/O bound and we do not want to block the background thread
        /// that is spawned by Task.Run
        /// <param name="asyncFunc">The asynchronous function to execute.</param>
        /// <param name="maxRequestWaitTime">The maximum cumulative time the client should wait for the request to finish.</param>
        public static void RunTaskOnBackgroundThreadSync(Func<CancellationToken, Task> asyncFunc, TimeSpan maxRequestWaitTime)
        {
            using (var cts = GetCancellationTokenSourceForDbOps(maxRequestWaitTime))
            {
                try
                {
                    if (!Task.Run(async () => await asyncFunc.Invoke(cts.Token)).Wait(maxRequestWaitTime))
                    {
                        // if we ran out of time, quit and throw operation canceled exception
                        throw new OperationCanceledException();
                    }
                }
                catch (Exception ex)
                {
                    CosmosDbStorageException.ProcessCosmosDbExceptionThrown(ex);
                }
                finally
                {
                    cts.Cancel();
                }
            }
        }

        /// <summary>Gets the cancellation token source for doc db operations.</summary>
        /// <param name="maxRequestWaitTime">The maximum cumulative time the client should wait for the request to finish.</param>
        /// <returns>The Cancellation Token source object.</returns>
        public static CancellationTokenSource GetCancellationTokenSourceForDbOps(TimeSpan maxRequestWaitTime)
        {
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(maxRequestWaitTime);
            return cancellationTokenSource;
        }

        /// <summary>Runs the given doc db task asynchronously with async/await and timeout.</summary>
        /// <typeparam name="T">The return type of the async operation</typeparam>
        /// <param name="asyncFunc">The asynchronous function to execute.</param>
        /// <param name="maxRequestWaitTime">The maximum cumulative time the client should wait for the request to finish.</param>
        /// <returns>The return of the async function</returns>
        public static async Task<T> RunTaskAsync<T>(Func<CancellationToken, Task<T>> asyncFunc, TimeSpan maxRequestWaitTime)
        {
            using (var cts = GetCancellationTokenSourceForDbOps(maxRequestWaitTime))
            {
                CancellationToken ct = cts.Token;

                try
                {
                    Task<T> asyncTask = asyncFunc.Invoke(ct);
                    Task completedTask = await Task.WhenAny(asyncTask, Task.Delay(maxRequestWaitTime, ct));

                    // if our async task has completed before the timeout
                    if (completedTask == asyncTask)
                    {
                        // this will return immediately since we have already waited and the task has completed
                        return await asyncTask;
                    }

                    // if we ran out of time, quit and throw operation canceled exception
                    throw new OperationCanceledException();
                }
                catch (Exception ex)
                {
                    CosmosDbStorageException.ProcessCosmosDbExceptionThrown(ex);
                }
                finally
                {
                    // make sure we cancel the cancellation token so that we notify all waiting processes (task delay in this case) to stop
                    cts.Cancel();
                }
            }

            return default(T);
        }
    }
}
