// ----------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;

namespace TestThreading.RetryPolicy
{
    /// <summary>
    ///   Provides the base implementation of the retry mechanism for unreliable actions and transient conditions.
    /// </summary>
    public abstract class RetryPolicyBase
    {
        /// <summary>
        ///   The default number of retry attempts.
        /// </summary>
        public const int DefaultClientRetryCount = 5;

        /// <summary>
        /// Initializes a default policy that implements a random exponential retry interval configured with
        /// <see cref = "DefaultClientRetryCount" />,
        /// <see cref = "DefaultMinBackoff" />,
        /// <see cref = "DefaultMaxBackoff" /> and
        /// <see cref = "DefaultClientBackoff" /> parameters.
        /// The default retry policy treats all caught exceptions as transient errors.
        /// </summary>
        private static readonly RetryPolicyBase defaultExponential
            = new RetryPolicy<TransientErrorCatchAllStrategy, ThresholdExceededAllowAllStrategy>(
                DefaultClientRetryCount, DefaultMinBackoff, DefaultMaxBackoff, DefaultClientBackoff, DefaultThresholdExceededInterval);

        /// <summary>
        /// Initializes a default policy that implements a fixed retry interval configured with
        /// <see cref = "RetryPolicy.DefaultClientRetryCount" /> and
        /// <see cref = "RetryPolicy.DefaultRetryInterval" />  parameters.
        /// The default retry policy treats all caught exceptions as transient errors.
        /// </summary>
        private static readonly RetryPolicyBase defaultFixed
            = new RetryPolicy<TransientErrorCatchAllStrategy, ThresholdExceededAllowAllStrategy>(
                DefaultClientRetryCount, DefaultRetryInterval, DefaultThresholdExceededInterval);

        /// <summary>
        /// Initializes a default policy that implements a progressive retry interval configured with
        /// <see cref = "RetryPolicy.DefaultClientRetryCount" />,
        /// <see cref = "RetryPolicy.DefaultRetryInterval" /> and
        /// <see cref = "RetryPolicy.DefaultRetryIncrement" /> parameters.
        /// The default retry policy treats all caught exceptions as transient errors.
        /// </summary>
        private static readonly RetryPolicyBase defaultProgressive
            = new RetryPolicy<TransientErrorCatchAllStrategy, ThresholdExceededAllowAllStrategy>(
                DefaultClientRetryCount, DefaultRetryInterval, DefaultRetryIncrement, DefaultThresholdExceededInterval, null);

        /// <summary>
        /// Initializes static members of the <see cref="RetryPolicy" /> class. 
        /// </summary>
        static RetryPolicyBase()
        {
            DefaultClientBackoff = TimeSpan.FromSeconds(10.0);
            DefaultMaxBackoff = TimeSpan.FromSeconds(30.0);
            DefaultMinBackoff = TimeSpan.FromSeconds(1.0);
            DefaultRetryIncrement = TimeSpan.FromMilliseconds(500);
            DefaultRetryInterval = TimeSpan.FromSeconds(1.0);
            DefaultThresholdExceededInterval = TimeSpan.FromMinutes(3);
        }

        /// <summary>
        /// Gets a default policy that implements a random exponential retry interval configured with
        /// <see cref = "DefaultClientRetryCount" />,
        /// <see cref = "DefaultMinBackoff" />,
        /// <see cref = "DefaultMaxBackoff" /> and
        /// <see cref = "DefaultClientBackoff" /> parameters.
        /// The default retry policy treats all caught exceptions as transient errors.
        /// </summary>
        public static RetryPolicyBase DefaultExponential
        {
            get { return defaultExponential; }
        }

        /// <summary>
        ///  Gets a default policy that implements a fixed retry interval configured with
        /// <see cref = "RetryPolicy.DefaultClientRetryCount" /> and
        /// <see cref = "RetryPolicy.DefaultRetryInterval" />
        /// parameters. The default retry policy treats all caught exceptions as transient errors.
        /// </summary>
        public static RetryPolicyBase DefaultFixed
        {
            get { return defaultFixed; }
        }

        /// <summary>
        /// Gets a default policy that implements a progressive retry interval configured with
        /// <see cref = "RetryPolicy.DefaultClientRetryCount" />,
        /// <see cref = "RetryPolicy.DefaultRetryInterval" /> and
        /// <see cref = "RetryPolicy.DefaultRetryIncrement" /> parameters.
        /// The default retry policy treats all caught exceptions as transient errors.
        /// </summary>
        public static RetryPolicyBase DefaultProgressive
        {
            get { return defaultProgressive; }
        }

        /// <summary>
        ///   Gets the default amount of time used when calculating a random delta in the exponential delay between retries.
        /// </summary>
        public static TimeSpan DefaultClientBackoff { get; private set; }

        /// <summary>
        ///   Gets the default maximum amount of time used when calculating the exponential delay between retries.
        /// </summary>
        public static TimeSpan DefaultMaxBackoff { get; private set; }

        /// <summary>
        ///   Gets the default minimum amount of time used when calculating the exponential delay between retries.
        /// </summary>
        public static TimeSpan DefaultMinBackoff { get; private set; }

        /// <summary>
        ///   Gets the default amount of time defining a time increment between retry attempts in the progressive delay policy.
        /// </summary>
        public static TimeSpan DefaultRetryIncrement { get; private set; }

        /// <summary>
        ///   Gets the default amount of time defining an interval between retries.
        /// </summary>
        public static TimeSpan DefaultRetryInterval { get; private set; }

        /// <summary>
        ///   Gets the default amount of time for defining an interval for retrying.
        /// </summary>
        public static TimeSpan DefaultThresholdExceededInterval { get; private set; }

        /// <summary>
        /// Gets or sets allows user's code to be executed with the occurred exception
        /// </summary>
        public Action<Exception, object> ErrorHandler { get; set; }

        /// <summary>
        /// Gets or sets allows user to provide custom validation code for the result
        /// </summary>
        public Action<object, object> ResultHandler { get; set; }

        /// <summary>
        ///   Gets or sets a value indicating whether or not the very first retry attempt will be made immediately
        ///   whereas the subsequent retries will remain subject to retry interval.
        /// </summary>
        public bool FastFirstRetry { get; set; }

        /// <summary>
        ///   Gets or sets the maximum number of retry attempts.
        /// </summary>
        public int MaxRetryCount { get; set; }

        /// <summary>
        /// Gets or sets the Retry count after which exception be thrown
        ///  In some cases you would like to get notified after a certain number of retries but still keep the retrie going. This flag is for those scenarios
        /// </summary>
        public int? RetryCountAfterWhichIncidentIsLogged { get; set; }

        /// <summary>
        ///   Gets or sets the max time interval for executing operations as part of an individual request.
        ///   If the total time exceeds the threshold, a service exception will be thrown.
        /// </summary>
        public TimeSpan ThresholdExceededInterval { get; set; }

        /// <summary>
        ///   Repetitively executes the specified action while it satisfies the current retry policy.
        /// </summary>
        /// <param name = "action">A delegate representing the executable action which doesn't return any results.</param>
        /// <param name = "state">The state object to be passed as an argument to the executable action</param>
        public abstract void ExecuteAction(Action<object> action, object state = null);

        /// <summary>
        ///   Repetitively executes the specified action while it satisfies the current retry policy.
        /// </summary>
        /// <typeparam name = "TResult">The type of result expected from the executable action.</typeparam>
        /// <param name = "func">A delegate representing the executable action which returns the result of type T.</param>
        /// <param name = "state">The state object to be passed as an argument to the executable action</param>
        /// <returns>The result from the action.</returns>
        public abstract TResult ExecuteAction<TResult>(Func<object, TResult> func, object state = null);

        /// <summary>
        /// Repetitively executes the specified action while it satisfies the current retry policy.
        /// </summary>
        /// <typeparam name = "TResult">The type of result expected from the executable action.</typeparam>
        /// <param name = "func">A delegate representing the executable action which returns the result of type T.</param>
        /// <param name = "token">Cancellation token.</param>
        /// <param name = "state">The state object to be passed as an argument to the executable action</param>
        /// <returns>The result from the action.</returns>
        public abstract Task ExecuteActionAsync(Func<object, Task> func, CancellationToken token, object state = null);

        /// <summary>
        /// Repetitively executes the specified action while it satisfies the current retry policy.
        /// </summary>
        /// <typeparam name = "TResult">The type of result expected from the executable action.</typeparam>
        /// <param name = "func">A delegate representing the executable action which returns the result of type T.</param>
        /// <param name = "token">Cancellation token.</param>
        /// <param name = "state">The state object to be passed as an argument to the executable action</param>
        /// <returns>The result from the action.</returns>
        public abstract Task<TResult> ExecutionActionAsync<TResult>(Func<object, Task<TResult>> func, CancellationToken token, object state = null);

        /// <summary>
        /// Log success
        /// </summary>
        /// <param name="operationTime">Operation latency ms</param>
        /// <param name="state">Parameters</param>
        protected virtual void LogSuccess(int operationTime, object state)
        {
        }

        /// <summary>
        /// Log success
        /// </summary>
        /// <param name="error">Operation exception</param>
        /// <param name="operationTime">Time of operation</param>
        /// <param name="state">Parameters</param>
        protected virtual void LogFailure(Exception error, int operationTime, object state)
        {
        }

        /// <summary>
        /// Log success
        /// </summary>
        /// <param name="error">Operation exception</param>
        /// <param name="operationTime">Time of operation</param>
        /// <param name="state">Parameters</param>
        protected virtual void LogRetry(Exception error, int operationTime, object state)
        {
        }

        /// <summary>
        ///   Implements a strategy that never times out a request.
        /// </summary>
        public sealed class ThresholdExceededAllowAllStrategy : IThresholdExceededStrategy
        {
            /// <summary>
            /// Is threshold exceeded
            /// </summary>
            /// <param name="threshold">Operation threshold</param>
            /// <param name="elapsed">Time spent so far</param>
            /// <returns>Allways false</returns>
            public bool IsThresholdExceeded(TimeSpan threshold, TimeSpan elapsed)
            {
                return false;
            }
        }

        /// <summary>
        ///   Implements a strategy that that never times out a request.
        /// </summary>
        public sealed class ThresholdExceedeStopStrategy : IThresholdExceededStrategy
        {
            /// <summary>
            /// Is threshold exceeded
            /// </summary>
            /// <param name="threshold">Operation threshold</param>
            /// <param name="elapsed">Time spent so far</param>
            /// <returns>Allways false</returns>
            public bool IsThresholdExceeded(TimeSpan threshold, TimeSpan elapsed)
            {
                return elapsed > threshold;
            }
        }

        /// <summary>
        ///   Implements a strategy that treats all exceptions as transient errors.
        /// </summary>
        public sealed class TransientErrorCatchAllStrategy : ITransientErrorDetectionStrategy
        {
            /// <summary>
            /// Is exception transient
            /// </summary>
            /// <param name="ex">Exception</param>
            /// <returns>Allways true</returns>
            public bool IsTransient(Exception ex)
            {
                return true;
            }
        }
    }
}
