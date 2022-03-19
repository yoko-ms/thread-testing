// ----------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------

using System;

namespace TestThreading.CosmosDB.TestRunners
{
    /// <summary>
    /// A <see cref="PolicyDocument"/> document handling test runner.
    /// </summary>
    public class PolicyDocumentReaderTask : TestRunnerBase
    {
        private const string TestName = "[PolicyDocumentReaderTask]";

        public override void OnExecuteTest()
        {
            // Prepare policy document
            PolicyDocument policyDocument = new PolicyDocument
            {
                Id = CosmosDbTestUtils.CreateIdForTesting(),
                PolicyId = Guid.NewGuid().ToString(),
                TenantObjectId = Guid.NewGuid().ToString(),
                PolicyContent = "original_content"
            };

            // Save PolicyDocument to Cosmos DB
            CosmosDbTestUtils.CosmosDbDataMapper.Value.CreateDoc<PolicyDocument>(
                collectionName: CosmosDbConfiguration.PolicyContainerName,
                documentId: policyDocument.Id,
                data: policyDocument,
                partitionId: policyDocument.TenantObjectId);

            // Get policy document from Cosmos DB
            CosmosDbQueryResult<PolicyDocument> policyData = CosmosDbTestUtils.CosmosDbDataMapper.Value
                .GetObjectWithEtag<PolicyDocument>(CosmosDbConfiguration.PolicyContainerName, policyDocument.Id, policyDocument.TenantObjectId);
            if (!"original_content".Equals(policyData.DataObject.PolicyContent, StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception($"{TestName}: Verify PolicyDocument object retrieved from Cosmos DB has original content.");
            }

            // Update the content
            policyDocument.PolicyContent = "updated_content";
            CosmosDbTestUtils.CosmosDbDataMapper.Value.UpdateDoc<PolicyDocument>(
                collectionName: CosmosDbConfiguration.PolicyContainerName,
                documentId: policyDocument.Id,
                data: policyDocument,
                partitionId: policyDocument.TenantObjectId,
                eTag: policyData.Etag);

            // Read back and check updated content.
            policyData = CosmosDbTestUtils.CosmosDbDataMapper.Value.GetObject<PolicyDocument>(
                collectionName: CosmosDbConfiguration.PolicyContainerName,
                documentId: policyDocument.Id,
                partitionKey: policyDocument.TenantObjectId);
            if (!"updated_content".Equals(policyData.DataObject.PolicyContent, StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception($"{TestName}: Verify PolicyDocument object retrieved from Cosmos DB has updated content.");
            }

            // remove policy object
            CosmosDbTestUtils.CosmosDbDataMapper.Value
                .DeleteDocAsync(CosmosDbConfiguration.PolicyContainerName, policyDocument.Id, policyDocument.TenantObjectId)
                .Wait();

            this.ValidateDocumentNotExist<PolicyDocument>(
                CosmosDbConfiguration.PolicyContainerName,
                policyDocument.Id,
                policyDocument.TenantObjectId);
        }

        private void ValidateDocumentNotExist<T>(
            string collectionName,
            string id,
            string partitionKey)
        {
            bool isResourceNotFoundExceptionThrown = false;
            string exceptionMessage = string.Empty;
            try
            {
                CosmosDbTestUtils.CosmosDbDataMapper.Value.GetObjectWithEtag<T>(collectionName, id, partitionKey);
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
                throw new Exception($"{TestName}: ValidateDocumentNotExist failed for document {id}");
            }
        }
    }
}
