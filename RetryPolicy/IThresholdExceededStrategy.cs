//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
//----------------------------------------------------------------

using System;

namespace TestThreading.RetryPolicy
{
    /// <summary>
    /// treshold exceeded detection strategy
    /// </summary>
    public interface IThresholdExceededStrategy
    {
        /// <summary>
        /// Returns true if the execution treshold is exceeded.
        /// </summary>
        /// <param name="threshold">The threshold</param>
        /// <param name="elapsed">The time elapsed</param>
        bool IsThresholdExceeded(TimeSpan threshold, TimeSpan elapsed);
    }
}