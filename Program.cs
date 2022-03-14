using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace TestThreading
{
    class Program
    {
        private static Random rand = new Random();
        private const int NumberOfTasks = 100;
        private const int WaitTimeInMs = 2000;

        private static int maxConcurrentTasks;

        private static ConcurrentQueue<OperationDiagnostics> resultQueue = new ConcurrentQueue<OperationDiagnostics>();

        static void Main(string[] arg)
        {
            maxConcurrentTasks = 0;
            TestCaseOne();

            Thread.Sleep(1000);

            maxConcurrentTasks = 0;
            TestCaseTwo();
        }

        private static void updateMaxTasks()
        {
            int number = Process.GetCurrentProcess().Threads.Count;
            if (number > maxConcurrentTasks)
            {
                Interlocked.Exchange(ref maxConcurrentTasks, number);
            }
        }

        private static void TestCaseOne()
        {
            Task<OperationDiagnostics>[] tasks = new Task<OperationDiagnostics>[NumberOfTasks];
            for (int i = 0; i < NumberOfTasks; i++)
            {
                tasks[i] = OperationDiagnostics.TraceRequestAsyncAction(
                    async (diagnostic) =>
                    {
                        updateMaxTasks();
                        resultQueue.Enqueue(diagnostic);

                        await Task.Run(() =>
                            {
                                updateMaxTasks();
                                for (int j = 0; j < 10; j++)
                                {
                                    diagnostic.ClientData += string.Format("counting-{0: 3:D2}", j);
                                    Thread.Sleep(50);
                                }
                            });

                        return diagnostic;
                    },
                    string.Format("{0, 3:D2}", i));
                Thread.Sleep(100);
            }

            Task.WaitAll(tasks);

            long avg = 0, maxWait = 0;
            Console.WriteLine("Task with Task.Run()");
            foreach (OperationDiagnostics data in resultQueue)
            {
                avg += data.ElapsedTime;
                maxWait = Math.Max(maxWait, data.ElapsedTime);

                Console.Write($" {data.ElapsedTime}");
            }

            Console.WriteLine($"\nMaxTasks: {maxConcurrentTasks}, Avg: {avg / NumberOfTasks} (ms), maxWait: {maxWait} (ms)");
        }

        private static void TestCaseTwo()
        {
            resultQueue.Clear();

            Task<OperationDiagnostics>[] tasks = new Task<OperationDiagnostics>[NumberOfTasks];
            for (int i = 0; i < NumberOfTasks; i++)
            {
                tasks[i] = OperationDiagnostics.TraceRequestAsyncAction(
                    (diagnostic) =>
                    {
                        updateMaxTasks();
                        resultQueue.Enqueue(diagnostic);

                        for (int j = 0; j < 10; j++)
                        {
                            updateMaxTasks();
                            diagnostic.ClientData += string.Format("counting-{0: 3:D2}", j);
                            Thread.Sleep(50);
                        }

                        return Task.FromResult(diagnostic);
                    },
                    string.Format("{0, 3:D2}", i));
                Thread.Sleep(100);
            }

            Task.WaitAll(tasks);

            long avg = 0, maxWait = 0;
            Console.WriteLine("Task with Task.FromResult()");
            foreach (OperationDiagnostics data in resultQueue)
            {
                avg += data.ElapsedTime;
                maxWait = Math.Max(maxWait, data.ElapsedTime);

                Console.Write($" {data.ElapsedTime}");
            }

            Console.WriteLine($"\nMaxTasks: {maxConcurrentTasks}, Avg: {avg / NumberOfTasks} (ms), maxWait: {maxWait} (ms)");
        }
    }
}
