// ----------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------

using System;
using Newtonsoft.Json;

namespace TestThreading.CosmosDB.TestRunners
{
    /// <summary>
    /// Tenant Location data
    /// </summary>
    public class TenantDetails
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TenantDetails"/> class
        /// </summary>
        /// <param name="tenantId">Tenant Id</param>
        /// <param name="tenantLocation">Tenant Location</param>
        public TenantDetails(Guid tenantId, string tenantLocation)
        {
            this.TenantId = tenantId;
            this.TenantLocation = tenantLocation;
            this.Id = tenantId.ToString().ToUpperInvariant();
        }

        /// <summary>
        /// Gets or sets Tenant Id
        /// </summary>
        public Guid TenantId { get; set; }

        /// <summary>
        /// Gets or sets Tenant Location
        /// </summary>
        public string TenantLocation { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the tenant.
        /// </summary>
        /// <remart>Comsos DB .NET V3 SDK requires id field to add and item into a container.</remart>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
    }
}
