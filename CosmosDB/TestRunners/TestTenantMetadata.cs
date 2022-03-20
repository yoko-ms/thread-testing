// ----------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading;

using Newtonsoft.Json.Linq;

namespace TestThreading.CosmosDB.TestRunners
{
    /// <summary>
    /// A <see cref="TenantMetadata"/> document handling test runner.
    /// </summary>
    public class TestTenantMetadata : TestRunnerBase
    {
        private const string TestName = "[TestTenantMetadata]";

        public override void OnExecuteTest(int repeatCount)
        {
            Console.WriteLine($"{DateTime.Now.ToString("h:mm:ss.fff")} {TestName} Running {repeatCount}");

            string tenantDomainName_1 = "contoso-" + CosmosDbTestUtils.CreateIdForTesting();
            TenantMetadata tenantMetadata = new TenantMetadata
            {
                Id = CosmosDbTestUtils.CreateIdForTesting(),
                InitialDomainHint = tenantDomainName_1
            };

            try
            {
                CosmosDbTestUtils.CosmosDbDataMapper
                    .Value
                    .CreateOrUpdateDocAsync<TenantMetadata>(
                        collectionName: CosmosDbConfiguration.TenantMetadataContainerName,
                        data: tenantMetadata,
                        partitionId: tenantMetadata.Id,
                        documentId: null)
                    .Wait();

                // Call GetObject method, since cache does not have it, it will retrieve from Cosmos DB, then back fill the cache
                TenantMetadata objInCosmosDb = CosmosDbTestUtils.CosmosDbDataMapper.Value
                    .GetObject<TenantMetadata>(
                        CosmosDbConfiguration.TenantMetadataContainerName,
                        tenantMetadata.Id,
                        tenantMetadata.Id)
                    .DataObject;
                if (objInCosmosDb == null)
                {
                    Interlocked.Increment(ref CosmosDbTestUtils.TestRunStatus.NumberOfFailures);
                    throw new ResourceNotFoundException($"{TestName}: {tenantMetadata.Id} with GetObject() failed");
                }

                // Update the data in Cosmos DB -  Now the tenant metadata record in Cosmos DB has a setting
                CreateOrUpdateTenantSetting(
                    tenantMetadata.Id,
                    "EmailTypeInputBox",
                    "TEST-*-*-*",
                    true);

                // Call GetObjectWithEtag method to retrieve the record directly from Cosmos DB to verify the update is there
                objInCosmosDb = CosmosDbTestUtils.CosmosDbDataMapper.Value
                    .GetObjectWithEtag<TenantMetadata>(
                        CosmosDbConfiguration.TenantMetadataContainerName,
                        tenantMetadata.Id,
                        tenantMetadata.Id)
                    .DataObject;
                if (objInCosmosDb == null)
                {
                    Interlocked.Increment(ref CosmosDbTestUtils.TestRunStatus.NumberOfFailures);
                    throw new ResourceNotFoundException($"{TestName}: {tenantMetadata.Id} with GetObjectWithEtag failed");
                }

                if (objInCosmosDb.Settings == null)
                {
                    Interlocked.Increment(ref CosmosDbTestUtils.TestRunStatus.NumberOfFailures);
                    throw new ResourceNotFoundException($"{TestName}: Document should have settings, but failed");
                }

                CosmosDbTestUtils.CosmosDbDataMapper.Value
                       .DeleteDocAsync(CosmosDbConfiguration.TenantMetadataContainerName, tenantMetadata.Id, tenantMetadata.Id)
                       .Wait();

                CosmosDbTestUtils.ValidateDocumentNotExist<PolicyDocument>(
                    TestName,
                    CosmosDbConfiguration.TenantMetadataContainerName,
                    tenantMetadata.Id,
                    tenantMetadata.Id);
            }
            finally
            {
                CosmosDbTestUtils.CosmosDbDataMapper.Value
                    .DeleteDocIfExists(CosmosDbConfiguration.TenantMetadataContainerName, tenantMetadata.Id, tenantMetadata.Id);
            }

            Console.WriteLine($"{DateTime.Now.ToString("h:mm:ss.fff")} {TestName} Done {repeatCount}");
        }

        /// <summary>
        /// Create or update tenant setting.
        /// </summary>
        /// <param name="tenantObjectId">The tenant object ID.</param>
        /// <param name="settingName">The setting name string.</param>
        /// <param name="environment">The environment.</param>
        /// <param name="settingValue">The setting value.</param>
        private void CreateOrUpdateTenantSetting(string tenantObjectId, string settingName, string environment, object settingValue)
        {
            // Data validation
            Check.NotNullOrWhiteSpace(@"tenantObjectId", tenantObjectId);
            Check.NotNull(@"settingValue", settingValue);
            Check.NotNullOrWhiteSpace(@"environment", environment);

            Check.NotNullOrWhiteSpace(@"tenantObjectId", tenantObjectId);
            Check.NotNull(@"settingValue", settingValue);
            Check.NotNullOrWhiteSpace(@"settingName", settingName);
            Check.NotNullOrWhiteSpace(@"environment", environment);
            tenantObjectId = tenantObjectId.ToUpperInvariant();
            environment = environment.ToUpperInvariant();

            // Get setting value type
            Type settingValueType = settingValue.GetType();

            // Check whether setting value is a primitive type
            bool isPrimitiveType = settingValueType.IsPrimitive
                || settingValueType.IsValueType
                || (settingValueType == typeof(string));

            // Setting value type validation
            if (isPrimitiveType || (settingValue is JToken))
            {
                // Good
            }
            else
            {
                // Throws
                throw new ArgumentException("settingValue: Primitive, JToken (custom object)");
            }

            TenantMetadata tenantMetadata = this.RetrieveTenantMetadataFromDocDb(tenantObjectId)?.Item;

            // Validate tenant metadata exist
            if (tenantMetadata == null)
            {
                // Throws
                throw new ResourceNotFoundException($"tenantObjectId: {tenantObjectId} not found");
            }

            if (tenantMetadata.Settings == null)
            {
                tenantMetadata.Settings = new Dictionary<string, Dictionary<string, object>>();
            }

            // The env to value dictionary
            Dictionary<string, object> envSettings;
            if (tenantMetadata.Settings.TryGetValue(settingName, out envSettings))
            {
                if (envSettings == null)
                {
                    tenantMetadata.Settings[settingName] = new Dictionary<string, object>();
                    envSettings = tenantMetadata.Settings[settingName];
                }

                envSettings[environment] = settingValue;
            }
            else
            {
                tenantMetadata.Settings[settingName] = new Dictionary<string, object> { { environment, settingValue } };
            }

            // Commit
            this.CreateOrUpdateTenantMetadata(tenantMetadata);
        }

        /// <summary>
        /// Create or update tenant metadata.
        /// </summary>
        /// <param name="tenantMetadata">The <see cref="TenantMetadata"/> object.</param>
        private void CreateOrUpdateTenantMetadata(TenantMetadata tenantMetadata)
        {
            tenantMetadata.Id = tenantMetadata.Id.ToUpperInvariant();
            CosmosDbTestUtils.CosmosDbDataMapper.Value
                .CreateOrUpdateDocAsync(
                    collectionName: CosmosDbConfiguration.TenantMetadataContainerName,
                    data: tenantMetadata,
                    partitionId: tenantMetadata.Id,
                    documentId: tenantMetadata.Id)
                .Wait();
        }

        private NullableCachedItem<TenantMetadata> RetrieveTenantMetadataFromDocDb(string tenantObjectId)
        {
            Check.NotNullOrWhiteSpace(@"tenantObjectId", tenantObjectId);
            tenantObjectId = tenantObjectId.ToUpperInvariant();
            TenantMetadata tenantMetadata;
            NullableCachedItem<TenantMetadata> nullableCachedItemTenantMetadata;
            try
            {
                tenantMetadata = CosmosDbTestUtils.CosmosDbDataMapper.Value
                    .GetObjectWithEtag<TenantMetadata>(
                        CosmosDbConfiguration.TenantMetadataContainerName,
                        tenantObjectId,
                        tenantObjectId)
                    .DataObject;
                nullableCachedItemTenantMetadata = new NullableCachedItem<TenantMetadata>(tenantMetadata);
            }
            catch (ResourceNotFoundException)
            {
                nullableCachedItemTenantMetadata = new NullableCachedItem<TenantMetadata>(null);
            }

            return nullableCachedItemTenantMetadata;
        }
    }
}
