// ----------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;

namespace TestThreading.CosmosDB.TestRunners
{
    /// <summary>
    /// A <see cref="TenantMetadata"/> document handling test runner.
    /// </summary>
    public class TestCredentialDocument : TestRunnerBase
    {
        private const string TestName = "[TestCredentialDocument]";
        /// <summary>
        /// Certificate tenant id used for tests.
        /// </summary>
        private const string TenantId = @"test-tenant-id";
        /// <summary>
        /// Tenant Id for tests that create many objects.
        /// </summary>
        private const string ManyTenantId = "many.tenant.id";
        /// <summary>
        /// Certificate id used for tests.
        /// </summary>
        private const string SecretId = @"test-secret-id";
        /// <summary>
        /// Content for objects that use base64 representation.
        /// </summary>
        private static readonly string String64Content = Convert.ToBase64String(new byte[] { 1, 2, 3 });

        /// <summary>
        /// Content for objects that use base64 representation.
        /// </summary>
        private static readonly string String64ContentUpdate = Convert.ToBase64String(new byte[] { 4, 5, 6 });

        private Dictionary<string, Tenant> TenantTable = new Dictionary<string, Tenant>();

        public override void OnExecuteTest(int repeatCount)
        {
            Console.WriteLine($"{DateTime.Now.ToString("h:mm:ss.fff")} {TestName} Running {repeatCount}");
            CreateTestTenants();

            CreateRemoveSecret(SecretType.X509Certificate);
            CreateRemoveSecret(SecretType.UProveKey);
            CreateRemoveSecret(SecretType.Secret);
            CreateUpdateRetrieveSecret(SecretType.Secret);

            Console.WriteLine($"{DateTime.Now.ToString("h:mm:ss.fff")} {TestName} Done {repeatCount}");
            Thread.Sleep(100);
        }

        private void CreateUpdateRetrieveSecret(SecretType secretType)
        {
            SaveSecret(TenantId, SecretId, String64Content, secretType);
            UpdateSecret(TenantId, SecretId, String64ContentUpdate, secretType);

            Record record = RetrieveSecret(TenantId, SecretId, secretType);

            if (record == null
                || !String64ContentUpdate.Equals(record.Content)
                || !TenantId.Equals(record.TenantId)
                || !SecretId.Equals(record.Id))
            {
                Interlocked.Increment(ref CosmosDbTestUtils.TestRunStatus.NumberOfFailures);
                throw new Exception($"{TestName} CreateUpdateRetrieveSecret is failed.");
            }
        }

        private void CreateRemoveSecret(SecretType secretType)
        {
            SaveSecret(TenantId, SecretId, String64Content, secretType);
            Record record = RetrieveSecret(TenantId, SecretId, secretType);

            if (record == null)
            {
                Interlocked.Increment(ref CosmosDbTestUtils.TestRunStatus.NumberOfFailures);
                throw new Exception("{TestName} CreateRemoveSecret, secret not found");
            }

            DeleteSecret(TenantId, SecretId, secretType);

            bool isExceptionCatched = false;
            try
            {
                RetrieveSecret(TenantId, SecretId, secretType);
            }
            catch (ResourceNotFoundException)
            {
                isExceptionCatched = true;
            }
            catch (Exception ex)
            {
                Interlocked.Increment(ref CosmosDbTestUtils.TestRunStatus.NumberOfFailures);
                throw new Exception("{TestName} Non expected exception {ex.ToString()}", ex);
            }

            if (!isExceptionCatched)
            {
                Interlocked.Increment(ref CosmosDbTestUtils.TestRunStatus.NumberOfFailures);
                throw new Exception("{TestName} ResourceNotFoundException is not catched.");
            }
        }

        public void SaveSecret(
            string tenantId,
            string secretId,
            string encryptedContent,
            SecretType secretType,
            RecordMetadata metadata = null)
        {
            Tenant tenant = TenantTable[tenantId];

            CredentialDocument credentialDocument = new CredentialDocument()
            {
                Id = CredentialDocument.ComposePrimaryKey(tenant.ObjectId, secretType.ToString(), secretId),
                TenantObjectId = tenant.ObjectId,
                CredentialType = secretType.ToString(),
                CredentialId = secretId,
                EncryptedCredentialContent = encryptedContent,
                Metadata = metadata
            };

            CreateCredential(credentialDocument);
        }

        private Record RetrieveSecret(string tenantId, string secretId, SecretType secretType)
        {
            string documentId = CredentialDocument.ComposePrimaryKey(tenantId, secretType.ToString(), secretId);
            CredentialDocument credentialDocument = null;

            credentialDocument = CosmosDbTestUtils.CosmosDbDataMapper.Value
                .GetObject<CredentialDocument>(
                    CosmosDbConfiguration.CredentialContainerName,
                    documentId,
                    CredentialDocument.FormatPartitionKey(tenantId))
                .DataObject;

            return new Record(tenantId, secretId, credentialDocument.EncryptedCredentialContent, null, credentialDocument.Metadata);
        }

        private IEnumerable<Record> RetrieveAllSecrets(SecretType secretType)
        {
            List<Record> records = new List<Record>();
            Expression<Func<CredentialDocument, bool>> searchPredicateExpression
                = cd => cd.CredentialType == secretType.ToString().ToLowerInvariant();

            // Query credentials in US account
            records.AddRange(CosmosDbTestUtils.CosmosDbDataMapper.Value
                .QueryDb(CosmosDbConfiguration.CredentialContainerName, searchPredicateExpression, CancellationToken.None)
                .Select(cd => new Record(cd.TenantObjectId, cd.CredentialId, cd.EncryptedCredentialContent, null, cd.Metadata)));

            Dictionary<string, Record> distinctRecordDictionary = new Dictionary<string, Record>();
            foreach (Record record in records)
            {
                distinctRecordDictionary[$"{record.TenantId}/{record.Id}"] = record;
            }

            return distinctRecordDictionary.Values;
        }

        private void UpdateSecret(
            string tenantId,
            string secretId,
            string secretAsBase64,
            SecretType secretType,
            RecordMetadata metadata = null)
        {
            Check.NotNullOrWhiteSpace(nameof(tenantId), tenantId);
            Check.NotNullOrWhiteSpace(nameof(secretId), secretId);
            Check.NotNullOrWhiteSpace(nameof(secretAsBase64), secretAsBase64);

            CredentialDocument credentialDocument = new CredentialDocument()
            {
                Id = CredentialDocument.ComposePrimaryKey(tenantId, secretType.ToString(), secretId),
                TenantObjectId = tenantId,
                CredentialType = secretType.ToString(),
                CredentialId = secretId,
                EncryptedCredentialContent = secretAsBase64,
                Metadata = metadata
            };

            CredentialDocument.ValidateCredentialDocument(credentialDocument);

            CosmosDbTestUtils.CosmosDbDataMapper.Value.UpdateDoc(
                CosmosDbConfiguration.CredentialContainerName,
                credentialDocument.Id,
                credentialDocument,
                CredentialDocument.FormatPartitionKey(tenantId),
                null);
        }

        private void DeleteSecret(string tenantId, string secretId, SecretType secretType)
        {
            Check.NotNullOrWhiteSpace(nameof(tenantId), tenantId);
            Check.NotNullOrWhiteSpace(nameof(secretId), secretId);

            string documentId = CredentialDocument.ComposePrimaryKey(
                tenantId,
                secretType.ToString(),
                secretId);

            CosmosDbTestUtils.CosmosDbDataMapper.Value
                .DeleteDoc(
                    CosmosDbConfiguration.CredentialContainerName,
                    documentId,
                    CredentialDocument.FormatPartitionKey(tenantId));
        }

        /// <summary>
        /// Create credential.
        /// </summary>
        /// <param name="credentialDocument">The <see cref="CredentialDocument"/> object.</param>
        private void CreateCredential(CredentialDocument credentialDocument)
        {
            Check.NotNull(nameof(credentialDocument), credentialDocument);
            CredentialDocument.ValidateCredentialDocument(credentialDocument);
            CosmosDbTestUtils.CosmosDbDataMapper.Value
                .CreateDoc(
                    CosmosDbConfiguration.CredentialContainerName,
                    credentialDocument.Id,
                    credentialDocument,
                    CredentialDocument.FormatPartitionKey(credentialDocument.TenantObjectId));
        }

        private void CreateTestTenants()
        {
            Tenant tenant1 = new Tenant()
            {
                ObjectId = TenantId,
                InitialDomainName = "chicago.onmicrosoft.com",
                CountryCode = "US"
            };
            TenantTable.Add(tenant1.ObjectId, tenant1);

            Tenant tenant2 = new Tenant()
            {
                ObjectId = ManyTenantId,
                InitialDomainName = "seattle.onmicrosoft.com",
                CountryCode = "US"
            };
            TenantTable.Add(tenant2.ObjectId, tenant2);

            Tenant tenant3 = new Tenant()
            {
                ObjectId = "a-tenant",
                InitialDomainName = "newyork.onmicrosoft.com",
                CountryCode = "US"
            };
            TenantTable.Add(tenant3.ObjectId, tenant3);

            for (int i = 0; i < 5; i++)
            {
                Tenant tenant = new Tenant()
                {
                    ObjectId = ManyTenantId + i.ToString(),
                    InitialDomainName = i.ToString() + ".onmicrosoft.com",
                    CountryCode = "US"
                };
                TenantTable.Add(tenant.ObjectId, tenant);
            }
        }

        public class Record
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Record"/> class.
            /// </summary>
            /// <param name="tenantId">
            /// The tenant id.
            /// </param>
            /// <param name="id">
            /// The id.
            /// </param>
            /// <param name="content">
            /// The content.
            /// </param>
            /// <param name="etag">
            /// The etag.
            /// </param>
            /// <param name="metadata">
            /// The (optional) record metadata.
            /// </param>
            public Record(string tenantId, string id, string content, string etag = null, RecordMetadata metadata = null)
            {
                Check.NotNullOrWhiteSpace("TenantId", tenantId);
                Check.NotNullOrWhiteSpace("Id", id);
                Check.NotNullOrWhiteSpace("Content", content);

                this.TenantId = tenantId;
                this.Id = id;
                this.Content = content;
                this.Etag = etag;
                this.Metadata = metadata;
            }

            /// <summary>
            /// Gets the tenant id.
            /// </summary>
            public string TenantId { get; private set; }

            /// <summary>
            /// Gets the id.
            /// </summary>
            public string Id { get; private set; }

            /// <summary>
            /// Gets the content.
            /// </summary>
            public string Content { get; private set; }

            /// <summary>
            /// Gets the etag.
            /// </summary>
            public string Etag { get; private set; }

            /// <summary>
            /// Gets the record metadata.
            /// </summary>
            public RecordMetadata Metadata { get; private set; }
        }
    }
}
