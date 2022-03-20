// ----------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------

using System.Collections.Generic;

using Newtonsoft.Json;

namespace TestThreading.CosmosDB.TestRunners
{
    /// <summary>
    /// A wrapper class for credential, which is serialized to JSON document and stored in Document DB.
    /// </summary>
    public class CredentialDocument
    {
        /// <summary>
        /// Gets or sets the unique identifier of the credential document.
        /// </summary>
        /// <remarks>
        /// This ID is composed by tenant object ID, credential type, and credential ID
        /// </remarks>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the tenant object id.
        /// </summary>
        [JsonProperty(PropertyName = "tenantObjectId")]
        public string TenantObjectId { get; set; }

        /// <summary>
        /// Gets or sets the credential type.
        /// </summary>
        [JsonProperty(PropertyName = "credentialType")]
        public string CredentialType { get; set; }

        /// <summary>
        /// Gets or sets the credential ID.
        /// </summary>
        [JsonProperty(PropertyName = "credentialId")]
        public string CredentialId { get; set; }

        /// <summary>
        /// Gets or sets the encrypted credential content.
        /// </summary>
        [JsonProperty(PropertyName = "encryptedCredentialContent")]
        public string EncryptedCredentialContent { get; set; }

        /// <summary>
        /// Gets or sets the metadata
        /// </summary>
        [JsonProperty(PropertyName = "metadata", NullValueHandling = NullValueHandling.Ignore)]
        public RecordMetadata Metadata { get; set; }

        /// <summary>
        /// Validate credential document.
        /// </summary>
        /// <param name="credentialDocument">The credential document object.</param>
        public static void ValidateCredentialDocument(CredentialDocument credentialDocument)
        {
            Check.NotNull(nameof(credentialDocument), credentialDocument);
            Check.NotNullOrWhiteSpace(nameof(credentialDocument.Id), credentialDocument.Id);
            Check.NotNullOrWhiteSpace(nameof(credentialDocument.TenantObjectId), credentialDocument.TenantObjectId);
            Check.NotNullOrWhiteSpace(nameof(credentialDocument.CredentialType), credentialDocument.CredentialType);
            Check.NotNullOrWhiteSpace(nameof(credentialDocument.CredentialId), credentialDocument.CredentialId);
            Check.NotNullOrWhiteSpace(nameof(credentialDocument.EncryptedCredentialContent), credentialDocument.EncryptedCredentialContent);

            credentialDocument.Id = credentialDocument.Id.ToLowerInvariant();
            credentialDocument.CredentialType = credentialDocument.CredentialType.ToLowerInvariant();
            credentialDocument.TenantObjectId = credentialDocument.TenantObjectId.ToLowerInvariant();
        }

        /// <summary>
        /// Compose primary key.
        /// </summary>
        /// <param name="tenantObjectId">The tenant ID.</param>
        /// <param name="credentialType">The credential type.</param>
        /// <param name="credentialId">The credential ID.</param>
        /// <returns>The primary key</returns>
        public static string ComposePrimaryKey(string tenantObjectId, string credentialType, string credentialId)
        {
            // Data validation
            Check.NotNullOrWhiteSpace(nameof(tenantObjectId), tenantObjectId);
            Check.NotNullOrWhiteSpace(nameof(credentialType), credentialType);
            Check.NotNullOrWhiteSpace(nameof(credentialId), credentialId);

            // Cosmos DB does not allow "/\?&#" characters in ID, so we are using '.' here.
            return $"{tenantObjectId.Trim()}.{credentialType.Trim()}.{credentialId.Trim()}".ToLowerInvariant();
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
