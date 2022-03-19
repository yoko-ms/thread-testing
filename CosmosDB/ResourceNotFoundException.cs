// ----------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------

using System;
using System.Runtime.Serialization;

namespace TestThreading.CosmosDB
{
    /// <summary>
    ///     The exception that is raised when a resource is not found.
    /// </summary>
    [Serializable]
    public class ResourceNotFoundException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ResourceNotFoundException"/> class.
        /// </summary>
        public ResourceNotFoundException()
            : base()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ResourceNotFoundException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">
        ///     A <see cref="string"/> that represents the message of the exception.
        /// </param>
        public ResourceNotFoundException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ResourceNotFoundException"/> class with a specified error message
        ///     a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">
        ///     The message that describes the error.
        /// </param>
        /// <param name="innerException">
        ///     The exception that is the cause of the current exception, or a null reference if no
        ///     inner exception is specified.
        /// </param>
        public ResourceNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceNotFoundException" /> class.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        protected ResourceNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
