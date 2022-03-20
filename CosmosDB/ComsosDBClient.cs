// ----------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Newtonsoft.Json.Linq;

using TestThreading.CosmosDB.TestRunners;

namespace TestThreading.CosmosDB
{
    public class CosmosDBClient : IDisposable
    {
        private const int RequestTimeoutInSeconds = 4;
        private const int RetryWaitTimeInSeconds = 1;
        private const int MaxRetryAttempts = 3;
        private const string ApplicationRegion = "West US";
        private const string ClientConsistencyLevel = "Session";

        /// <summary>
        /// Document Db Session client
        /// </summary>
        private readonly CosmosClient cosmosClient;

        private readonly TimeSpan maxRequestWaitTime = TimeSpan.FromSeconds(4);

        /// <summary>
        /// The disposed.
        /// </summary>
        private bool disposed = false;

        /// <summary>
        ///  Initializes a new instance of the <see cref="CosmosDBClient"/> class
        /// </summary>
        /// <remarks>No network calls in this constructor.</remarks>
        public CosmosDBClient()
        {
            CosmosClientOptions cosmosClientOptions = new CosmosClientOptions
            {
                ConnectionMode = ConnectionMode.Direct,
                RequestTimeout = TimeSpan.FromSeconds(RequestTimeoutInSeconds),
                MaxRetryAttemptsOnRateLimitedRequests = MaxRetryAttempts,
                MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(RetryWaitTimeInSeconds)
            };

            // Only primary region is passed in, use Cosmos DB client to generate the secondary regions sequence
            cosmosClientOptions.ApplicationRegion = ApplicationRegion;

            cosmosClientOptions.ConsistencyLevel = ConsistencyLevel.Session;

            this.cosmosClient = new CosmosClient(
                accountEndpoint: CosmosDbTestUtils.CosmosDbServiceEndpoint,
                authKeyOrResourceToken: CosmosDbTestUtils.CosmosDbPrimaryKey);
        }

        /// <summary>
        /// Get document etag synchronously.
        /// </summary>
        /// <typeparam name="T">Data type</param>
        /// <param name="databaseName">Database name</param>
        /// <param name="collectionName">Collection name</param>
        /// <param name="documentId">The document ID</param>
        /// <param name="partitionKey">The partition key</param>
        /// <returns>A tuple of type and doc db request meta-data</returns>
        public virtual Tuple<T, CosmosDbRequestMetadata> GetDocumentWithEtag<T>(
            string databaseName,
            string collectionName,
            string documentId,
            string partitionKey)
        {
            ItemResponse<T> response = CosmosDbClientUtils.RunTaskOnBackgroundThreadSync(
                (ct) => this.cosmosClient
                        .GetContainer(databaseName, collectionName)
                        .ReadItemAsync<T>(id: documentId, partitionKey: CreatePartitionKey(partitionKey), cancellationToken: ct),
                this.maxRequestWaitTime);

            CosmosDbRequestMetadata requestMetadata = this.ComposeDocumentDbRequestMetadata(response);
            T obj = (T)(dynamic)response.Resource;

            return new Tuple<T, CosmosDbRequestMetadata>(obj, requestMetadata);
        }

        public CosmosDbQuery<T> QueryDb<T>(
            string databaseName,
            string collectionName,
            Expression<Func<T, bool>> searchPredicateExpression,
            int degreeOfParallelism,
            int pageSize)
        {
            QueryRequestOptions queryOptions = new QueryRequestOptions
            {
                MaxItemCount = pageSize,
                MaxConcurrency = degreeOfParallelism,
                MaxBufferedItemCount = CosmosDbClientUtils.MaxBufferedItemCount
            };

            var query = this.cosmosClient
                .GetContainer(databaseName, collectionName)
                .GetItemLinqQueryable<T>(requestOptions: queryOptions)
                .Where(searchPredicateExpression);

            return new CosmosDbQuery<T>(query.ToFeedIterator(), JObject.Parse(query.ToString())["query"].ToString());
        }

        public CosmosDbQuery<TP> SelectPropertiesFromAllDocuments<T, TP>(
            string databaseName,
            string collectionName,
            Expression<Func<T, TP>> selectPredicateExpression,
            int degreeOfParallelism,
            int pageSize)
        {
            QueryRequestOptions queryOptions = new QueryRequestOptions
            {
                MaxItemCount = pageSize,
                MaxConcurrency = degreeOfParallelism,
                MaxBufferedItemCount = CosmosDbClientUtils.MaxBufferedItemCount
            };

            var query = this.cosmosClient
                .GetContainer(databaseName, collectionName)
                .GetItemLinqQueryable<T>(requestOptions: queryOptions)
                .Select(selectPredicateExpression);

            return new CosmosDbQuery<TP>(query.ToFeedIterator(), JObject.Parse(query.ToString())["query"].ToString());
        }

        public CosmosDbQuery<TP> SearchAndSelectProperties<T, TP>(
            string databaseName,
            string collectionName,
            Expression<Func<T, bool>> searchPredicateExpression,
            Expression<Func<T, TP>> selectPredicateExpression,
            int degreeOfParallelism,
            int pageSize)
        {
            QueryRequestOptions queryOptions = new QueryRequestOptions
            {
                MaxItemCount = pageSize,
                MaxConcurrency = degreeOfParallelism,
                MaxBufferedItemCount = CosmosDbClientUtils.MaxBufferedItemCount
            };

            var query = this.cosmosClient
                .GetContainer(databaseName, collectionName)
                .GetItemLinqQueryable<T>(requestOptions: queryOptions)
                .Where(searchPredicateExpression)
                .Select(selectPredicateExpression);

            return new CosmosDbQuery<TP>(query.ToFeedIterator(), JObject.Parse(query.ToString())["query"].ToString());
        }

        public CosmosDbQuery<T> QueryWithSql<T>(
            string databaseName,
            string collectionName,
            string sqlQuery,
            int degreeOfParallelism,
            int pageSize)
        {
            QueryRequestOptions queryOptions = new QueryRequestOptions
            {
                MaxItemCount = pageSize,
                MaxConcurrency = degreeOfParallelism,
                MaxBufferedItemCount = CosmosDbClientUtils.MaxBufferedItemCount
            };

            var query = this.cosmosClient
                .GetContainer(databaseName, collectionName)
                .GetItemQueryIterator<T>(
                    queryDefinition: new QueryDefinition(sqlQuery),
                    requestOptions: queryOptions);

            return new CosmosDbQuery<T>(query, sqlQuery);
        }

        public async Task CreateOrUpdateDocAsync<T>(string databaseName, string collectionName, T data, string partitionKey)
        {
            if (string.IsNullOrEmpty(partitionKey))
            {
                await CosmosDbClientUtils.RunTaskAsync(
                    (ct) => this.cosmosClient
                                .GetContainer(databaseName, collectionName)
                                .UpsertItemAsync<T>(item: data, cancellationToken: ct),
                    this.maxRequestWaitTime);
            }
            else
            {
                await CosmosDbClientUtils.RunTaskAsync(
                    (ct) => this.cosmosClient
                                .GetContainer(databaseName, collectionName)
                                .UpsertItemAsync<T>(item: data, partitionKey: new PartitionKey(partitionKey), cancellationToken: ct),
                    this.maxRequestWaitTime);
            }
        }

        public string CreateDoc<T>(string databaseName, string collectionName, T data, string partitionKey)
        {
            ItemResponse<T> response;

            if (string.IsNullOrEmpty(partitionKey))
            {
                response = CosmosDbClientUtils.RunTaskOnBackgroundThreadSync(
                    (ct) => this.cosmosClient
                                .GetContainer(databaseName, collectionName)
                                .CreateItemAsync<T>(
                                    item: data,
                                    cancellationToken: ct),
                    this.maxRequestWaitTime);
            }
            else
            {
                response = CosmosDbClientUtils.RunTaskOnBackgroundThreadSync(
                    (ct) => this.cosmosClient
                                .GetContainer(databaseName, collectionName)
                                .CreateItemAsync<T>(
                                    item: data,
                                    partitionKey: new PartitionKey(partitionKey),
                                    cancellationToken: ct),
                    this.maxRequestWaitTime);
            }

            return response.ETag;
        }

        public async Task<string> ReplaceDocAsync<T>(
            string databaseName,
            string collectionName,
            string documentId,
            T data,
            string partitionKey,
            string eTag)
        {
            ItemResponse<T> response = null;

            ItemRequestOptions requestOptions = new ItemRequestOptions();
            if (eTag != null)
            {
                requestOptions.IfMatchEtag = eTag;
            }

            if (string.IsNullOrEmpty(partitionKey))
            {
                response = await CosmosDbClientUtils.RunTaskAsync(
                    (ct) => this.cosmosClient
                                .GetContainer(databaseName, collectionName)
                                .ReplaceItemAsync<T>(
                                    item: data,
                                    id: documentId,
                                    requestOptions: requestOptions,
                                    cancellationToken: ct),
                    this.maxRequestWaitTime);
            }
            else
            {
                response = await CosmosDbClientUtils.RunTaskAsync(
                    (ct) => this.cosmosClient
                                .GetContainer(databaseName, collectionName)
                                .ReplaceItemAsync<T>(
                                    item: data,
                                    id: documentId,
                                    partitionKey: new PartitionKey(partitionKey),
                                    requestOptions: requestOptions,
                                    cancellationToken: ct),
                    this.maxRequestWaitTime);
            }

            return response.ETag;
        }

        public void DeleteDoc(string databaseName, string collectionName, string id, string partitionKey)
        {
            CosmosDbClientUtils.RunTaskOnBackgroundThreadSync(
                (ct) => this.cosmosClient
                            .GetContainer(databaseName, collectionName)
                            .DeleteItemAsync<dynamic>(
                                id: id,
                                partitionKey: CreatePartitionKey(partitionKey),
                                cancellationToken: ct),
                this.maxRequestWaitTime);
        }

        public async Task DeleteDocAsync(string databaseName, string collectionName, string id, string partitionKey)
        {
            await CosmosDbClientUtils.RunTaskAsync(
                (ct) => this.cosmosClient
                            .GetContainer(databaseName, collectionName)
                            .DeleteItemAsync<dynamic>(
                                id: id,
                                partitionKey: CreatePartitionKey(partitionKey),
                                cancellationToken: ct),
                this.maxRequestWaitTime);
        }

        public CosmosDBClient CreateDatabaseIfNotExists(string databaseId)
        {
            this.cosmosClient
                .CreateDatabaseIfNotExistsAsync(databaseId)
                .Wait();

            return this;
        }

        public CosmosDBClient CreateContainerIfNotExists(string databaseId, string collectionName, string partitionKeyPath)
        {
            this.cosmosClient
                .GetDatabase(databaseId)
                .CreateContainerIfNotExistsAsync(
                    collectionName,
                    string.IsNullOrEmpty(partitionKeyPath) ? PartitionKey.SystemKeyPath : partitionKeyPath)
                .Wait();

            return this;
        }

        public CosmosDBClient DeleteContainer(string databaseId, string collectionName)
        {
            this.cosmosClient.GetContainer(databaseId, collectionName)
                .DeleteContainerAsync()
                .Wait();

            return this;
        }

        /// <summary>
        /// Function to dispose this object.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Function to dispose of this object
        /// </summary>
        /// <param name="disposing">whether the object is being disposed at the moment</param>
        protected virtual void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
                this.cosmosClient.Dispose();
            }

            this.disposed = true;
        }

        /// <summary>
        /// Create a <see cref="PartitionKey"/> object.
        /// </summary>
        /// <param name="partitionKey">Partition key to use.</param>
        /// <returns>A <see cref="PartitionKey"/> object created.</returns>
        private static PartitionKey CreatePartitionKey(string partitionKey)
        {
            return string.IsNullOrEmpty(partitionKey) ? PartitionKey.None : new PartitionKey(partitionKey);
        }

        /// <summary>
        /// Compose DocDb request metadata from response.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <returns>The CosmosDbRequestMetadata.</returns>
        private CosmosDbRequestMetadata ComposeDocumentDbRequestMetadata<T>(ItemResponse<T> response)
        {
            CosmosDbRequestMetadata requestMetadata = new CosmosDbRequestMetadata();

            try
            {
                requestMetadata.ActivityId = response?.ActivityId;
                requestMetadata.Etag = response?.ETag;
                requestMetadata.RequestCharge = response?.RequestCharge;
                requestMetadata.RequestLatency = response?.Diagnostics?.GetClientElapsedTime();
                requestMetadata.StatusCode = response?.StatusCode;
                requestMetadata.RequestDiagnosticsString = response?.Diagnostics.ToString();
            }
            catch (Exception ex)
            {
                requestMetadata.Note = ex.ToString();
            }

            return requestMetadata;
        }
    }
}
