// ----------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------

using System;

namespace TestThreading.CosmosDB
{
    /// <summary>
    /// The exception to throw when update failed due to etag not matching.
    /// This is a protection against one overwrite other's changes.
    /// </summary>
    public class StorageEtagNotMatchException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="StorageEtagNotMatchException"/> class with a specified error message.
        /// </summary>
        public StorageEtagNotMatchException()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="StorageEtagNotMatchException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">
        ///     A <see cref="string"/> that represents the message of the exception.
        /// </param>
        public StorageEtagNotMatchException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="StorageEtagNotMatchException"/> class with a specified error message
        ///     a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">
        ///     The message that describes the error.
        /// </param>
        /// <param name="innerException">
        ///     The exception that is the cause of the current exception, or a null reference if no
        ///     inner exception is specified.
        /// </param>
        public StorageEtagNotMatchException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
