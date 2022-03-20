// ----------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------

using TestThreading.CosmosDB.TestRunners;

namespace TestThreading
{
    class Program
    {
        static void Main(string[] arg)
        {
            CosmosDbTestRunner runner = new CosmosDbTestRunner();
            runner.Run();
        }
    }
}
