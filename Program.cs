// ----------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------

namespace TestThreading
{
    class Program
    {
        static void Main(string[] arg)
        {
            TaskWaitQueueTest tester = new TaskWaitQueueTest();
            tester.Run();
        }
    }
}
