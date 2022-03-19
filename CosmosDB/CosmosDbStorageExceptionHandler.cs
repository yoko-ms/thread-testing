// ----------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------

using System;

namespace TestThreading.CosmosDB
{
    /// <summary>
    /// Cosmos DB interface exception type.
    /// </summary>
    internal sealed class CosmosDbStorageExceptionHandler
    {
        /// <summary>
        /// Type of exceptions to be handled
        /// </summary>
        internal enum StorageExceptionType
        {
            /// <summary>
            /// No exception.
            /// </summary>
            None,

            /// <summary>
            /// Document not found.
            /// </summary>
            ResourceNotFound,

            /// <summary>
            /// Etag is not matched.
            /// </summary>
            EtagNotMatched,

            /// <summary>
            /// Duplicated Id conflicts.
            /// </summary>
            DuplicatedId,

            /// <summary>
            /// Stroage exception.
            /// </summary>
            StorageException
        }

        /// <summary>
        /// Gets or sets message text.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets exception type.
        /// </summary>
        public StorageExceptionType ExceptionType { get; set; }

        /// <summary>
        /// Gets or sets storage exception detected.
        /// </summary>
        public CosmosDbStorageException StorageException { get; set; }

        /// <summary>
        /// Parse the storage exception info.
        /// </summary>
        /// <param name="ex">Exception detected from Cosmos DB client.</param>
        /// <param name="collectionName">Collection name</param>
        /// <param name="documentId">The document ID</param>
        /// <param name="partitionId">The partition Id</param>
        /// <param name="methodName">Name of caller method.</param>
        /// <returns>A <see cref="CosmosDbStorageExceptionHandler"/> object.</returns>
        public static CosmosDbStorageExceptionHandler ParseException(
            Exception ex,
            string collectionName,
            string documentId,
            string partitionId)
        {
            if (TryGetDocumentDbStorageException(ex, out CosmosDbStorageException docDbStorageException))
            {
                if (docDbStorageException.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return new CosmosDbStorageExceptionHandler()
                    {
                        Message = $"document not found, collectionName: {collectionName}, documentId: {documentId}, partitionKey: {partitionId}",
                        ExceptionType = StorageExceptionType.ResourceNotFound,
                        StorageException = docDbStorageException
                    };
                }
                else if (docDbStorageException.StatusCode == System.Net.HttpStatusCode.PreconditionFailed)
                {
                    return new CosmosDbStorageExceptionHandler()
                    {
                        Message = $"document etag not matched, collectionName: {collectionName}, documentId: {documentId}, partitionKey: {partitionId}",
                        ExceptionType = StorageExceptionType.EtagNotMatched,
                        StorageException = docDbStorageException
                    };
                }
                else if (docDbStorageException.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    // DuplicatedIdException for create document operation should be categorized as caller error
                    return new CosmosDbStorageExceptionHandler()
                    {
                        Message = $"document with this ID already exist, collectionName: {collectionName}, partitionKey: {partitionId}",
                        ExceptionType = StorageExceptionType.DuplicatedId,
                        StorageException = docDbStorageException
                    };
                }

                // if ex is already a CosmosDbStorageException, preserve the call stack
                // otherwise, just throw the CosmosDbStorageException
                if (!(ex is CosmosDbStorageException))
                {
                    return new CosmosDbStorageExceptionHandler()
                    {
                        Message = String.Empty,
                        ExceptionType = StorageExceptionType.StorageException,
                        StorageException = docDbStorageException
                    };
                }
            }

            return new CosmosDbStorageExceptionHandler()
            {
                Message = String.Empty,
                ExceptionType = StorageExceptionType.None
            };
        }

        /// <summary>Tries the get document database storage exception.</summary>
        /// <param name="ex">The ex.</param>
        /// <param name="documentDbStorageException">The document database storage exception.</param>
        /// <returns>True, if CosmosDbStorageException was found</returns>
        private static bool TryGetDocumentDbStorageException(
            Exception ex,
            out CosmosDbStorageException documentDbStorageException)
        {
            if (ex is CosmosDbStorageException docDbEx)
            {
                documentDbStorageException = docDbEx;
                return true;
            }

            if (ex is AggregateException ae && ae.InnerException is CosmosDbStorageException docDbExFromAe)
            {
                documentDbStorageException = docDbExFromAe;
                return true;
            }

            documentDbStorageException = null;
            return false;
        }
    }
}
