// ----------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------

using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace TestThreading.CosmosDB.TestRunners
{
    /// <summary>
    /// A wrapper class for trust framework policy, which is serialized to JSON document and stored in Document DB.
    /// </summary>
    public class PolicyDocument
    {
        private string originalPolicyContent;

        /// <summary>
        /// Gets or sets the unique identifier of the policy document.
        /// </summary>
        /// <remarks>
        /// This ID is composed by tenant object ID and policy ID
        /// </remarks>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the tenant object id.
        /// </summary>
        [JsonProperty(PropertyName = "tenantObjectId")]
        public string TenantObjectId { get; set; }

        /// <summary>
        /// Gets or sets the policy ID.
        /// </summary>
        [JsonProperty(PropertyName = "policyId")]
        public string PolicyId { get; set; }

        /// <summary>
        /// Gets or sets the policy content.
        /// </summary>
        [JsonProperty(PropertyName = "policyContent")]
        public string PolicyContent { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the policy content is compressed.
        /// </summary>
        [JsonProperty(PropertyName = "isCompressed")]
        public bool IsCompressed { get; private set; }

        /// <summary>
        /// Gets or sets a etag
        /// </summary>
        [JsonProperty(PropertyName = "_etag", NullValueHandling = NullValueHandling.Ignore)]
        public string ETag { get; private set; }

        /// <summary>
        /// Gets or sets the (optional) record metadata
        /// </summary>
        [JsonProperty(PropertyName = "metaData", NullValueHandling = NullValueHandling.Ignore)]
        public RecordMetadata MetaData { get; set; }

        /// <summary>
        /// On serializing, compress PolicyContent if eligible.
        /// </summary>
        /// <param name="context">The context.</param>
        [OnSerializing]
        private void OnSerializing(StreamingContext context)
        {
            if (this.IsEligibleForCompression())
            {
                int originalLength = this.PolicyContent.Length;
                this.originalPolicyContent = this.PolicyContent;

                // Compress
                this.PolicyContent = CompressionUtil.Compress(this.PolicyContent);
                this.IsCompressed = true;
            }
        }

        /// <summary>
        /// On serialized.
        /// </summary>
        /// <param name="context">The context.</param>
        [OnSerialized]
        private void OnSerialized(StreamingContext context)
        {
            if (this.IsCompressed)
            {
                this.PolicyContent = this.originalPolicyContent;
                this.IsCompressed = false;
            }
        }

        /// <summary>
        /// On deserialized, decompress PolicyContent if it is compressed.
        /// </summary>
        /// <param name="context">The context.</param>
        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            if (this.IsEligibleForDecompression())
            {
                int originalLength = this.PolicyContent.Length;

                // Decompress
                this.PolicyContent = CompressionUtil.Decompress(this.PolicyContent);
                this.IsCompressed = false;
            }
        }

        /// <summary>
        /// Check if eligible for compression.
        /// </summary>
        /// <returns>True if eligible, false otherwise.</returns>
        private bool IsEligibleForCompression()
        {
            return true;;
        }

        /// <summary>
        /// Check if eligible for decompression.
        /// </summary>
        /// <returns>True if eligible, false otherwise.</returns>
        private bool IsEligibleForDecompression()
        {
            return true;
        }

        /// <summary>
        /// Validate and format policy document.
        /// </summary>
        /// <param name="policyDocument">The policy document object.</param>
        public static void ValidatePolicyDocument(PolicyDocument policyDocument)
        {
            Check.NotNull(nameof(policyDocument), policyDocument);
            Check.NotNullOrWhiteSpace(nameof(policyDocument.Id), policyDocument.Id);
            Check.NotNullOrWhiteSpace(nameof(policyDocument.TenantObjectId), policyDocument.TenantObjectId);
            Check.NotNullOrWhiteSpace(nameof(policyDocument.PolicyId), policyDocument.PolicyId);
            Check.NotNullOrWhiteSpace(nameof(policyDocument.PolicyContent), policyDocument.PolicyContent);

            policyDocument.Id = policyDocument.Id.ToLowerInvariant();
            policyDocument.TenantObjectId = policyDocument.TenantObjectId.ToLowerInvariant();
            policyDocument.PolicyId = policyDocument.PolicyId.ToUpperInvariant();
        }

        /// <summary>
        /// Compose primary key.
        /// </summary>
        /// <param name="tenantObjectId">The tenant ID.</param>
        /// <param name="policyId">The policy ID.</param>
        /// <returns>The primary key</returns>
        public static string ComposePrimaryKey(string tenantObjectId, string policyId)
        {
            // Data validation
            Check.NotNullOrWhiteSpace(nameof(tenantObjectId), tenantObjectId);
            Check.NotNullOrWhiteSpace(nameof(policyId), policyId);

            // Cosmos DB does not allow "/\?&#" characters in ID, so we are using '.' here.
            return $"{tenantObjectId.Trim()}.{policyId.Trim()}".ToLowerInvariant();
        }

        /// <summary>
        /// Format partition key.
        /// </summary>
        /// <param name="tenantObjectId">The tenant object ID.</param>
        /// <returns>The formatted partition key.</returns>
        public static string FormatPartitionKey(string tenantObjectId)
        {
            // Data validation
            Check.NotNullOrWhiteSpace(nameof(tenantObjectId), tenantObjectId);

            return $"{tenantObjectId.Trim().ToLowerInvariant()}";
        }
    }
}
