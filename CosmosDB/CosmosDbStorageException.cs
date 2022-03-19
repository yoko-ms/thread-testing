// ----------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------

using System;
using System.Linq;
using System.Net;

namespace TestThreading.CosmosDB
{
    /// <summary>
    /// Wrapper class for the Cosmos DB operation exception.
    /// </summary>
    [Serializable]
    public class CosmosDbStorageException : Exception
    {
        /// <summary>
        /// Status code of the exception
        /// </summary>
        private HttpStatusCode? statusCode;

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDbStorageException"/> class.
        /// </summary>
        /// <param name="message">Message to be added to generated exception.</param>
        /// <param name="innerException">Original inner exception object.</param>
        public CosmosDbStorageException(string message, Exception innerException)
            : base(message, innerException)
        {
            if (innerException is Microsoft.Azure.Cosmos.CosmosException)
            {
                // SDK V3 document exception
                Microsoft.Azure.Cosmos.CosmosException docException
                    = this.InnerException as Microsoft.Azure.Cosmos.CosmosException;
                this.statusCode = docException?.StatusCode;
            }
        }

        /// <summary>
        /// Gets or sets the status code
        /// </summary>
        public HttpStatusCode? StatusCode
        {
            get
            {
                return this.statusCode;
            }

            set
            {
                this.statusCode = value;
            }
        }

        /// <summary>
        /// Processes the exception thrown
        /// </summary>
        /// <param name="ex">Exception thrown</param>
        public static void ProcessCosmosDbExceptionThrown(Exception ex)
        {
            AggregateException aggregateException = ex as AggregateException;
            if (aggregateException?.Flatten().InnerExceptions.Count() == 1)
            {
                throw new CosmosDbStorageException(
                    aggregateException.Flatten().InnerExceptions.Single().Message,
                    aggregateException.Flatten().InnerException);
            }

            throw new CosmosDbStorageException(ex.Message, ex);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDbStorageException"/> class.
        /// </summary>
        protected CosmosDbStorageException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDbStorageException"/> class.
        /// </summary>
        /// <param name="message">Message to be saved.</param>
        protected CosmosDbStorageException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDbStorageException"/> class.
        /// </summary>
        /// <param name="serializationInfo">Serialization info object.</param>
        /// <param name="streamingContext">Streaming context.</param>
        protected CosmosDbStorageException(
            System.Runtime.Serialization.SerializationInfo serializationInfo,
            System.Runtime.Serialization.StreamingContext streamingContext)
        {
            throw new NotImplementedException();
        }
    }
}
