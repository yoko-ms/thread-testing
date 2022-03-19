// ----------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace TestThreading.CosmosDB
{
    /// <summary>
    /// Represent the RU borrowing state for throttled Cosmos DB calls.
    /// </summary>
    public class CosmosDbRequestRetryState
    {
        /// <summary>
        /// The index
        /// </summary>
        private int regionSequenceIndex = -1;

        /// <summary>
        /// The exception that triggers current retry
        /// </summary>
        private Exception retryException;

        /// <summary>
        /// The region sequence list to use.
        /// </summary>
        private List<string> regionSequenceList;

        /// <summary>
        /// Gets or sets the attempt
        /// </summary>
        public ushort Attempt { get; set; }

        /// <summary>
        /// Gets a value indicating whether the region sequence list exists.
        /// </summary>
        public bool HasRegionList => this.regionSequenceList != null;

        /// <summary>
        /// Sets the region list to use for retry.
        /// </summary>
        /// <param name="regionList">List of regions.</param>
        public void SetRegionSequenceList(List<string> regionList)
        {
            this.regionSequenceList = new List<string>(regionList);
        }

        /// <summary>
        /// Gets the current region
        /// </summary>
        public string CurrentRegion
        {
            get
            {
                this.CheckWhetherExhaustedAllRegions();

                return this.regionSequenceList[this.regionSequenceIndex];
            }
        }

        /// <summary>
        /// Will retry in next region.
        /// </summary>
        /// <param name="ex">The exception to retry</param>
        public void WillRetryNextRegion(Exception ex)
        {
            this.regionSequenceIndex++;
            this.Attempt++;
            this.retryException = ex;
            this.CheckWhetherExhaustedAllRegions();
        }

        /// <summary>
        /// Will retry in current region.
        /// </summary>
        /// <param name="ex">The exception to retry</param>
        public void WillRetry(Exception ex)
        {
            this.Attempt++;
            this.retryException = ex;
        }

        /// <summary>
        /// Gets a value indicating whether retry in primary region
        /// </summary>
        public bool IsPrimaryRegion
        {
            get
            {
                if (this.regionSequenceIndex == -1)
                {
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Check index out of range
        /// </summary>
        private void CheckWhetherExhaustedAllRegions()
        {
            if (this.regionSequenceIndex >= this.regionSequenceList.Count)
            {
                throw new CosmosDbStorageException("All regions tried", new IndexOutOfRangeException("Region is out of index"));
            }
        }
    }
}
