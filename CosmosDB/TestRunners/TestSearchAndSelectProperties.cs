// ----------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;

using Newtonsoft.Json.Linq;

namespace TestThreading.CosmosDB.TestRunners
{
    /// <summary>
    /// Search and Select properties API tester
    /// </summary>
    public class TestSearchAndSelectProperties : TestRunnerBase
    {
        private const string TestName = "[TestSearchAndSelectProperties]";

        public override void OnExecuteTest(int repeatCount)
        {
            Console.WriteLine($"{DateTime.Now.ToString("h:mm:ss.fff")} {TestName} Running {repeatCount}");

            OnSearchAnSelectProperties();
            OnSelectPropertiesFromAllDocuments();

            Console.WriteLine($"{DateTime.Now.ToString("h:mm:ss.fff")} {TestName} Done {repeatCount}");
        }

        private void OnSelectPropertiesFromAllDocuments()
        {
            int items = CosmosDbTestUtils.RandGenerator.Next(5, 20);
            Dictionary<string, Guid> storedDetails = CosmosDbTestUtils.AddTenantDetailsForTesting(items, CosmosDbConfiguration.TestContainerName);

            try
            {
                Expression<Func<TenantDetails, TenantDetails>> selectPredicateExpression = rp => rp;
                IEnumerable<TenantDetails> retVal = CosmosDbTestUtils.CosmosDbDataMapper.Value.SelectPropertiesFromAllDocuments(
                    CosmosDbConfiguration.TestContainerName,
                    selectPredicateExpression,
                    CancellationToken.None);

                List<string> idsToRemove = new List<string>();
                foreach (TenantDetails detail in retVal)
                {
                    if (storedDetails.ContainsKey(detail.TenantLocation))
                    {
                        if (!storedDetails[detail.TenantLocation].Equals(detail.TenantId))
                        {
                            Interlocked.Increment(ref CosmosDbTestUtils.TestRunStatus.NumberOfFailures);
                            throw new Exception($"{TestName} Id is not matched ${storedDetails[detail.TenantLocation]} vs ${detail.TenantId}.");
                        }

                        idsToRemove.Add(detail.Id);
                    }
                }

                if (storedDetails.Count == idsToRemove.Count)
                {
                    Interlocked.Increment(ref CosmosDbTestUtils.TestRunStatus.NumberOfFailures);
                    throw new Exception($"{TestName} There's remained documents unchecked.");
                }
            }
            finally
            {
                CosmosDbTestUtils.ClearTenantDetailsAddedForTesting(CosmosDbConfiguration.TestContainerName, storedDetails);
            }
        }

        private void OnSearchAnSelectProperties()
        {
            PolicyDocument policyDocument = new PolicyDocument
            {
                Id = CosmosDbTestUtils.CreateIdForTesting(),
                PolicyId = CosmosDbTestUtils.CreateIdForTesting(),
                TenantObjectId = "tenant_object_id_" + CosmosDbTestUtils.CreateIdForTesting(),
                PolicyContent = "original_content"
            };

            try
            {
                CosmosDbTestUtils.CosmosDbDataMapper.Value
                    .CreateOrUpdateDocAsync<PolicyDocument>(
                        collectionName: CosmosDbConfiguration.PolicyContainerName,
                        documentId: policyDocument.Id,
                        data: policyDocument,
                        partitionId: policyDocument.TenantObjectId)
                    .Wait();

                Expression<Func<PolicyDocument, bool>> searchPredicateExpression = pd => pd.TenantObjectId == policyDocument.TenantObjectId.ToLowerInvariant();
                Expression<Func<PolicyDocument, string>> selectPredicateExpression = pd => pd.PolicyId;

                IEnumerable<string> policyIdList = CosmosDbTestUtils.CosmosDbDataMapper.Value
                    .SearchAndSelectProperties<PolicyDocument, string>(
                        CosmosDbConfiguration.PolicyContainerName,
                        searchPredicateExpression,
                        selectPredicateExpression,
                        CancellationToken.None);

                if (policyIdList.Count() != 1
                    || policyDocument.PolicyId.Equals(policyIdList.First(), StringComparison.OrdinalIgnoreCase))
                {
                    Interlocked.Increment(ref CosmosDbTestUtils.TestRunStatus.NumberOfFailures);
                    throw new Exception($"{TestName} Count {policyIdList.Count()}, {policyDocument.PolicyId} vs {policyIdList.First()} mismatched.");
                }
            }
            finally
            {
                try
                {
                    CosmosDbTestUtils.CosmosDbDataMapper.Value
                        .DeleteDocAsync(CosmosDbConfiguration.PolicyContainerName, policyDocument.Id, policyDocument.TenantObjectId)
                        .Wait();
                }
                catch (ResourceNotFoundException)
                {
                    // ignore not found exception.
                }
            }
        }
    }
}
