// ----------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading;

namespace TestThreading
{
    public class TaskWaitQueue
    {
        private const long DefaultDueTime = 500;
        private const long DefaultPeriod = 50;

        /// <summary>
        /// Timer object to execute.
        /// </summary>
        private Timer timer;
        private object lockObject = new object();
        private SortedList<long, List<TaskWaitInfo>> WaitList = new SortedList<long, List<TaskWaitInfo>>();

        public bool HasData => (this.WaitList.Count > 0);

        /// <summary>
        /// Stop the timer.
        /// </summary>
        public void Stop()
        {
            if (this.timer != null)
            {
                this.timer.Dispose();
                this.timer = null;
            }
        }

        /// <summary>
        /// Start the Task wait queue 
        /// </summary>
        /// <param name="dueTime">
        /// The amount of time to delay before the callback is invoked. Specify System.Threading.Timeout.InfiniteTimeSpan
        //  to prevent the timer from starting. Specify System.TimeSpan.Zero to start the
        //  timer immediately.
        /// </param>
        /// <param name="period">
        /// The time interval between invocations of callback. Specify System.Threading.Timeout.InfiniteTimeSpan
        /// to disable periodic signaling.
        /// </param>
        public void Start(long dueTime = DefaultDueTime, long period = DefaultPeriod)
        {
            this.timer = new Timer(this.TimerCheck, null, dueTime, period);
        }

        public void Enqueue(TaskWaitInfo waitInfo)
        {
            lock (lockObject)
            {
                Console.WriteLine($"Enqueue ...{waitInfo.EndTimeTick}");

                List<TaskWaitInfo> waitList;
                if (this.WaitList.ContainsKey(waitInfo.EndTimeTick))
                {
                    if (this.WaitList.TryGetValue(waitInfo.EndTimeTick, out waitList))
                    {
                        waitList.Add(waitInfo);
                        return;
                    }
                }

                waitList = new List<TaskWaitInfo>();
                waitList.Add(waitInfo);
                this.WaitList.Add(waitInfo.EndTimeTick, waitList);
            }
        }

        private void TimerCheck(object stateInfo)
        {
            long currentTick = DateTime.Now.Ticks;
            List<long> keysToRemove = new List<long>();
            List<TaskWaitInfo> waitList;

            lock (lockObject)
            {
                foreach (long timestamp in this.WaitList.Keys)
                {
                    if (timestamp > currentTick)
                    {
                        break;
                    }

                    if (this.WaitList.TryGetValue(timestamp, out waitList))
                    {
                        keysToRemove.Add(timestamp);
                        foreach (TaskWaitInfo info in waitList)
                        {
                            info.OnTimedOut(currentTick);
                        }
                    }
                }

                foreach (long timestamp in keysToRemove)
                {
                    this.WaitList.Remove(timestamp);
                }
            }
        }
    }
}
