// ----------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;

namespace TestThreading.CosmosDB.TestRunners
{
    /// <summary>
    /// A QueryDB API test runner
    /// </summary>
    public class TestQueryDb : TestRunnerBase
    {
        private const string TestName = "[TestQueryDb]";

        public override void OnExecuteTest(int repeatCount)
        {
            Console.WriteLine($"{TestName} Running {repeatCount}");

            // populate items
            int items = CosmosDbTestUtils.RandGenerator.Next(5, 20);
            Dictionary<string, Guid> storedDetails = CosmosDbTestUtils.AddTenantDetailsForTesting(items, CosmosDbConfiguration.TestContainerName);

            try
            {
                // read back and verify
                Expression<Func<TenantDetails, bool>> selectPredicateExpression = rp => true;
                IEnumerable<TenantDetails> retVal = CosmosDbTestUtils.CosmosDbDataMapper.Value
                    .QueryDb(
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

            Console.WriteLine($"{TestName} Done {repeatCount}");
        }
    }
}
