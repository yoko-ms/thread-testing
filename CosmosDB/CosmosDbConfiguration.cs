// ----------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------

namespace TestThreading.CosmosDB
{
    /// <summary>
    /// Provides constant configurations for the Cosmos DB usage.
    /// </summary>
    public static class CosmosDbConfiguration
    {
        /// <summary>
        /// Cosmos database name for CPIM.
        /// </summary>
        public const string CpimCosmosDatabase = "cpimdocdb";

        /// <summary>
        /// PolicyDocument container name..
        /// </summary>
        public const string PolicyContainerName = "Policy";

        /// <summary>
        /// PartitionKey for Policy container name..
        /// </summary>
        public const string PolicyContainerPartitionKeyPath = "/tenantObjectId";

        /// <summary>
        /// TenantMetadata container name. name.
        /// </summary>
        public const string TenantMetadataContainerName = "TenantMetadata";

        /// <summary>
        /// PartitonKey path for TenantMetadata container name..
        /// </summary>
        public const string TenantMetadataContainerPartitionKeyPath = "/id";

        /// <summary>
        /// Credential/Secrets container name. name.
        /// </summary>
        public const string CredentialContainerName = "Credential";

        /// <summary>
        /// PartitionKey path for Credential container name..
        /// </summary>
        public const string CredentialContainerPartitionKeyPath = "/tenantObjectId";

        /// <summary>
        /// DeploymentConfig container name. name.
        /// </summary>
        public const string DeploymentConfigContainerName = "DeploymentConfig";

        /// <summary>
        /// PerRolloutDeploymentConfig container name. name
        /// </summary>
        public const string PerRolloutDeploymentConfigContainerName = "PerRolloutDeploymentConfig";

        /// <summary>
        /// RolloutSpec container name. name
        /// </summary>
        public const string RolloutSpecContainerName = "RolloutSpec";

        /// <summary>
        /// DeploymentRolloutStatus container name. name
        /// </summary>
        public const string RolloutStatusContainerName = "DeploymentRolloutStatus";

        /// <summary>
        /// KustoQueryConfig container name. name
        /// </summary>
        public const string KustoQueryConfigContainerName = "KustoQueryConfig";

        /// <summary>
        /// BillingConfig container name. name
        /// </summary>
        public const string BillingDeploymentContainerName = "BillingConfigCollection";

        /// <summary>
        /// B2C resource container name. name
        /// </summary>
        public const string B2CResourceContainerName = "b2cresources";

        /// <summary>
        /// PartitionKey path for B2C resource container name..
        /// </summary>
        public const string B2CResourceContainerPartitionKeyPath = "/id";

        /// <summary>
        /// Test container name.
        /// </summary>
        public const string TestContainerName = "testCollection";

        /// <summary>
        /// Key string for <see cref="ICosmosDbDataMapper"/> instance stored in the <see cref="CallContext"/> object.
        /// </summary>
        public const string CosmosDBDataMapperUtilCacheId = "DataMapperUtilDocDb";

        /// <summary>
        /// Endpoint uri for Comsos DB
        /// </summary>
        public const string CosmosDBEndpointConfig = "DocDbEndpointConfig";

        /// <summary>
        /// Endpoint uri for the Cosmos DB.
        /// </summary>
        public const string CosmosDbEndpointSetting = "DocDbEndpoint";

        /// <summary>
        /// Local comsos DB emulator endpoint uri.
        /// </summary>
        public const string LocalCosmosDbEndpointUri = "https://localhost:8081";
    }
}
