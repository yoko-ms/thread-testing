// ----------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace TestThreading.CosmosDB.TestRunners
{
    public static class CosmosDbTestUtils
    {
        /// <summary>
        /// Local CosmosDB URI
        /// </summary>
        public const string CosmosDbServiceEndpoint = "https://localhost:8081";

        /// <summary>
        /// Local CosmosDB emulator primary key.
        /// </summary>
        public const string CosmosDbPrimaryKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

        /// <summary>
        /// Event to signal the end of event.
        /// </summary>
        public static ManualResetEvent TestStopEvent = new ManualResetEvent(false);

        /// <summary>
        /// A <see cref="ICosmosDbDataMapper"/> instance to use for Cosmos DB interface.
        /// </summary>
        public static Lazy<CosmosDbDataMapper> CosmosDbDataMapper
            = new Lazy<CosmosDbDataMapper>(() => new CosmosDbDataMapper());

        /// <summary>
        /// Random number generator.
        /// </summary>
        internal static Random RandGenerator = new Random();

        /// <summary>
        /// Current test running status.
        /// </summary>
        private static TestRunStatus testRunStatus = new TestRunStatus();

        /// <summary>
        /// Gets the current test run status information.
        /// </summary>
        /// <returns></returns>
        public static TestRunStatus TestRunStatus
        {
            get
            {
                return testRunStatus;
            }
        }

        public static void DelayTask(int minDelay = 500, int maxDelay = 5000)
        {
            int delay = RandGenerator.Next(minDelay, maxDelay);
            Thread.Sleep(delay);
        }

        // <summary>
        /// Create a new ID for testring.
        /// </summary>
        /// <returns>ID value to use.</returns>
        public static string CreateIdForTesting()
        {
            return Guid.NewGuid().ToString().ToUpperInvariant();
        }

        /// <summary>
        /// Initialize the Cosmos DB for testing.
        /// </summary>
        public static CosmosDBClient InitializeCosmosDbForTesting()
        {
            return CosmosDbDataMapper.Value.GetCosmosDbClientObject().CreateDatabaseIfNotExists(CosmosDbConfiguration.CpimCosmosDatabase);
        }

        /// <summary>
        /// Populate <see cref="TenantDetails"/> for testing.
        /// </summary>
        /// <param name="numberOfItems">Number of items to add.</param>
        /// <param name="collectionName">Collection name.</param>
        /// <returns>A dictionary with added <see cref="TenantDetails"/> documents.</returns>
        public static Dictionary<string, Guid> AddTenantDetailsForTesting(int numberOfItems, string collectionName)
        {
            Dictionary<string, Guid> storedDetails = new Dictionary<string, Guid>();
            for (int i = 0; i < numberOfItems; i++)
            {
                string location = CreateIdForTesting();
                Guid guid = Guid.NewGuid();
                storedDetails.Add(location, guid);
                CosmosDbDataMapper
                    .Value
                    .CreateOrUpdateDocAsync<TenantDetails>(
                        collectionName,
                        new TenantDetails(guid, location),
                        String.Empty,
                        String.Empty)
                    .Wait();
            }

            return storedDetails;
        }

        /// <summary>
        /// Remove given list of <see cref="TenantDetails"/> test data.
        /// </summary>
        /// <param name="collectionName">Collection name.</param>
        /// <param name="tenantIdMap">Table of tenant Id to be removed.</param>
        public static void ClearTenantDetailsAddedForTesting(string collectionName, Dictionary<string, Guid> tenantIdMap)
        {
            tenantIdMap
                .ToList()
                .ForEach((x) => CosmosDbDataMapper.Value.DeleteDocIfExists(collectionName, x.Key, String.Empty));
        }

        public static void ValidateDocumentNotExist<T>(
            string testName,
            string collectionName,
            string id,
            string partitionKey)
        {
            bool isResourceNotFoundExceptionThrown = false;
            string exceptionMessage = string.Empty;
            try
            {
                CosmosDbDataMapper.Value.GetObjectWithEtag<T>(collectionName, id, partitionKey);
            }
            catch (ResourceNotFoundException ex)
            {
                isResourceNotFoundExceptionThrown = true;
                exceptionMessage = ex.Message;
            }

            if (!isResourceNotFoundExceptionThrown
                || !exceptionMessage.Contains(
                        "document not found, collectionName: {collectionName}, documentId: {id}, partitionKey: {partitionKey}",
                        StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception($"{testName}: ValidateDocumentNotExist failed for document {id}");
            }
        }
    }
}
