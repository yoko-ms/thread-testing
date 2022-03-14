// ----------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace TestThreading
{
    public class OperationDiagnostics
    {
        private Stopwatch stopWatcher;

        public string Name { get; set; }
        public string Message { get; set; }
        public string ResultSignature { get; set; }
        public long ElapsedTime { get; set; }
        public string ClientData { get; set; }

        public void StartTimer()
        {
            this.stopWatcher = new Stopwatch();
            this.stopWatcher.Start();
        }

        public void StopTimer()
        {
            this.stopWatcher.Stop();
            this.ElapsedTime = this.stopWatcher.ElapsedMilliseconds;
        }

        /// <summary>
        /// Delegate for action with latency tracking
        /// </summary>
        /// <param name="diagnostics">Operation Diagnostics object</param>
        public delegate void TrackLatencyAction(ref OperationDiagnostics diagnostics);

        /// <summary>
        /// Delegate for async action with latency tracking
        /// </summary>
        /// <returns>Modified Diagnostics</returns>
        /// <param name="diagnostics">Operation Diagnostics object</param>
        public delegate Task<OperationDiagnostics> TrackLatencyAsyncAction(OperationDiagnostics diagnostics);

        /// <summary>
        /// Helper for logging latency metrics and Ifx events
        /// </summary>
        /// <param name="latencyAction">Traced operation</param>
        /// <param name="name">Name of operation.</param>
        /// <returns>Traced event</returns>
        public static OperationDiagnostics TraceRequestAction(TrackLatencyAction latencyAction, string name)
        {
            OperationDiagnostics operation = new OperationDiagnostics();
            operation.Name = name;
            operation.StartTimer();

            try
            {
                latencyAction(ref operation);
                operation.Message = "None";
                operation.ResultSignature = "OK";
            }
            catch (Exception ex)
            {
                operation.Message = ex.ToString();
                operation.ResultSignature = "Failed";
            }

            operation.StopTimer();
            return operation;
        }

        /// <summary>
        /// Helper for logging latency metrics and Ifx events
        /// </summary>
        /// <param name="latencyAction">Traced operation</param>
        /// <param name="name">Name of operation.</param>
        /// <returns>Traced event</returns>
        public static async Task<OperationDiagnostics> TraceRequestAsyncAction(TrackLatencyAsyncAction latencyAction, string name)
        {
            OperationDiagnostics operation = new OperationDiagnostics();
            operation.StartTimer();
            operation.Name = name;

            try
            {
                operation = await latencyAction(operation);
                operation.Message = "None";
                operation.ResultSignature = "OK";
            }
            catch (Exception ex)
            {
                operation.Message = ex.ToString();
                operation.ResultSignature = "Failed";
            }

            operation.StopTimer();
            return operation;
        }

    }
}
