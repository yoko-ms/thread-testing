// ----------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------

using System.Collections.Generic;
using System.Runtime.Serialization;

namespace TestThreading.CosmosDB.TestRunners
{
    /// <summary>
    ///     Provides properties and methods to retrieve and represent details about the tenant, such as its unique identifier, domain names, etc.
    /// </summary>
    public class Tenant
    {
        /// <summary>
        ///     Gets or sets the domain names of this tenant.
        /// </summary>
        private ICollection<string> verifiedDomainNames = new List<string>();

        /// <summary>
        ///     Gets or sets the assigned plans for the tenant.
        /// </summary>
        private ICollection<AssignedPlan> assignedPlans = new List<AssignedPlan>();

        /// <summary>
        ///     Gets the unique identifier of the tenant.
        /// </summary>
        private string objectId;

        /// <summary>
        ///  Gets or sets the unique identifier of the tenant.
        /// </summary>
        [DataMember]
        public string ObjectId
        {
            get { return this.objectId; }
            set { this.objectId = value; }
        }

        /// <summary>
        ///     Gets or sets the initial domain name of the tenant.
        /// </summary>
        [DataMember]
        public string InitialDomainName { get; set; }

        /// <summary>
        ///     Gets the tenant's domain names.
        /// </summary>
        [DataMember]
        public ICollection<string> VerifiedDomainNames
        {
            get { return this.verifiedDomainNames; }
        }

        /// <summary>
        ///     Gets or sets the tenant's country code.
        /// </summary>
        [DataMember]
        public string CountryCode { get; set; }

        /// <summary>
        ///     Gets or sets the tenant's display name.
        /// </summary>
        [DataMember]
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets the tenant's assigned plan
        /// </summary>
        [DataMember]
        public ICollection<AssignedPlan> AssignedPlans
        {
            get { return this.assignedPlans; }
        }
    }
}