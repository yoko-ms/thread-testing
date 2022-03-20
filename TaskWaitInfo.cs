// ----------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------

using System;
using System.Threading;

namespace TestThreading
{
    public class TaskWaitInfo : IComparable
    {
        /// <summary>
        /// Timer ticks when this item should be timed out.
        /// </summary>
        public long EndTimeTick { get; set; }

        /// <summary>
        /// An event should be set when timed out, if exists.
        /// </summary>
        private ManualResetEvent TaskResetEvent;

        /// <summary>
        /// Time ticke when this event registered.
        /// </summary>
        private long regTime;
        private long requestedDelayTime;

        /// <summary>
        /// Initializes object.
        /// </summary>
        /// <param name="timesToWaitInMs">Milliseconds to wait until timed out.</param>
        /// <param name="resetEvent">A <see cref="ManualResetEvent"/> to signal when timed out.</param>
        public TaskWaitInfo(
            long timesToWaitInMs,
            ManualResetEvent resetEvent = null)
        {
            this.requestedDelayTime = timesToWaitInMs;
            this.regTime = DateTime.Now.Ticks;
            this.EndTimeTick = this.regTime + (timesToWaitInMs * TimeSpan.TicksPerMillisecond);
            this.TaskResetEvent = resetEvent;
        }

        public void OnTimedOut(long curTick)
        {
            if (this.TaskResetEvent != null)
            {
                this.TaskResetEvent.Set();
            }
        }

        public int CompareTo(object obj)
        {
            return (EndTimeTick <= (obj as TaskWaitInfo).EndTimeTick) ? -1 : 1;
        }
    }
}
