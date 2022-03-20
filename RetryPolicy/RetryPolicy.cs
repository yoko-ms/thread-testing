// ----------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using TestThreading.CosmosDB.TestRunners;

namespace TestThreading.RetryPolicy
{
    /// <summary>
    /// Extends the base <see cref = "RetryPolicy" /> implementation with strategy objects capable of
    /// detecting transient conditions.
    /// </summary>
    /// <typeparam name = "TTransient">The type implementing the <see cref = "ITransientErrorDetectionStrategy" />
    /// interface which is responsible for detecting transient conditions.
    /// </typeparam>
    /// <typeparam name = "TThreshold">The type implementing the <see cref = "IThresholdExceededStrategy" />
    /// interface which is responsible for detecting threshold exceeded conditions.
    /// </typeparam>
    public class RetryPolicy<TTransient, TThreshold> : RetryPolicyBase
            where TTransient : ITransientErrorDetectionStrategy, new()
            where TThreshold : IThresholdExceededStrategy, new()
    {
        private const int MaxRetryDelayMs = 60000;
        private readonly TTransient errorDetectionStrategy = new TTransient();
        private readonly TThreshold thresholdExceededStrategy = new TThreshold();
        private readonly ShouldRetry shouldRetry;

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryPolicy{TTransient, TThreshold}"/> class with the
        /// specified number of retry attempts and default fixed time interval between retries.
        /// </summary>
        /// <param name = "retryCount">The number of retry attempts.</param>
        public RetryPolicy(int retryCount)
            : this(retryCount, RetryPolicyBase.DefaultRetryInterval, RetryPolicyBase.DefaultThresholdExceededInterval)
        {
            // linear retry policy using the specified retry count and the default retry interval
            Debug.Assert(retryCount >= 0, "retryCount >= 0");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryPolicy{TTransient, TThreshold}"/> class with the
        /// specified number of retry attempts and fixed time interval between retries.
        /// </summary>
        /// <param name = "retryCount">The number of retry attempts.</param>
        /// <param name = "retryInterval">The interval between retries.</param>
        /// <param name = "thresholdInterval">
        /// The max time interval for executing operations as part of an individual request.
        /// If the total time exceeds the threshold, a service exception will be thrown.
        /// </param>
        public RetryPolicy(int retryCount, TimeSpan retryInterval, TimeSpan thresholdInterval)
            : this(true)
        {
            Debug.Assert(retryCount >= 0, "retryCount >= 0");
            Debug.Assert(retryInterval.Ticks >= 0, "retryInterval >= 0");
            Debug.Assert(thresholdInterval.Ticks >= 0, "thresholdInterval >= 0");

            this.MaxRetryCount = retryCount;
            this.ThresholdExceededInterval = thresholdInterval;

            if (retryCount == 0)
            {
                // no retry
                this.shouldRetry = delegate (int currentRetryCount, Exception lastException, out TimeSpan interval)
                {
                    interval = TimeSpan.Zero;
                    return false;
                };
            }
            else
            {
                // linear retry policy using the specified retry count and retry interval
                this.shouldRetry = delegate (int currentRetryCount, Exception lastException, out TimeSpan interval)
                {
                    // only retry for transient errors if we didn't exceed the retry count
                    if (currentRetryCount < this.MaxRetryCount && this.errorDetectionStrategy.IsTransient(lastException))
                    {
                        interval = retryInterval;
                        return true;
                    }

                    interval = TimeSpan.Zero;
                    return false;
                };
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryPolicy{TTransient, TThreshold}"/> class with the
        /// specified number of retry attempts and back-off parameters for calculating the exponential delay between retries.
        /// </summary>
        /// <param name = "retryCount">The number of retry attempts.</param>
        /// <param name = "minBackoff">The minimum back-off time.</param>
        /// <param name = "maxBackoff">The maximum back-off time.</param>
        /// <param name = "deltaBackoff">The time value which will be used for calculating a random delta in the exponential delay between retries.</param>
        /// <param name = "thresholdInterval">
        /// The max time interval for executing operations as part of an individual request.
        /// If the total time exceeds the threshold, a service exception will be thrown.
        /// </param>
        public RetryPolicy(int retryCount, TimeSpan minBackoff, TimeSpan maxBackoff, TimeSpan deltaBackoff, TimeSpan thresholdInterval)
            : this(true)
        {
            Debug.Assert(retryCount >= 0, "retryCount >= 0");
            Debug.Assert(minBackoff.Ticks >= 0, "minBackoff >= 0");
            Debug.Assert(maxBackoff.Ticks >= 0, "maxBackoff >= 0");
            Debug.Assert(deltaBackoff.Ticks >= 0, "deltaBackoff >= 0");
            Debug.Assert(minBackoff.TotalMilliseconds <= maxBackoff.TotalMilliseconds, "minBackoff <= maxBackoff");
            Debug.Assert(thresholdInterval.Ticks >= 0, "thresholdInterval >= 0");

            this.MaxRetryCount = retryCount;
            this.ThresholdExceededInterval = thresholdInterval;

            // exponential retry policy, with minimum and maximum bounds for the retry interval
            this.shouldRetry = delegate (int currentRetryCount, Exception lastException, out TimeSpan retryInterval)
            {
                // compute the next retry interval if the error is transient and we didn't exceed the retry count limit
                if (currentRetryCount < this.MaxRetryCount && this.errorDetectionStrategy.IsTransient(lastException))
                {
                    Random random = new Random();

                    // the retry delta is computed as random value in from the range of 80% to 120% of the specified
                    // deltaBackoff value, increasing exponentially with the retry count
                    int delta = (int)((Math.Pow(2.0, currentRetryCount) - 1.0) *
                                random.Next((int)(deltaBackoff.TotalMilliseconds * 0.8), (int)(deltaBackoff.TotalMilliseconds * 1.2)));

                    // the retry interval is bounded by the minBackoff and maxBackoff values and is increasing with exponential delta increments
                    int interval = (int)Math.Min(checked(minBackoff.TotalMilliseconds + delta), maxBackoff.TotalMilliseconds);

                    retryInterval = TimeSpan.FromMilliseconds(interval);

                    return true;
                }

                retryInterval = TimeSpan.Zero;
                return false;
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryPolicy{TTransient, TThreshold}"/> class with the
        /// specified number of retry attempts and parameters defining the progressive delay between retries.
        /// </summary>
        /// <param name = "retryCount">The number of retry attempts.</param>
        /// <param name = "initialInterval">The initial interval which will apply for the first retry.</param>
        /// <param name = "increment">The incremental time value which will be used for calculating the progressive delay between retries.</param>
        /// <param name = "thresholdInterval">
        ///   The max time interval for executing operations as part of an individual request.
        ///   If the total time exceeds the threshold, a service exception will be thrown.
        /// </param>
        /// <param name = "retryCountAfterWhichIncidentIsLogged">
        ///  In some cases you would like to get notified after a certain number of retries but still keep the retry going. This flag is for those scenarios
        /// </param>
        public RetryPolicy(int retryCount, TimeSpan initialInterval, TimeSpan increment, TimeSpan thresholdInterval, int? retryCountAfterWhichIncidentIsLogged)
            : this(false)
        {
            Debug.Assert(retryCount >= 0, "retryCount >= 0");
            Debug.Assert(initialInterval.Ticks >= 0, "initialInterval >= 0");
            Debug.Assert(increment.Ticks >= 0, "increment >= 0");
            Debug.Assert(thresholdInterval.Ticks >= 0, "thresholdInterval >= 0");
            Debug.Assert(!retryCountAfterWhichIncidentIsLogged.HasValue || retryCountAfterWhichIncidentIsLogged.Value >= 0, "retryCountAfterWhichExceptionBeThrown is either null or >=0");

            this.MaxRetryCount = retryCount;
            this.RetryCountAfterWhichIncidentIsLogged = retryCountAfterWhichIncidentIsLogged;
            this.ThresholdExceededInterval = thresholdInterval;

            // linear retry policy in the number of retry counts with an initialInterval lower bound
            this.shouldRetry = delegate (int currentRetryCount, Exception lastException, out TimeSpan retryInterval)
            {
                // compute the next retry interval if the error is transient and we didn't exceed the retry count limit
                if (currentRetryCount < this.MaxRetryCount && this.errorDetectionStrategy.IsTransient(lastException))
                {
                    retryInterval =
                        TimeSpan.FromMilliseconds(
                            initialInterval.TotalMilliseconds
                            + (increment.TotalMilliseconds * currentRetryCount));

                    return true;
                }

                retryInterval = TimeSpan.Zero;
                return false;
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryPolicy{TTransient, TThreshold}"/> class with default settings.
        /// </summary>
        /// <param name = "fastFirstRetry">Fast first retry</param>
        private RetryPolicy(bool fastFirstRetry)
        {
            // the first retry is executed without waiting for a fixed interval
            this.FastFirstRetry = fastFirstRetry;
        }

        /// <summary>
        ///   Repetitively executes the specified action while it satisfies the current retry policy.
        /// </summary>
        /// <param name = "action">A delegate representing the executable action which doesn't return any results.</param>
        /// <param name = "state">The state object to be passed as an argument to the executable action</param>
        public override void ExecuteAction(Action<object> action, object state = null)
        {
            this.ExecuteAction(
                s =>
                {
                    action(s);
                    return 0;
                },
                state);
        }

        /// <summary>
        ///   Repetitively executes the specified action while it satisfies the current retry policy.
        /// </summary>
        /// <typeparam name = "TResult">The type of result expected from the executable action.</typeparam>
        /// <param name = "func">A delegate representing the executable action which returns the result of type T.</param>
        /// <param name = "state">The state object to be passed as an argument to the executable action</param>
        /// <returns>The result from the action.</returns>
        public override TResult ExecuteAction<TResult>(Func<object, TResult> func, object state = null)
        {
            int retryCount = 0;
            Stopwatch totalElapsedTime = Stopwatch.StartNew();

            while (true)
            {
                TimeSpan delay;

                // if current execution time exceeds the threshold, as specified by the strategy, skip executing the operation and return an error
                if (this.thresholdExceededStrategy.IsThresholdExceeded(this.ThresholdExceededInterval, totalElapsedTime.Elapsed))
                {
                    throw new OperationCanceledException(
                        $"The current execution time {totalElapsedTime.Elapsed} exceeds the threshold {this.ThresholdExceededInterval} after {retryCount} retrys");
                }

                Stopwatch stopwatch = Stopwatch.StartNew();

                try
                {
                    // execute the user code
                    TResult result = func(state);
                    stopwatch.Stop();

                    this.ResultHandler?.Invoke(result, state);

                    this.LogSuccess((int)stopwatch.ElapsedMilliseconds, state);

                    return result;
                }
                catch (Exception ex)
                {
                    stopwatch.Stop();

                    // Execute user error handling code if provided
                    this.ErrorHandler?.Invoke(ex, state);

                    int elapsedTimeInMilliseconds = (int)stopwatch.ElapsedMilliseconds;
                    if (!this.shouldRetry(retryCount, ex, out delay))
                    {
                        throw;
                    }

                    // This is to notify that retries has exceeded a certain count, but we would like more retries before the operation is aborted
                    if (this.RetryCountAfterWhichIncidentIsLogged.HasValue && retryCount > this.RetryCountAfterWhichIncidentIsLogged)
                    {
                        throw new OperationCanceledException($"The retry threshold of {this.RetryCountAfterWhichIncidentIsLogged} was exceeded", ex);
                    }

                    // increase the retry count and check whether this is a transient error that needs retrying
                    retryCount++;
                    Interlocked.Increment(ref CosmosDbTestUtils.TestRunStatus.NumberOfRetries);
                }

                // Perform an extra check in the delay interval. Should prevent from accidentally ending up with the value of -1 which will block a thread indefinitely.
                // In addition, any other negative numbers will cause an ArgumentOutOfRangeException fault which will be thrown by Thread.Sleep.
                if (delay.TotalMilliseconds < 0 || delay.TotalMilliseconds > MaxRetryDelayMs)
                {
                    delay = TimeSpan.FromMilliseconds(MaxRetryDelayMs);
                }

                if (retryCount > 1 || !this.FastFirstRetry)
                {
                    Thread.Sleep(delay);
                }
            }
        }

        /// <summary>
        ///   Repetitively executes the specified action while it satisfies the current retry policy.
        /// </summary>
        /// <param name = "func">A delegate representing the executable action which returns the result of type T.</param>
        /// <param name = "token">Cancellation token.</param>
        /// <param name = "state">The state object to be passed as an argument to the executable action</param>
        public override async Task ExecuteActionAsync(Func<object, Task> func, CancellationToken token, object state = null)
        {
            await this.ExecutionActionAsync<int>(
                async s =>
                {
                    await func(s);
                    return 0;
                },
                token,
                state);
        }

        /// <summary>
        ///   Repetitively executes the specified action while it satisfies the current retry policy.
        /// </summary>
        /// <typeparam name = "TResult">The type of result expected from the executable action.</typeparam>
        /// <param name = "func">A delegate representing the executable action which returns the result of type T.</param>
        /// <param name = "token">Cancellation token.</param>
        /// <param name = "state">The state object to be passed as an argument to the executable action</param>
        /// <returns>The result from the action.</returns>
        public override async Task<TResult> ExecutionActionAsync<TResult>(Func<object, Task<TResult>> func, CancellationToken token, object state = null)
        {
            int retryCount = 0;
            Stopwatch totalElapsedTime = Stopwatch.StartNew();

            while (!token.IsCancellationRequested)
            {
                TimeSpan delay;

                // if current execution time exceeds the threshold, as specified by the strategy, skip executing the operation and return an error
                if (this.thresholdExceededStrategy.IsThresholdExceeded(this.ThresholdExceededInterval, totalElapsedTime.Elapsed))
                {
                    throw new OperationCanceledException(
                        $"The current execution time {totalElapsedTime.Elapsed} exceeds the threshold {this.ThresholdExceededInterval} after {retryCount} retrys");
                }

                Stopwatch stopwatch = Stopwatch.StartNew();

                try
                {
                    // execute the user code
                    TResult result = await func(state);
                    stopwatch.Stop();

                    this.ResultHandler?.Invoke(result, state);

                    this.LogSuccess((int)stopwatch.ElapsedMilliseconds, state);

                    return result;
                }
                catch (Exception ex)
                {
                    stopwatch.Stop();

                    // Execute user error handling code if provided
                    this.ErrorHandler?.Invoke(ex, state);

                    if (!this.shouldRetry(retryCount, ex, out delay))
                    {
                        throw;
                    }

                    // This is to notify that retries has exceeded a certain count, but we would like more retries before the operation is aborted
                    if (this.RetryCountAfterWhichIncidentIsLogged.HasValue && retryCount > this.RetryCountAfterWhichIncidentIsLogged)
                    {
                        throw new OperationCanceledException($"The retry threshold of {this.RetryCountAfterWhichIncidentIsLogged} was exceeded", ex);
                    }

                    // increase the retry count and check whether this is a transient error that needs retrying
                    retryCount++;
                    Interlocked.Increment(ref CosmosDbTestUtils.TestRunStatus.NumberOfRetries);
                }

                // Perform an extra check in the delay interval. Should prevent from accidentally ending up with the value of -1 which will block a thread indefinitely.
                // In addition, any other negative numbers will cause an ArgumentOutOfRangeException fault which will be thrown by Thread.Sleep.
                if (delay.TotalMilliseconds < 0 || delay.TotalMilliseconds > MaxRetryDelayMs)
                {
                    delay = TimeSpan.FromMilliseconds(MaxRetryDelayMs);
                }

                if (retryCount > 1 || !this.FastFirstRetry)
                {
                    await Task.Delay(delay);
                }
            }

            throw new Exception("The operation was cancelled");
        }

        /// <summary>
        ///   Defines a delegate that is responsible for notifying the subscribers whenever a retry condition is encountered.
        /// </summary>
        /// <param name = "retryCount">The current retry attempt count.</param>
        /// <param name = "lastException">The exception which caused the retry conditions to occur.</param>
        /// <param name = "delay">The delay indicating how long the current thread will be suspended for before the next iteration will be invoked.</param>
        protected delegate bool ShouldRetry(int retryCount, Exception lastException, out TimeSpan delay);
    }
}
