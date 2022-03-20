// ----------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------

using System;

namespace TestThreading.CosmosDB.TestRunners
{
    /// <summary>
    /// The capability status for the assigned plan
    /// </summary>
    public enum CapabilityStatus
    {
        /// <summary>
        /// Non parsable Capability Status type
        /// </summary>
        None,

        /// <summary>
        /// Enabled Service Plan
        /// </summary>
        Enabled,

        /// <summary>
        /// Warning Service Plan
        /// </summary>
        Warning,

        /// <summary>
        /// Suspend Service Plan
        /// </summary>
        Suspended,

        /// <summary>
        /// Deleted Service Plan
        /// </summary>
        Deleted,

        /// <summary>
        /// LockedOut Service Plan
        /// </summary>
        LockedOut
    }

    /// <summary>
    /// Assigned Plan for a tenant
    /// https://docs.microsoft.com/en-us/graph/api/resources/assignedplan?view=graph-rest-1.0
    /// </summary>
    public class AssignedPlan
    {
        /// <summary>
        /// Gets or sets the data time the plan was assigned
        /// </summary>
        public DateTime AssignedTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the capability status for the plan
        /// </summary>
        public CapabilityStatus CapabilityStatus { get; set; }

        /// <summary>
        /// Gets or sets the name of the service
        /// </summary>
        public string Service { get; set; }

        /// <summary>
        /// Gets or sets the service plan Id
        /// </summary>
        public Guid ServicePlanId { get; set; }
    }
}
