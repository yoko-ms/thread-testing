// ----------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------

using System;
using System.Threading;

namespace TestThreading
{
    public class TaskWaitQueueTest
    {
        private const int TestWaitCount = 100;

        public void Run()
        {
            TaskWaitQueue queue = new TaskWaitQueue();
            queue.Start();

            Random rand = new Random();
            for (int i = 0; i < 100; i++)
            {
                long delay = rand.Next(3000, 6000);
                queue.Enqueue(new TaskWaitInfo(delay));
            }

            while (queue.HasData)
            {
                PrintThreadPoolStatus();

                Thread.Sleep(1000);
            }
        }

        private static void PrintThreadPoolStatus()
        {
            Console.WriteLine($"{DateTime.Now.ToString("h:mm:ss.fff")}, ThreadCount: {ThreadPool.ThreadCount}, Pending: {ThreadPool.PendingWorkItemCount}, Completed: {ThreadPool.CompletedWorkItemCount}");
        }
    }
}
