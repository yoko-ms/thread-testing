// ----------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------

using System;
using System.Threading;

namespace TestThreading.CosmosDB.TestRunners
{
    /// <summary>
    /// A <see cref="PolicyDocument"/> document handling test runner.
    /// </summary>
    public class TestPolicyDocument : TestRunnerBase
    {
        private const string TestName = "[TestPolicyDocument]";

        public override void OnExecuteTest(int repeatCount)
        {
            Console.WriteLine($"{DateTime.Now.ToString("h:mm:ss.fff")} {TestName} Running {repeatCount}");

            // Prepare policy document
            PolicyDocument policyDocument = new PolicyDocument
            {
                Id = CosmosDbTestUtils.CreateIdForTesting(),
                PolicyId = Guid.NewGuid().ToString(),
                TenantObjectId = Guid.NewGuid().ToString(),
                PolicyContent = "original_content"
            };

            try
            {
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
                    Interlocked.Increment(ref CosmosDbTestUtils.TestRunStatus.NumberOfFailures);
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
                    Interlocked.Increment(ref CosmosDbTestUtils.TestRunStatus.NumberOfFailures);
                    throw new Exception($"{TestName}: Verify PolicyDocument object retrieved from Cosmos DB has updated content.");
                }

                // remove policy object
                CosmosDbTestUtils.CosmosDbDataMapper.Value
                    .DeleteDocAsync(CosmosDbConfiguration.PolicyContainerName, policyDocument.Id, policyDocument.TenantObjectId)
                    .Wait();

                CosmosDbTestUtils.ValidateDocumentNotExist<PolicyDocument>(
                    TestName,
                    CosmosDbConfiguration.PolicyContainerName,
                    policyDocument.Id,
                    policyDocument.TenantObjectId);
            }
            finally
            {
                CosmosDbTestUtils.CosmosDbDataMapper.Value
                    .DeleteDocIfExists(CosmosDbConfiguration.PolicyContainerName, policyDocument.Id, policyDocument.TenantObjectId);
            }

            Console.WriteLine($"{DateTime.Now.ToString("h:mm:ss.fff")} {TestName} Done {repeatCount}");
        }
    }
}
