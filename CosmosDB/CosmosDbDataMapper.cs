// ----------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using TestThreading.RetryPolicy;

namespace TestThreading.CosmosDB
{
    /// <summary>
    /// Cosmos DB interface.
    /// </summary>
    public class CosmosDbDataMapper
    {
        /// <summary>
        /// Lock for the data table management.
        /// </summary>
        private static readonly object LockObject = new object();

        /// <summary>
        /// Table of Cosmos DB client interface <see cref="CosmosDBClient"/> object.
        /// </summary>
        /// <remarks>
        /// From Cosmos DB SDK: Use a singleton DocumentDB client for the lifetime of your application.
        /// Each DocumentClient instance is thread-safe and performs efficient connection management and address caching when operating in Direct Mode.
        /// To allow efficient connection management and better performance by DocumentClient, it is recommended to use a single instance of
        /// DocumentClient per AppDomain for the lifetime of the application.
        /// </remarks>
        private static Lazy<CosmosDBClient> cosmosDbClient = new Lazy<CosmosDBClient>(() => new CosmosDBClient());

        /// <summary>
        /// The Cosmos DB account key or resource token to use to create the client.
        /// </summary>
        internal string AuthKeyOrResourceToken;

        /// <summary>
        /// The service endpoint to use to create the Cosmos DB client.
        /// </summary>
        internal string ServiceEndpoint;

        /// <summary>
        /// The Cosmos DB request transient failure (e.g. time out, 503) retry policy
        /// </summary>
        internal RetryPolicyBase CosmosDbRetryPolicy
            = new RetryPolicy<CosmosDbErrorDetectionStrategy, RetryPolicyBase.ThresholdExceededAllowAllStrategy>(
                        3,
                        TimeSpan.FromSeconds(1),
                        TimeSpan.FromSeconds(1),
                        TimeSpan.FromSeconds(10),
                        null);

        /// <summary>
        /// The Cosmos DB throttled request retry policy
        /// </summary>
        internal RetryPolicyBase CosmosDbThrottledRequestRetryPolicy
            = new RetryPolicy<CosmosDbThrottleDetectionStrategy, RetryPolicyBase.ThresholdExceededAllowAllStrategy>(
                int.MaxValue,
                TimeSpan.Zero,
                TimeSpan.Zero,
                TimeSpan.FromSeconds(10),
                null);

        /// <summary>
        ///  Initializes a new instance of the <see cref="CosmosDbDataMapper"/> class.
        /// </summary>
        public CosmosDbDataMapper()
        {
            this.CosmosDbRetryPolicy.ErrorHandler = (a, b) => CosmosDbRequestTransientExceptionHandler(a, b);
            this.CosmosDbThrottledRequestRetryPolicy.ErrorHandler
                = (a, b) => this.CosmosDbRequestThrottledExceptionHandler(a, b);
        }

        public IEnumerable<T> QueryDb<T>(
            string collectionName,
            Expression<Func<T, bool>> searchPredicate,
            CancellationToken cancellationToken)
        {
            CosmosDbQuery<T> documentQuery = this.GetCosmosDbClientObject().QueryDb<T>(
                CosmosDbConfiguration.CpimCosmosDatabase,
                collectionName,
                searchPredicate,
                10,
                10);

            return this.InternalGetDataList<T>(
                documentQuery,
                collectionName,
                documentQuery.QueryString,
                cancellationToken);
        }

        public IEnumerable<T> QueryDbWithoutCache<T>(
            string collectionName,
            Expression<Func<T, bool>> searchPredicate,
            CancellationToken cancellationToken)
        {
            CosmosDbQuery<T> documentQuery = this.GetCosmosDbClientObject().QueryDb<T>(
                CosmosDbConfiguration.CpimCosmosDatabase,
                collectionName,
                searchPredicate,
                10,
                10);

            return this.InternalGetDataList(
                documentQuery,
                collectionName,
                documentQuery.QueryString,
                cancellationToken);
        }

        public IEnumerable<TP> SelectPropertiesFromAllDocuments<T, TP>(
            string collectionName,
            Expression<Func<T, TP>> selectPredicateExpression,
            CancellationToken cancellationToken)
        {
            CosmosDbQuery<TP> documentQuery = this.GetCosmosDbClientObject().SelectPropertiesFromAllDocuments(
                CosmosDbConfiguration.CpimCosmosDatabase,
                collectionName,
                selectPredicateExpression,
                10,
                10);

            return this.InternalGetDataList<TP>(
                documentQuery,
                collectionName,
                documentQuery.QueryString,
                cancellationToken);
        }

        public IEnumerable<TP> SearchAndSelectProperties<T, TP>(
            string collectionName,
            Expression<Func<T, bool>> searchPredicateExpression,
            Expression<Func<T, TP>> selectPredicateExpression,
            CancellationToken cancellationToken)
        {
            CosmosDbQuery<TP> documentQuery = this.GetCosmosDbClientObject().SearchAndSelectProperties(
                CosmosDbConfiguration.CpimCosmosDatabase,
                collectionName,
                searchPredicateExpression,
                selectPredicateExpression,
                10,
                10);

            return this.InternalGetDataList<TP>(
                documentQuery,
                collectionName,
                documentQuery.QueryString,
                cancellationToken);
        }

        public IEnumerable<T> QueryWithSql<T>(
            string collectionName,
            string sqlQuery,
            CancellationToken cancellationToken)
        {
            CosmosDbQuery<T> documentQuery = this.GetCosmosDbClientObject().QueryWithSql<T>(
                CosmosDbConfiguration.CpimCosmosDatabase,
                collectionName,
                sqlQuery,
                10,
                10);

            return this.InternalGetDataList(
                documentQuery,
                collectionName,
                documentQuery.QueryString,
                cancellationToken);
        }

        public async Task CreateOrUpdateDocAsync<T>(
            string collectionName,
            T data,
            string partitionId,
            string documentId)
        {
            await this.CosmosDbRetryPolicy.ExecuteActionAsync(
                async (o) => await this.InternalCreateOrUpdateAsync(collectionName, data, partitionId, documentId),
                default(CancellationToken));
        }

        public string UpdateDoc<T>(
            string collectionName,
            string documentId,
            T data,
            string partitionId,
            string eTag)
        {

            string updateDatedETag = string.Empty;
            this.CosmosDbRetryPolicy.ExecuteAction(o =>
            {
                try
                {
                    updateDatedETag = this.GetCosmosDbClientObject().ReplaceDocAsync(
                                            CosmosDbConfiguration.CpimCosmosDatabase,
                                            collectionName,
                                            documentId,
                                            data,
                                            partitionId,
                                            eTag)
                                        .GetAwaiter()
                                        .GetResult();
                }
                catch (Exception ex)
                {
                    HandleCosmosDbStorageException(ex, collectionName, documentId, partitionId);

                    // throw original exception.
                    throw;
                }
            });

            return updateDatedETag;
        }

        public string CreateDoc<T>(
            string collectionName,
            string documentId,
            T data,
            string partitionId)
        {
            string eTag = string.Empty;
            this.CosmosDbRetryPolicy.ExecuteAction(o =>
            {
                try
                {
                    eTag = this.GetCosmosDbClientObject().CreateDoc(
                        CosmosDbConfiguration.CpimCosmosDatabase,
                        collectionName,
                        data,
                        partitionId);
                }
                catch (Exception ex)
                {
                    HandleCosmosDbStorageException(ex, collectionName, documentId, partitionId);

                    // throw original exception.
                    throw;
                }
            });

            return eTag;
        }

        public CosmosDbQueryResult<T> GetObjectWithEtag<T>(
            string collectionName,
            string documentId,
            string partitionKey)
        {
            CosmosDbQueryResult<T> cosmosDataWithEtag = new CosmosDbQueryResult<T>();

            this.CosmosDbThrottledRequestRetryPolicy.ExecuteAction(
                a => this.CosmosDbRetryPolicy.ExecuteAction(
                    b =>
                    {
                        CosmosDbRequestRetryState retryState = (CosmosDbRequestRetryState)b;

                        try
                        {
                            CosmosDbRequestMetadata requestMetadata;
                            Tuple<T, CosmosDbRequestMetadata> result = this.GetCosmosDbClientObject().GetDocumentWithEtag<T>(
                                CosmosDbConfiguration.CpimCosmosDatabase,
                                collectionName,
                                documentId,
                                partitionKey);
                            requestMetadata = result.Item2;
                            cosmosDataWithEtag.DataObject = result.Item1;

                            cosmosDataWithEtag.Etag = requestMetadata.Etag;
                        }
                        catch (Exception ex)
                        {
                            HandleCosmosDbStorageException(ex, collectionName, documentId, partitionKey);

                            // throw original exception.
                            throw;
                        }
                    },
                    a),
                new CosmosDbRequestRetryState());
            return cosmosDataWithEtag;
        }

        /// <inheritdoc/>
        public void DeleteDoc(string collectionName, string documentId, string partitionKey)
        {
            this.CosmosDbRetryPolicy.ExecuteAction(o =>
            {
                try
                {
                    this.GetCosmosDbClientObject().DeleteDoc(
                        CosmosDbConfiguration.CpimCosmosDatabase,
                        collectionName,
                        documentId,
                        partitionKey);
                }
                catch (Exception ex)
                {
                    HandleCosmosDbStorageException(ex, collectionName, String.Empty, partitionKey);

                    // throw original exception.
                    throw;
                }
            });
        }

        public async Task DeleteDocAsync(string collectionName, string documentId, string partitionKey)
        {
            await this.CosmosDbRetryPolicy.ExecuteActionAsync(
                async (o) => await this.InternalDeleteDocAsync(collectionName, documentId, partitionKey),
                default(CancellationToken));
        }

        public CosmosDbQueryResult<T> GetObject<T>(
            string collectionName,
            string documentId,
            string partitionKey)
        {
            // Object does not exist in off node cache, retrieve it from the Cosmos DB.
            return this.GetObjectWithEtag<T>(collectionName, documentId, partitionKey);
        }

        public void DeleteDocIfExists(string collectionName, string documentId, string partitionKey)
        {
            try
            {
                this.DeleteDoc(collectionName, documentId, partitionKey);
            }
            catch (ResourceNotFoundException)
            {
                // ignore exception.
            }
        }

        public CosmosDBClient GetCosmosDbClientObject()
        {
            return cosmosDbClient.Value;
        }

        /// <summary>
        /// Handles the stroage exception.
        /// </summary>
        /// <param name="ex">Exception detected from the Cosmos DB client.</param>
        /// <param name="collectionName">Collection name</param>
        /// <param name="documentId">The document ID</param>
        /// <param name="partitionId">The partition Id</param>
        internal static void HandleCosmosDbStorageException(
            Exception ex,
            string collectionName,
            string documentId,
            string partitionId)
        {
            CosmosDbStorageExceptionHandler handler = CosmosDbStorageExceptionHandler.ParseException(
                ex,
                collectionName,
                documentId,
                partitionId);

            if (handler.ExceptionType == CosmosDbStorageExceptionHandler.StorageExceptionType.ResourceNotFound)
            {
                throw new ResourceNotFoundException(handler.Message, (Exception)handler.StorageException);
            }
            else if (handler.ExceptionType == CosmosDbStorageExceptionHandler.StorageExceptionType.EtagNotMatched)
            {
                throw new StorageEtagNotMatchException(handler.Message, (Exception)handler.StorageException);
            }
            else if (handler.ExceptionType == CosmosDbStorageExceptionHandler.StorageExceptionType.DuplicatedId)
            {
                throw new DuplicateIdException(handler.Message, (Exception)handler.StorageException);
            }
            else if (handler.ExceptionType == CosmosDbStorageExceptionHandler.StorageExceptionType.StorageException)
            {
                throw handler.StorageException;
            }
        }

        /// <summary>
        /// Gets the collecton description
        /// </summary>
        /// <param name="collectionName">Queue name</param>
        /// <param name="methodName">Method name</param>
        /// <returns>Queue description</returns>
        internal static string GetDescriptionForWriteOperation(string collectionName, string methodName)
        {
            return $"Target collection: {collectionName}. Method name: {methodName}";
        }

        /// <summary>
        /// Gets the collecton description
        /// </summary>
        /// <param name="collectionName">Queue name</param>
        /// <param name="methodName">Method name</param>
        /// <param name="pageNumber">Page number</param>
        /// <returns>Queue description</returns>
        internal static string GetDescriptionForReadOperation(string collectionName, string methodName, int pageNumber)
        {
            return $"Target collection: {collectionName}. Method name: {methodName}. Page number: {pageNumber}";
        }

        /// <summary>
        /// The Cosmos DB request transient exception handler, which is used for retry policy.
        /// </summary>
        /// <param name="ex">The exception.</param>
        /// <param name="retryStateObj">The retry state object.</param>
        internal static void CosmosDbRequestTransientExceptionHandler(Exception ex, object retryStateObj)
        {
            CosmosDbErrorDetectionStrategy transientErrorDetection = new CosmosDbErrorDetectionStrategy();
            if (transientErrorDetection.IsTransient(ex) && retryStateObj != null)
            {
                CosmosDbRequestRetryState retryState = (CosmosDbRequestRetryState)retryStateObj;

                retryState.WillRetry(ex);
            }
        }

        /// <summary>
        /// The Cosmos DB request throttled exception handler, which is used for retry policy.
        /// </summary>
        /// <param name="ex">The exception.</param>
        /// <param name="retryStateObj">The retry state object.</param>
        internal void CosmosDbRequestThrottledExceptionHandler(Exception ex, object retryStateObj)
        {
            CosmosDbThrottleDetectionStrategy throttleDetection = new CosmosDbThrottleDetectionStrategy();
            if (throttleDetection.IsTransient(ex) && retryStateObj != null)
            {
                CosmosDbRequestRetryState retryState = (CosmosDbRequestRetryState)retryStateObj;
                if (!retryState.HasRegionList)
                {
                    List<String> regions = new List<string>();
                    regions.Add("West US");
                    regions.Add("East US");
                    retryState.SetRegionSequenceList(regions);
                }

                retryState.WillRetryNextRegion(ex);
            }
        }

        /// <summary>
        /// Internal implementation to query document from database and returns as a list.
        /// </summary>
        /// <typeparam name="T">The document type</typeparam>
        /// <param name="documentQuery">Document Query</param>
        /// <param name="collectionName">Collection Name</param>
        /// <param name="queryString">query string for the Cosmos DB query used as key for caching</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns>List of queried objects.</returns>
        private List<T> InternalGetDataList<T>(
            CosmosDbQuery<T> documentQuery,
            string collectionName,
            string queryString,
            CancellationToken cancellationToken)
        {
            int pageNumberCount = 0;
            List<T> resultList = new List<T>();
            while (documentQuery.HasMoreResults)
            {
                pageNumberCount++;

                int pageNumberCountCaptured = pageNumberCount++;
                this.CosmosDbRetryPolicy.ExecuteAction((o) =>
                    {
                        try
                        {
                            var feedResponse = documentQuery
                                .FeedIterator
                                .ReadNextAsync(cancellationToken)
                                .GetAwaiter()
                                .GetResult();
                            resultList.AddRange(feedResponse.ToList());
                        }
                        catch (Exception ex)
                        {
                            CosmosDbStorageException.ProcessCosmosDbExceptionThrown(ex);
                        }
                    });
            }

            return resultList;
        }

        /// <summary>
        /// Internal implementation to create/update a document.
        /// </summary>
        /// <typeparam name="T">The document type</typeparam>
        /// <param name="collectionName">Collection name</param>
        /// <param name="data">The data to be written</param>
        /// <param name="partitionId">The partition Id</param>
        /// <param name="documentId">The document ID</param>
        /// <returns>Returns the new etag of the updated document</returns>
        private async Task InternalCreateOrUpdateAsync<T>(
            string collectionName,
            T data,
            string partitionId,
            string documentId)
        {
            try
            {
                await this.GetCosmosDbClientObject().CreateOrUpdateDocAsync(
                    CosmosDbConfiguration.CpimCosmosDatabase,
                    collectionName,
                    data,
                    partitionId);
            }
            catch (Exception ex)
            {
                HandleCosmosDbStorageException(ex, collectionName, documentId, partitionId);

                // throw original exception.
                throw;
            }
        }

        /// <summary>
        /// Internal implementation of document delete.
        /// </summary>
        /// <typeparam name="T">The document type</typeparam>
        /// <param name="collectionName">Collection name</param>
        /// <param name="documentId">The document ID</param>
        /// <param name="partitionKey">The data to be written</param>
        private async Task InternalDeleteDocAsync(
            string collectionName,
            string documentId,
            string partitionKey)
        {
            try
            {
                await this.GetCosmosDbClientObject().DeleteDocAsync(
                    CosmosDbConfiguration.CpimCosmosDatabase,
                    collectionName,
                    documentId,
                    partitionKey);
            }
            catch (Exception ex)
            {
                HandleCosmosDbStorageException(ex, collectionName, documentId, partitionKey);

                // throw original exception.
                throw;
            }
        }
    }
}
