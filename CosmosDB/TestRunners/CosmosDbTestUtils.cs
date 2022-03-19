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
        /// A <see cref="ICosmosDbDataMapper"/> instance to use for Cosmos DB interface.
        /// </summary>
        public static Lazy<CosmosDbDataMapper> CosmosDbDataMapper
            = new Lazy<CosmosDbDataMapper>(() => new CosmosDbDataMapper());

        /// <summary>
        /// Random number generator.
        /// </summary>
        private static Random RandGenerator = new Random();

        /// <summary>
        /// Gets or sets the number of running task runners.
        /// </summary>
        private static int TaskRunnerCount = 0;

        /// <summary>
        /// Increment the running task runner count.
        /// </summary>
        /// <returns>Return the increased number.</returns>
        public static int IncrementTaskRunner()
        {
            return Interlocked.Increment(ref TaskRunnerCount);
        }

        /// <summary>
        /// Decrement the running task runner count.
        /// </summary>
        /// <returns>Return the decremented number.</returns>
        public static int DecrementTaskRunner()
        {
            return Interlocked.Decrement(ref TaskRunnerCount);
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
        /// <param name="tenantIdList">List of tenant Id to be removed.</param>
        public static void ClearTenantDetailsAddedForTesting(string collectionName, List<string> tenantIdList)
        {
            tenantIdList
                .ToList()
                .ForEach((x) => CosmosDbDataMapper.Value.DeleteDocIfExists(collectionName, x, String.Empty));
        }
    }
}
